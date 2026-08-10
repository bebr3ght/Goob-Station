using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Mind;

[Prototype]
public sealed partial class CharacterRelationGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
