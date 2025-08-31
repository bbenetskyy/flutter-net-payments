using System.ComponentModel.DataAnnotations;
using Common.Domain.Enums;

namespace PaymentsService.Domain.Entities;

public class Account
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    [MaxLength(64)]
    public string Iban { get; set; } = string.Empty;

    public Currency Currency { get; set; } = Currency.EUR;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
