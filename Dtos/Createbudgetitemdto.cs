using BudgetApp.Models;

namespace BudgetApp.Dtos;

public class CreateBudgetItemDto
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public BudgetItemType Type { get; set; }
    public int CategoryId { get; set; }
    public int BudgetId { get; set; }
}