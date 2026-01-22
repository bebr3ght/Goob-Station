using Content.Server.Antag;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.NPC.Components;
using Content.Server.Roles;
using Content.Shared.Bible.Components;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Familiar;

public sealed class FamiliarSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<FamiliarComponent, MindGotAddedEvent>(OnFamiliarInit);
        // SubscribeLocalEvent<FamiliarComponent, MobStateChangedEvent>(OnFamiliarDeath);
        // SubscribeLocalEvent<FamiliarComponent, ComponentShutdown>(OnFamiliarDeath);

        // SubscribeLocalEvent<EnsureOwnerEvent>(OnEnsureOwner);

        SubscribeLocalEvent<FamiliarRoleComponent, GetBriefingEvent>(OnGetBrief);

        // SubscribeLocalEvent<GhostRoleMobSpawnerComponent, ComponentShutdown>(OnComponentShutdown);
    }

    // private void OnFamiliarInit(EntityUid uid, FamiliarComponent component, MindGotAddedEvent args)
    // {
    //     var briefing = GetBriefingText(args.Mind.Owner);
    //
    //     _roleSystem.MindHasRole<FamiliarRoleComponent>(args.Mind.Owner, out var familiarRole);
    //     if (familiarRole is not null)
    //         EnsureComp<RoleBriefingComponent>(familiarRole.Value.Owner);
    //
    //     _antag.SendBriefing(uid, briefing, Color.Orange, null);
    // }

    // private void OnEnsureOwner(ref EnsureOwnerEvent args)
    // {
    //     _ghostRole.SetSpawnedFrom(args.Familiar, args.OwnerMob, args.OwnerItem);
    // }

    private void OnGetBrief(Entity<FamiliarRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind;

        var briefing = GetBriefingText(ent);
        args.Briefing = briefing;
    }

    private string GetBriefingText(EntityUid mind)
    {
        if (!TryComp<MindComponent>(mind, out var mindComponent))
            return string.Empty;

        var ownedEntity = mindComponent.OwnedEntity;
        if (ownedEntity == null)
            return string.Empty;

        EntityUid? owner = new();

        var color = Color.CornflowerBlue;
        if (_proto.TryIndex(mindComponent.RoleType, out var proto))
            color = proto.Color;

        if (TryComp<HasOwnerComponent>(ownedEntity, out var hasOwner)
            && hasOwner.OwnerMob != null)
        {
            owner = hasOwner.OwnerMob;
            if (hasOwner.RandomizeOwner &&
                hasOwner.PossibleOwners.Count > 0)
                owner = _random.Pick(hasOwner.PossibleOwners);
        }

        if (owner == null)
            return string.Empty;

        if (!TryComp<MindContainerComponent>(owner.Value, out var mindContainer) ||
            mindContainer.Mind == null ||
            !TryComp<MindComponent>(mindContainer.Mind.Value, out var masterMind))
            return string.Empty;

        var ownerName = MetaData(owner.Value).EntityName;
        var ownerRole = _jobs.MindTryGetJobName(mindContainer.Mind.Value); // maybe should use current owner's id card job
        var ownerSubtype = string.Empty;
        var mindSubtype = string.Empty;

        if (!string.IsNullOrEmpty(mindComponent.Subtype))
            mindSubtype = Loc.GetString($"{mindComponent.Subtype}");
        if (!string.IsNullOrEmpty(masterMind.Subtype))
            ownerSubtype = Loc.GetString($"{masterMind.Subtype}");

        var roleBriefing = Loc.GetString("briefing-message-familiar",
            ("owner", ownerName),
            ("owner-role", ownerRole),
            ("hasTeam", !string.IsNullOrEmpty(mindSubtype)),
            ("ownerHasTeam", !string.IsNullOrEmpty(ownerSubtype)),
            ("subColor", color),
            ("team", mindSubtype),
            ("ownerTeam", ownerSubtype));

        return roleBriefing;
    }

    // /// <summary>
    // /// Similar with MobStateChangedEvent, but when familiar disappears.
    // /// </summary>
    // /// <param name="uid"></param>
    // /// <param name="component"></param>
    // /// <param name="args"></param>
    // private void OnFamiliarDeath(EntityUid uid, FamiliarComponent component, ComponentShutdown args)
    // {
    //     if (!TryComp<HasOwnerComponent>(uid, out var hasOwner))
    //         return;
    //     if (hasOwner.OwnerItem == null || !hasOwner.HandleOwnerItem)
    //         return;
    //
    //     var item = hasOwner.OwnerItem;
    //     EnsureComp<SpawnCooldownComponent>(item.Value);
    // }

    // /// <summary>
    // /// Starts up the respawn stuff when
    // /// the familiar dies.
    // /// </summary>
    // private void OnFamiliarDeath(EntityUid uid, FamiliarComponent component, MobStateChangedEvent args)
    // {
    //     if (!TryComp<HasOwnerComponent>(uid, out var hasOwner))
    //         return;
    //     if (args.NewMobState != MobState.Dead || hasOwner.OwnerItem == null || !hasOwner.HandleOwnerItem)
    //         return;
    //
    //     var item = hasOwner.OwnerItem;
    //     EnsureComp<SpawnCooldownComponent>(item.Value);
    // }

    // /// <summary>
    // /// Delete summoned creature when item that spawned it was deleted.
    // /// </summary>
    // /// <param name="uid"></param>
    // /// <param name="component"></param>
    // /// <param name="args"></param>
    // private void OnComponentShutdown(EntityUid uid, GhostRoleMobSpawnerComponent component, ComponentShutdown args)
    // {
    //     var spawnedEntity = component.SpawnedEntity;
    //
    //     if (!component.AlreadySummoned || spawnedEntity == null)
    //         return;
    //     if (!TryComp<HasOwnerComponent>(spawnedEntity, out var hasOwner) ||
    //         !hasOwner.HandleOwnerItem)
    //         return;
    //
    //     Del(spawnedEntity);
    //     component.AlreadySummoned = false;
    // }
}
