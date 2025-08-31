using System.ComponentModel.DataAnnotations;

namespace WalletService.Domain.Entities;

public class Wallet
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
