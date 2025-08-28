using System;

namespace CardsService.Domain.Enums;

[Flags]
public enum CardOptions
{
    None = 0,
    ATM = 1 << 0,
    MagneticStripeReader = 1 << 1,
    Contactless = 1 << 2,
    OnlinePayments = 1 << 3,
    AllowChangingSettings = 1 << 4,
    AllowPlasticOrder = 1 << 5
}
