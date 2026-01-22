namespace Content.Server.Roles;

[ByRefEvent]
public struct EnsureOwnerEvent
{
    public EntityUid? OwnerMob;
    public EntityUid Familiar;
    public EntityUid? OwnerItem;

    public EnsureOwnerEvent(EntityUid familiar, EntityUid? owner = null, EntityUid? ownerItem = null)
    {
        Familiar = familiar;
        OwnerMob = owner;
        OwnerItem = ownerItem;
    }
}
