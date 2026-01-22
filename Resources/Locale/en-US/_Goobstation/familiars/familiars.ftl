briefing-message-familiar =
    Your master is [color={$subColor}]{$owner}[/color] ([color={$subColor}]{$owner-role}[/color]{ $ownerHasTeam ->
    [true], [color={$subColor}]{$ownerTeam}[/color])
    *[other])
    }
    { $hasTeam ->
    [true] Team is [color={$subColor}]{$team}[/color]. You are bound by the ties of service. Follow and obey all instructions states from them and their Allies.
    *[other] You are bound by the ties of service. Follow and obey all instructions states from them and their Allies.
}

summon-verb = Summon creature
summon-verb-desc = Summon creature that will aid you and gain humanlike intelligence once inhabited by a soul.
summon-requested = Your creature will arrive once a willing soul comes forth.
summon-respawn-ready = {CAPITALIZE(THE(item))} surges with ethereal power. {CAPITALIZE(POSS-ADJ(item))} resident is home again.
