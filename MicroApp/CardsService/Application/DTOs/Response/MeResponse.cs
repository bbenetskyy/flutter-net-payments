namespace CardsService.Application.DTOs;

internal sealed class MeResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public long EffectivePermissions { get; set; }
}
