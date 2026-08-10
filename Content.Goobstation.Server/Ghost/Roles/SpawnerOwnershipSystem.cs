using Content.Goobstation.Common.Mind;
using Content.Server._Goobstation.Wizard.Store;
using Content.Server.CharacterInfo;
using Content.Server.Ghost.Roles.Events;
using Content.Shared.CharacterInfo;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind.Components;

namespace Content.Goobstation.Server.Ghost.Roles;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpawnerOwnershipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostRoleMobSpawnerComponent, ItemPurchasedEvent>(OnPurchased);
        // 2. Спавнер превращается в моба (Кота)
        SubscribeLocalEvent<GhostRoleSpawnerUsedEvent>(OnGhostRoleSpawned);

        // 3. Моб (Кот) открывает меню персонажа
        SubscribeLocalEvent<HasOwnersComponent, GetCharacterRelationsEvent>(OnGetRelations);
    }

    /// <summary>
    /// Записывает игрока как Овнера в само радио, когда он берет его в руку.
    /// Позволяет радио "помнить" хозяина, даже если его скинут на пол.
    /// </summary>
    private void OnPurchased(EntityUid uid, GhostRoleMobSpawnerComponent comp, ItemPurchasedEvent args)
    {
        var owners = EnsureComp<HasOwnersComponent>(uid);
        owners.OwnersMob.Add(args.Buyer);
    }

    /// <summary>
    /// Шаг 2: Переносим владельца со спавнера (Радио) на заспавненного моба (Кота).
    /// </summary>
    private void OnGhostRoleSpawned(GhostRoleSpawnerUsedEvent args)
    {
        if (!TryComp<HasOwnersComponent>(args.Spawner, out var hasOwner))
            return;

        var owner = EnsureComp<HasOwnersComponent>(args.Spawned);
        owner.OwnersMob.AddRange(hasOwner.OwnersMob);
    }

    /// <summary>
    /// Шаг 3: Отвечаем на запрос меню персонажа, используя новую архитектуру CharacterRelationInfo.
    /// </summary>
    private void OnGetRelations(EntityUid uid, HasOwnersComponent comp, GetCharacterRelationsEvent args)
    {
        if (comp.OwnersMob.Count <= 0)
            return;
        foreach (var ownerUid in comp.OwnersMob)
        {
            if (!Exists(ownerUid))
                return;
            args.RelationsInfo.Add(new CharacterRelationInfo(MetaData(ownerUid).EntityName, null, CharacterRelationType.Owner));
        }
    }
}
