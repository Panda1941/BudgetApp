namespace BudgetApp.Models;

public class Budget
{
    // Basic budget information
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Budget timeframe
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    // Budget information
    private List<BudgetItem> StaticExpenses { get; }    // Fixed monthly expenses (e.g. rent) - shouldn't change much
    private List<BudgetItem> VariableExpenses { get; }  // Categorized expenses that fluctuate month to month (e.g. food, gas)
    private List<BudgetItem> Income { get; }             // Total monthly income
    private decimal _freeBudget;                         // Income - (StaticExpenses + VariableExpenses)

    // Constructor
    public Budget()
    {
        Id = 0;
        Name = "Example Budget";

        StaticExpenses = new List<BudgetItem>();
        VariableExpenses = new List<BudgetItem>();
        Income = new List<BudgetItem>();
        _freeBudget = 0;
    }

    // Methods - Income
    public void AddIncome(BudgetItem income)
    {
        Income.Add(income);
    }

    public void UpdateIncome(int id, BudgetItem income)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to adjust income
    }

    public void RemoveIncome(int id)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to remove income
    }

    // Methods - Static (fixed) expenses
    public void AddStaticExpense(BudgetItem expense)
    {
        StaticExpenses.Add(expense);
    }

    public void UpdateStaticExpense(int id, BudgetItem expense)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to adjust static expenses
    }

    public void RemoveStaticExpense(int id)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to remove static expenses
    }

    // Methods - Variable (categorized) expenses
    public void AddVariableExpense(BudgetItem expense)
    {
        VariableExpenses.Add(expense);
    }

    public void UpdateVariableExpense(int id, BudgetItem expense)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to adjust variable expenses
    }

    public void RemoveVariableExpense(int id)
    {
        throw new NotImplementedException();
        // Eventually, this will be used to remove variable expenses
    }

    // Read-only accessors
    public IReadOnlyList<BudgetItem> GetIncome() => Income.AsReadOnly();
    public IReadOnlyList<BudgetItem> GetStaticExpenses() => StaticExpenses.AsReadOnly();
    public IReadOnlyList<BudgetItem> GetVariableExpenses() => VariableExpenses.AsReadOnly();

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