using BudgetApp.Models;

namespace BudgetApp.Dtos;

public class BudgetItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public BudgetItemType Type { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}