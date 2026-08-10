using Content.Goobstation.Common.Mind;
using Content.Server._Goobstation.Wizard.Store;
using Content.Server.CharacterInfo;
using Content.Server.Chat.Managers;
using Content.Server.Ghost.Roles.Events;
using Content.Shared.Chat;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Roles;
using Robust.Server.Player;

namespace Content.Goobstation.Server.Ghost.Roles;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpawnerOwnershipSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostRoleMobSpawnerComponent, ItemPurchasedEvent>(OnPurchased);
        SubscribeLocalEvent<GhostRoleSpawnerUsedEvent>(OnGhostRoleSpawned);
        SubscribeLocalEvent<HasOwnersComponent, GetCharacterRelationsEvent>(OnGetRelations);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAdded);
    }

    private void OnRoleAdded(RoleAddedEvent args)
    {
        if (args.Mind.OwnedEntity is not { } body || !TryComp<HasOwnersComponent>(body, out var hasOwners) ||
            hasOwners.OwnersMob.Count == 0 || !_playerManager.TryGetSessionById(args.Mind.UserId, out var session))
            return;

        var ownerNames = string.Empty;
        foreach (var owner in hasOwners.OwnersMob)
        {
            var entity = GetEntity(owner);
            if (!Exists(entity))
                continue;

            if (ownerNames.Length > 0)
                ownerNames += ", ";

            ownerNames += MetaData(entity).EntityName;
        }

        var briefing = Loc.GetString("roles-antag-familiar-briefing-owner", ("ownersCount", hasOwners.OwnersMob.Count), ("owner", ownerNames));
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", briefing));
        _chat.ChatMessageToOne(ChatChannel.Server, briefing, wrappedMessage, default, false, session.Channel); // role briefing system is the fucking shit
    }

    private void OnPurchased(EntityUid uid, GhostRoleMobSpawnerComponent comp, ItemPurchasedEvent args)
    {
        var owners = EnsureComp<HasOwnersComponent>(uid);
        owners.OwnersMob.Add(GetNetEntity(args.Buyer));

        EnsureComp<IsOwnerComponent>(args.Buyer);
    }

    private void OnGhostRoleSpawned(GhostRoleSpawnerUsedEvent args)
    {
        if (!TryComp<HasOwnersComponent>(args.Spawner, out var hasOwner))
            return;

        var owner = EnsureComp<HasOwnersComponent>(args.Spawned);
        owner.OwnersMob.AddRange(hasOwner.OwnersMob);
        foreach (var ownerUid in hasOwner.OwnersMob)
        {
            EnsureComp<IsOwnerComponent>(GetEntity(ownerUid));
        }
    }

    private void OnGetRelations(EntityUid uid, HasOwnersComponent comp, GetCharacterRelationsEvent args)
    {
        if (comp.OwnersMob.Count <= 0)
            return;
        foreach (var ownerUid in comp.OwnersMob)
        {
            var entity = GetEntity(ownerUid);
            if (!Exists(entity))
                return;
            args.RelationsInfo.Add(new CharacterRelationInfo(MetaData(entity).EntityName, null, CharacterRelationType.Owner));
        }
    }
}
