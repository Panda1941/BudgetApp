using BudgetApp.Models;

namespace BudgetApp.Dtos;

public class FinancialEventResponseDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int AccountId { get; set; }
    public FinancialEventType Type { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public Guid? TransferPairId { get; set; }
    public bool IsTransfer { get; set; }
}