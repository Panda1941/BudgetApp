namespace BudgetApp.Models;

public class Account
{
    // Basic account information
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Account financial data
    private List<FinancialEvent> FinancialEvents { get; }  // TODO: Implement pagination, FUTURE PAUL PROBLEM

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

    public IReadOnlyList<FinancialEvent> GetFinancialEvents()
    {
        return FinancialEvents.AsReadOnly();
    }

    public decimal GetBalance()
    {
        throw new NotImplementedException();
        // Eventually: sum Income/Expense events belonging to this account,
        // plus/minus Transfer events depending on whether this account
        // is the source or the DestinationAccountId.
    }
}