namespace BudgetApp.Models;

public class Account
{
    // Basic account information
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Foreign key + navigation to the owning User.
    // We store a reference to the actual User object (not just its Id) so EF Core
    // can correctly wire up the relationship even before the User has a real Id.
    public int UserId { get; private set; }
    public User User { get; private set; }

    // Account financial data
    private List<FinancialEvent> FinancialEvents { get; } = new List<FinancialEvent>();  // TODO: Implement pagination, FUTURE PAUL PROBLEM

    // Constructor
    public Account(string name, User user)
    {
        Id = 0;
        Name = name;
        User = user;
        UserId = user.Id; // EF Core re-resolves this correctly at save time via the User reference above
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private Account() { }

    // Methods
    public void AddFinancialEvent(FinancialEvent financialEvent)
    {
        FinancialEvents.Add(financialEvent);
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public IReadOnlyList<FinancialEvent> GetFinancialEvents()
    {
        return FinancialEvents.AsReadOnly();
    }

    public decimal GetBalance()
    {
        return FinancialEvents.Sum(fe => fe.Amount);
    }
}