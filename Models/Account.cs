namespace BudgetApp.Models;

public class Account
{
    // Basic account information
    public int Id { get; private set; }
    public string Name { get; private set; }
    
    // Account financial data
    private List<FinancialEvent> FinancialEvents { get; }  // The get would return everything. TODO: Implement pagination, FUTURE PAUL PROBLEM
    
    // Constructor
    public Account(string name)
    {
        Id = 0;
        Name = name;

        FinancialEvents = new List<FinancialEvent>();
    }
    
    // Methods
    public void AddFinancialEvent(FinancialEvent financialEvent)
    {
        FinancialEvents.Add(financialEvent);
    }
}