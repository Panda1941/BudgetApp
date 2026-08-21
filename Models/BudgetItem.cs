namespace BudgetApp.Models;

public class BudgetItem
{
    // Basic budget item information
    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public Category Category { get; private set; }

    // Constructor
    public BudgetItem(string name, decimal amount, Category category)
    {
        Id = 0;
        Name = name;
        Amount = amount;
        Category = category;
    }
}