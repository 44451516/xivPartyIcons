namespace PartyIcons.Entities;

public enum RoleId
{
    Undefined,
    MT = 1,
    OT,
    D1,
    D2,
    D3,
    D4,
    H1,
    H2
}

public static class RoleIdUtils
{
    public static RoleId Counterpart(RoleId roleId)
    {
        return roleId switch
        {
            RoleId.MT => RoleId.OT,
            RoleId.OT => RoleId.MT,
            RoleId.H1 => RoleId.H2,
            RoleId.H2 => RoleId.H1,
            RoleId.D1 => RoleId.D2,
            RoleId.D2 => RoleId.D1,
            RoleId.D3 => RoleId.D4,
            RoleId.D4 => RoleId.D3,
            _ => RoleId.Undefined
        };
    }
}
