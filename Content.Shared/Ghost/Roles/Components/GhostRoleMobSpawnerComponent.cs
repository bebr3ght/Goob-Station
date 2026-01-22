// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take this role, spawning a new entity.
    /// </summary>
    [RegisterComponent, EntityCategory("Spawner")]
    public sealed partial class GhostRoleMobSpawnerComponent : Component
    {
        [DataField]
        public bool DeleteOnSpawn = true;

        // Goobstation - briefing for familiars - Start
        [DataField]
        public bool Repeatable
        {
            get => !DeleteOnSpawn;
            set => DeleteOnSpawn = !value;
        }

        /// <summary>
        ///     Cooldown time before the ghost role can be spawned again.
        ///     Required when Repeatable is true (DeleteOnSpawn is false).
        /// </summary>
        [DataField]
        public float? RepeatCooldown;


        /// <summary>
        /// Validates that RepeatCooldown is set when Repeatable is true.
        /// </summary>
        [MemberNotNullWhen(true, nameof(RepeatCooldown))]
        public bool ValidateRepeatCooldown()
        {
            if (Repeatable && RepeatCooldown == null)
                return false;

            RepeatCooldown ??= 0f;

            return true;
        }

        [DataField]
        public bool AlreadySummoned;

        /// <summary>
        /// The specific creature this summoned.
        /// </summary>
        [ViewVariables]
        public EntityUid? SpawnedEntity = null;

        /// <summary>
        ///     Cooldown time before the ghost role can be spawned again.
        ///     Required when Repeatable is true (DeleteOnSpawn is false).
        /// </summary>
        [DataField]
        public float? SpawnCooldown = 100f;

        [DataField]
        public bool AssignOwners;

        [ViewVariables]
        public EntityUid? OwnerItem = null;

        [DataField]
        public EntityUid? OwnerMob = null;

        [DataField]
        public ComponentRegistry RequiredComponents = new();
        // Goobstation - briefing for familiars - End

        [DataField]
        public int AvailableTakeovers = 1;

        [ViewVariables]
        public int CurrentTakeovers = 0;

        [DataField]
        public EntProtoId? Prototype;

        /// <summary>
        ///     If this ghostrole spawner has multiple selectable ghostrole prototypes.
        /// </summary>
        [DataField]
        public List<string> SelectablePrototypes = [];

    }
}
