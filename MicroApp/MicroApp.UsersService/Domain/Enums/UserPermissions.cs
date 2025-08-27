namespace AuthService.Domain.Enums;

[Flags]
public enum UserPermissions : long
{
    None                = 0,
    ViewPayments        = 1 << 0,
    CreatePayments      = 1 << 1,
    ConfirmPayments     = 1 << 2,
    ViewUsers           = 1 << 3,
    ManageCompanyUsers  = 1 << 4,
    EditCompanyDetails  = 1 << 5,
    ViewCards           = 1 << 6,
    ManageCompanyCards  = 1 << 7,
    // резерв під майбутні права
}
