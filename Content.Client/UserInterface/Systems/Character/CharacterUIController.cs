// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Client.CharacterInfo;
using Content.Client.Gameplay;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Character.Controls;
using Content.Client.UserInterface.Systems.Character.Windows;
using Content.Client.UserInterface.Systems.Objectives.Controls;
using Content.Goobstation.Common.Mind;
using Content.Goobstation.Shared.Supermatter.Components;
using Content.Shared.CharacterInfo;
using Content.Shared.Input;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Content.Client.CharacterInfo.CharacterInfoSystem;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.UserInterface.Systems.Character;

[UsedImplicitly]
public sealed class CharacterUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CharacterInfoSystem>
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [UISystemDependency] private readonly CharacterInfoSystem _characterInfo = default!;
    [UISystemDependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<MindRoleTypeChangedEvent>(OnRoleTypeChanged);
    }

    private CharacterWindow? _window;
    private MenuButton? CharacterButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.CharacterButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<CharacterWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenCharacterMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CharacterUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<CharacterUIController>();
    }

    public void OnSystemLoaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate += CharacterUpdated;
        _player.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(CharacterInfoSystem system)
    {
        system.OnCharacterUpdate -= CharacterUpdated;
        _player.LocalPlayerDetached -= CharacterDetached;
    }

    public void UnloadButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.OnPressed -= CharacterButtonPressed;
    }

    public void LoadButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.OnPressed += CharacterButtonPressed;
    }

    private void DeactivateButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (CharacterButton == null)
        {
            return;
        }

        CharacterButton.Pressed = true;
    }

    private void CharacterUpdated(CharacterData data)
    {
        if (_window == null)
        {
            return;
        }

        var (entity, job, allegiance, relationsInfo, objectives, briefing, entityName) = data;

        _window.SpriteView.SetEntity(entity);

        UpdateRoleType();

        _window.NameLabel.Text = entityName;
        _window.JobLabel.Text = job;
        _window.Objectives.RemoveAllChildren();
        _window.OwnersList.RemoveAllChildren();
        _window.OwnersLabel.Visible = false;
        _window.AllegianceLabel.Text = null;
        _window.ObjectivesLabel.Visible = objectives.Any();

        for (var i = _window.SubInfo.ChildCount - 1; i >= 0; i--)
        {
            var child = _window.SubInfo.GetChild(i);
            if (child is TextureRect)
            {
                _window.SubInfo.RemoveChild(child);
            }
        }

        foreach (var (groupId, conditions) in objectives)
        {
            var objectiveControl = new CharacterObjectiveControl
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                Modulate = Color.Gray
            };


            var objectiveText = new FormattedMessage();
            objectiveText.TryAddMarkup(groupId, out _);

            var objectiveLabel = new RichTextLabel
            {
                StyleClasses = { StyleClass.TooltipTitle }
            };
            objectiveLabel.SetMessage(objectiveText);

            objectiveControl.AddChild(objectiveLabel);

            foreach (var condition in conditions)
            {
                var conditionControl = new ObjectiveConditionsControl();
                conditionControl.ProgressTexture.Texture = _sprite.Frame0(condition.Icon);
                conditionControl.ProgressTexture.Progress = condition.Progress;
                var titleMessage = new FormattedMessage();
                var descriptionMessage = new FormattedMessage();
                titleMessage.AddText(condition.Title);
                descriptionMessage.AddText(condition.Description);

                conditionControl.Title.SetMessage(titleMessage);
                conditionControl.Description.SetMessage(descriptionMessage);

                objectiveControl.AddChild(conditionControl);
            }

            _window.Objectives.AddChild(objectiveControl);
        }

        if (briefing != null)
        {
            var briefingControl = new ObjectiveBriefingControl();
            var text = new FormattedMessage();
            text.PushColor(Color.Yellow);
            text.AddText(briefing);
            briefingControl.Label.SetMessage(text);
            _window.Objectives.AddChild(briefingControl);
        }

        if (allegiance != null)
            _window.AllegianceLabel.Text = $"Allegiance: {Loc.GetString(allegiance)}";

        if (relationsInfo != null && relationsInfo.Count > 0)
        {
            var groupedRelations = new Dictionary<CharacterRelationType, List<CharacterRelationInfo>>();

            foreach (var rel in relationsInfo)
            {
                var type = rel.RelationType ?? CharacterRelationType.Colleague;

                if (!groupedRelations.TryGetValue(type, out var list))
                {
                    list = new List<CharacterRelationInfo>();
                    groupedRelations[type] = list;
                }

                list.Add(rel);
            }

            CharacterRelationType[] renderOrder =
            {
                CharacterRelationType.Owner,
                CharacterRelationType.Commander,
                CharacterRelationType.Colleague
            };

            foreach (var type in renderOrder)
            {
                if (!groupedRelations.TryGetValue(type, out var list) || list.Count == 0)
                    continue;

                if (list.Count == 1)
                {
                    var rel = list[0];
                    var titleDisplay = string.IsNullOrEmpty(rel.Title) ? "" : $" ({rel.Title})";
                    var content = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };

                    var prefix = type switch
                    {
                        CharacterRelationType.Owner => "Owner: ",
                        CharacterRelationType.Commander => "Commander: ",
                        CharacterRelationType.Colleague => "Colleague: ",
                        _ => "Relation:"
                    };

                    var prefixLabel = new Label
                    {
                        Text = prefix,
                        StyleClasses = { "LabelSubText" },
                    };
                    content.AddChild(prefixLabel);

                    if (rel.FactionIcon != null && _prototypeManager.TryIndex<FactionIconPrototype>(rel.FactionIcon, out var iconProto))
                    {
                        var ic = new TextureRect
                        {
                            Texture = _sprite.Frame0(iconProto.Icon),
                            TextureScale = new Vector2(2),
                            Margin = new Thickness(0, 0, 3, 0)
                        };
                        content.AddChild(ic);
                    }

                    var characterName = new Label
                    {
                        Text = $"{rel.Name}{titleDisplay}",
                        StyleClasses = { "LabelSubText" },
                    };
                    content.AddChild(characterName);
                    _window.OwnersList.AddChild(content);
                }
                else
                {
                    var headerText = type switch
                    {
                        CharacterRelationType.Owner => "Owners:",
                        CharacterRelationType.Commander => "Commanders:",
                        CharacterRelationType.Colleague => "Colleagues:",
                        _ => "Relations:"
                    };

                    var groupHeader = new Label
                    {
                        Text = headerText,
                        StyleClasses = { "LabelSubText" },
                    };
                    _window.OwnersList.AddChild(groupHeader);

                    for (var i = 0; i < list.Count; i++)
                    {
                        var rel = list[i];
                        var titleDisplay = string.IsNullOrEmpty(rel.Title) ? "" : $" ({rel.Title})";
                        var content = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };

                        var countLabel = new Label
                        {
                            Text = $"{i + 1}. ",
                            StyleClasses = { "LabelSubText" }
                        };
                        content.AddChild(countLabel);

                        if (rel.FactionIcon != null && _prototypeManager.TryIndex<FactionIconPrototype>(rel.FactionIcon, out var iconProto))
                        {
                            var ic = new TextureRect
                            {
                                Texture = _sprite.Frame0(iconProto.Icon),
                                TextureScale = new Vector2(2),
                                Margin = new Thickness(0, 0, 3, 0)
                            };
                            content.AddChild(ic);
                        }

                        var relationLabel = new Label
                        {
                            Text = $"{rel.Name}{titleDisplay}",
                            StyleClasses = { "LabelSubText" }
                        };
                        content.AddChild(relationLabel);

                        _window.OwnersList.AddChild(content);
                    }
                }
            }
        }

        var controls = _characterInfo.GetCharacterInfoControls(entity);
        foreach (var control in controls)
        {
            _window.Objectives.AddChild(control);
        }

        _window.RolePlaceholder.Visible = briefing == null && !controls.Any() && !objectives.Any();
    }

    private void OnRoleTypeChanged(MindRoleTypeChangedEvent ev, EntitySessionEventArgs _)
    {
        UpdateRoleType();
    }

    private void UpdateRoleType()
    {
        if (_window == null || !_window.IsOpen)
            return;

        if (!_ent.TryGetComponent<MindContainerComponent>(_player.LocalEntity, out var container)
            || container.Mind is null)
            return;

        if (!_ent.TryGetComponent<MindComponent>(container.Mind.Value, out var mind))
            return;

        if (!_prototypeManager.TryIndex(mind.RoleType, out var proto))
            Log.Error($"Player '{_player.LocalSession}' has invalid Role Type '{mind.RoleType}'. Displaying default instead");

        _window.RoleType.Text = Loc.GetString(proto?.Name ?? "role-type-crew-aligned-name");
        _window.RoleType.FontColorOverride = proto?.Color ?? Color.White;
    }

    private void CharacterDetached(EntityUid uid)
    {
        CloseWindow();
    }

    private void CharacterButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void CloseWindow()
    {
        _window?.Close();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        CharacterButton?.SetClickPressed(!_window.IsOpen);

        if (_window.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _characterInfo.RequestCharacterInfo();
            _window.Open();
        }
    }
}
