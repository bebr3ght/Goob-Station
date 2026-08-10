using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Mind;

[RegisterComponent, NetworkedComponent]
public sealed partial class CharacterRelationGroupComponent : Component
{
    /// <summary>
    /// Имя группы (например, "Abductor" или "NukeOps"). Сущности с одинаковой группой увидят друг друга в меню.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CharacterRelationGroupPrototype>? Group = "";

    /// <summary>
    /// Локализованный титул в группе (например, "abductor-role-scientist" -> "Учёный").
    /// </summary>
    [DataField]
    public LocId? Title;

    /// <summary>
    /// Тип связи (по умолчанию - Коллега).
    /// </summary>
    [DataField]
    public CharacterRelationType RelationType = CharacterRelationType.Colleague;

    [DataField]
    public string? FactionIcon;

    [DataField]
    public EntityUid? TeamUid;
}

[Serializable, NetSerializable]
public readonly record struct CharacterRelationInfo(string Name, string? Title = null, CharacterRelationType? RelationType = null, string? FactionIcon = null);

[Serializable, NetSerializable]
public enum CharacterRelationType : byte
{
    None,
    Commander,
    Owner,
    Colleague,
}
