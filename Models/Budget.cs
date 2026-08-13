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
    List<BudgetItem> StaticExpenses { get; }     // Total monthly expenses, shouldn't change much
    List<BudgetItem> Income { get; }     // Total monthly income 
    private decimal FreeBudget;         // Income - StaticExpenses
    
    // Constructor
    public Budget()
    {
        Id = 0;
        Name = "Example Budget";
        
        StaticExpenses = new List<BudgetItem>();
        Income = new List<BudgetItem>();
        FreeBudget = 0;
    }
    
    // Methods
    public void AddIncome(BudgetItem income)
    {
        Income.Add(income);
    }

    public void UpdateIncome(int id, BudgetItem income)
    {
        // Eventually, this will be used to adjust income
    }

    public void RemoveIncome(int id)
    {
        // Eventually, this will be used to remove income
    }
    
    public void AddStaticExpense(BudgetItem expense)
    {
        StaticExpenses.Add(expense);
    }

    public void UpdateStaticExpense(int id, BudgetItem expense)
    {
        // Eventually, this will be used to adjust static expenses
    }

    public void RemoveStaticExpense(int id)
    {
        // Eventually, this will be used to remove static expenses
    }

    public void CalculateFreeBudget()
    {
        decimal total = 0;
        
        // foreach income add to total
        
        // foreach expense subtract from total

        this.FreeBudget = total;
    }
}