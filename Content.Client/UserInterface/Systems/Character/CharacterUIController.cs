// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 Justin Trotter <trotter.justin@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SpaceManiac <tad@platymuus.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.CharacterInfo;
using Content.Client.Gameplay;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.Character.Controls;
using Content.Client.UserInterface.Systems.Character.Windows;
using Content.Client.UserInterface.Systems.Objectives.Controls;
using Content.Shared.Input;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
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
// Goobstation
using Content.Shared.CharacterInfo;
using Content.Shared.Objectives;

namespace Content.Client.UserInterface.Systems.Character;

[UsedImplicitly]
public sealed class CharacterUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CharacterInfoSystem>
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [UISystemDependency] private readonly CharacterInfoSystem _characterInfo = default!;
    [UISystemDependency] private readonly SpriteSystem _sprite = default!;

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

    // Goobstation: Briefing Improve - heavily edited. Added supervisors, multiple antag roles "tabs", improved character menu UI
    private void CharacterUpdated(CharacterData data)
    {
        if (_window == null)
        {
            return;
        }

        var (entity, job, supervisors, antagRoles, entityName) = data;

        _window.SpriteView.SetEntity(entity);

        _window.CharacterMainInfoBox.RemoveAllChildren();
        var nameLabel = new Label
        {
            Text = entityName,
        };
        _window.CharacterMainInfoBox.AddChild(nameLabel);
        if (!string.IsNullOrEmpty(job))
        {
            var jobLabel = new Label
            {
                Text = "Job: " + job,
                StyleClasses = { "LabelSubText" }
            };
            _window.CharacterMainInfoBox.AddChild(jobLabel);
        }

        if (!string.IsNullOrEmpty(supervisors))
        {
            var supervisorsLabel = new Label
            {
                Text = "Supervisors: " + supervisors,
                StyleClasses = { "LabelSubText" }
            };
            _window.CharacterMainInfoBox.AddChild(supervisorsLabel);
        }

        _window.SpecialRoleInfo.RemoveAllChildren();

        var roleControls = new Dictionary<string, CharacterObjectiveControl>();
        foreach (var antagRole in antagRoles)
        {
            CharacterObjectiveControl roleControl;

            if (roleControls.TryGetValue(antagRole.RoleTitle, out var existingControl))
            {
                roleControl = existingControl;
            }
            else
            {
                roleControl = CreateAntagRoleControl(antagRole);
                _window.SpecialRoleInfo.AddChild(roleControl);
                roleControls[antagRole.RoleTitle] = roleControl;
            }

            AddObjectivesToRole(roleControl, antagRole.Objectives);
        }

        var controls = _characterInfo.GetCharacterInfoControls(entity);
        foreach (var control in controls)
        {
            _window.SpecialRoleInfo.AddChild(control);
        }

        _window.RolePlaceholder.Visible = antagRoles.Count == 0 && controls.Count == 0;
    }

    // Goobstation: Briefing Improve
    private CharacterObjectiveControl CreateAntagRoleControl(AntagRoleInfo antagRole)
    {
        var roleControl = new CharacterObjectiveControl();

        var roleTitleMsg = new FormattedMessage();
        roleTitleMsg.PushColor(antagRole.RoleTitleColor ?? antagRole.Color);
        roleTitleMsg.PushTag(new MarkupNode("bold", null, null));
        roleTitleMsg.TryAddMarkup($"[font size=15]{antagRole.RoleTitle}[/font]", out _);
        roleTitleMsg.Pop();
        roleTitleMsg.Pop();
        roleControl.RoleTitle.SetMessage(roleTitleMsg);

        var roleDetailsMsg = new FormattedMessage();
        roleDetailsMsg.PushColor(antagRole.Color);
        var detailsText = string.IsNullOrWhiteSpace(antagRole.Issuer)
            ? antagRole.RoleType
            : $"{antagRole.RoleType} | {antagRole.Issuer}";
        roleDetailsMsg.TryAddMarkup($"[font size=12]{detailsText}[/font]", out _);
        roleDetailsMsg.Pop();
        roleControl.RoleDetails.SetMessage(roleDetailsMsg);

        if (!string.IsNullOrWhiteSpace(antagRole.Briefing))
        {
            var msg = new FormattedMessage();
            msg.PushColor(antagRole.BriefingColor);

            if (antagRole.Bold)
                msg.PushTag(new MarkupNode("bold", null, null));
            msg.TryAddMarkup(antagRole.Briefing, out _);
            if (antagRole.Bold)
                msg.Pop();
            msg.Pop();

            var briefingControl = new ObjectiveBriefingControl();
            briefingControl.Label.SetMessage(msg);

            roleControl.BriefingContainer.AddChild(briefingControl);
        }

        if (antagRole.Objectives.Count > 0)
        {
            var objectivesLabel = new Label
            {
                Text = Loc.GetString("character-info-objectives-label"),
                HorizontalAlignment = Control.HAlignment.Left,
            };
            roleControl.ObjectivesContainer.AddChild(objectivesLabel);
        }

        return roleControl;
    }

    // Goobstation: Briefing Improve
    private void AddObjectivesToRole(CharacterObjectiveControl roleControl, List<ObjectiveInfo> objectives)
    {
        foreach (var condition in objectives)
        {
            var conditionControl = new ObjectiveConditionsControl();
            conditionControl.ProgressTexture.Texture = _sprite.Frame0(condition.Icon);
            conditionControl.ProgressTexture.Progress = condition.Progress;

            var titleMsg = new FormattedMessage();
            titleMsg.AddText(condition.Title);

            var descMsg = new FormattedMessage();
            descMsg.AddText(condition.Description);

            conditionControl.Title.SetMessage(titleMsg);
            conditionControl.Description.SetMessage(descMsg);

            roleControl.ObjectivesContainer.AddChild(conditionControl);
        }
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
