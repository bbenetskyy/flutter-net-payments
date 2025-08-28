using CardsService.Domain.Enums;

namespace CardsService.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public CardType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SingleTransactionLimit { get; set; }
    public decimal MonthlyLimit { get; set; }
    public Guid? AssignedUserId { get; set; }
    public CardOptions Options { get; set; }
    public bool Printed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
