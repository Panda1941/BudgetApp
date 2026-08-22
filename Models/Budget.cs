namespace BudgetApp.Models;

public class Budget
{
    // Basic budget information
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Budget timeframe
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    // Foreign key + navigation to the owning User
    public int UserId { get; private set; }
    public User User { get; private set; }

    // All budget items (income, static expenses, variable expenses) live in one
    // backing collection, distinguished by BudgetItem.Type - mirrors one table in the DB.
    private List<BudgetItem> Items { get; } = new List<BudgetItem>();

    private decimal _freeBudget; // Income - (StaticExpenses + VariableExpenses)

    // Constructor
    public Budget(string name, User user)
    {
        Id = 0;
        Name = name;
        User = user;
        UserId = user.Id;
        StartDate = DateTime.UtcNow;
        EndDate = DateTime.UtcNow.AddMonths(1);
        _freeBudget = 0;
    }

    public Budget(string name, User user, DateTime startDate, DateTime endDate)
    {
        Id = 0;
        Name = name;
        User = user;
        UserId = user.Id;
        StartDate = startDate;
        EndDate = endDate;
        _freeBudget = 0;
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private Budget() { }

    // Methods
    public void AddItem(BudgetItem item)
    {
        Items.Add(item);
    }

    public void UpdateItem(int id, BudgetItem item)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to adjust any income/expense item
    }

    public void RemoveItem(int id)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to remove any income/expense item
    }

    public void UpdateDetails(string name, DateTime startDate, DateTime endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    // Filtered read-only views over the single backing list
    public IReadOnlyList<BudgetItem> GetIncome() =>
        Items.Where(i => i.Type == BudgetItemType.Income).ToList().AsReadOnly();

    public IReadOnlyList<BudgetItem> GetStaticExpenses() =>
        Items.Where(i => i.Type == BudgetItemType.StaticExpense).ToList().AsReadOnly();

    public IReadOnlyList<BudgetItem> GetVariableExpenses() =>
        Items.Where(i => i.Type == BudgetItemType.VariableExpense).ToList().AsReadOnly();

    public void CalculateFreeBudget()
    {
        throw new NotImplementedException();
        // Eventually:
        // decimal total = 0;
        // foreach income add to total
        // foreach expense (static + variable) subtract from total
        // _freeBudget = total;
    }
}