using Content.Goobstation.Common.Mind;
using Content.Server.CharacterInfo;

namespace Content.Goobstation.Server.Ghost.Roles;

public sealed class CharacterRelationGroupSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CharacterRelationGroupComponent, GetCharacterRelationsEvent>(OnGetRelations);
    }

    private void OnGetRelations(EntityUid uid, CharacterRelationGroupComponent comp, GetCharacterRelationsEvent args)
    {
        var query = EntityQueryEnumerator<CharacterRelationGroupComponent, MetaDataComponent>();

        while (query.MoveNext(out var otherUid, out var otherComp, out var meta))
        {
            if (otherUid == uid || otherComp.Group != comp.Group || otherComp.RelationType == CharacterRelationType.None)
                continue;

            if (comp.TeamUid != null || otherComp.TeamUid != null)
            {
                if (comp.TeamUid != otherComp.TeamUid)
                    continue;
            }

            var displayedRelation = otherComp.RelationType;
            if (comp.RelationType == CharacterRelationType.Owner && otherComp.RelationType == CharacterRelationType.Owner)
                continue;
            if (comp.RelationType == CharacterRelationType.Commander && otherComp.RelationType == CharacterRelationType.Commander)
                displayedRelation = CharacterRelationType.Colleague;

            // Если титул есть — переводим его через Loc.GetString
            var title = otherComp.Title != null ? Loc.GetString(otherComp.Title) : null;

            args.RelationsInfo.Add(new CharacterRelationInfo(meta.EntityName, title, displayedRelation, otherComp.FactionIcon));
        }
    }
}
