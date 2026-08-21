using BudgetApp.Models;

namespace BudgetApp.Dtos;

public class CreateFinancialEventDto
{
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int AccountId { get; set; }
    public FinancialEventType Type { get; set; }

    // Required for Income/Expense, must be null for Transfer
    public int? CategoryId { get; set; }

    // Required for Transfer, must be null for Income/Expense
    public int? DestinationAccountId { get; set; }
}