// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Objectives;
using Robust.Shared.Serialization;

namespace Content.Shared.CharacterInfo;

[Serializable, NetSerializable]
public sealed class RequestCharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;

    public RequestCharacterInfoEvent(NetEntity netEntity)
    {
        NetEntity = netEntity;
    }
}

// Goobstation: Briefing Improve
[Serializable, NetSerializable]
public readonly record struct AntagRoleInfo(
    string RoleTitle,
    Color? RoleTitleColor,
    string RoleType,
    string Issuer,
    string? Briefing,
    List<ObjectiveInfo> Objectives,
    Color Color,
    Color BriefingColor,
    bool Bold
);

// Goobstation: Briefing Improve - add supervisors & antagrolesinfo
[Serializable, NetSerializable]
public sealed class CharacterInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public readonly string JobTitle;
    public readonly string Supervisors;
    public readonly List<AntagRoleInfo> AntagRolesInfo;

    public CharacterInfoEvent(NetEntity netEntity, string jobTitle, string supervisors, List<AntagRoleInfo> antagRolesInfo)
    {
        NetEntity = netEntity;
        JobTitle = jobTitle;
        Supervisors = supervisors;
        AntagRolesInfo = antagRolesInfo;
    }
}
