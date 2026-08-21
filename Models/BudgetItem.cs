namespace BudgetApp.Models;

public enum BudgetItemType
{
    Income,
    StaticExpense,
    VariableExpense
}

public class BudgetItem
{
    // Basic budget item information
    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public BudgetItemType Type { get; private set; }

    // Foreign key + navigation to Category
    public int CategoryId { get; private set; }
    public Category Category { get; private set; }

    // Foreign key + navigation to the owning Budget
    public int BudgetId { get; private set; }
    public Budget Budget { get; private set; }

    // Constructor
    public BudgetItem(string name, decimal amount, BudgetItemType type, Category category, Budget budget)
    {
        Id = 0;
        Name = name;
        Amount = amount;
        Type = type;
        Category = category;
        CategoryId = category.Id;
        Budget = budget;
        BudgetId = budget.Id;
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private BudgetItem() { }
}