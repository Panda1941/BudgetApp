namespace BudgetApp.Models;

public enum FinancialEventType
{
    Income,
    Expense,
    Transfer
}

public class FinancialEvent
{
    // Basic event information
    public int Id { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }

    // Foreign key + navigation to the owning Account
    public int AccountId { get; private set; }
    public Account Account { get; private set; }

    public FinancialEventType Type { get; private set; }

    // Null for Transfers - a transfer isn't a spending/income category
    public int? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    // Only relevant when Type == Transfer. The account this event's money moves TO.
    public int? DestinationAccountId { get; private set; }

    // Constructor for Income/Expense
    public FinancialEvent(string description, decimal amount, DateTime date, Account account, FinancialEventType type, Category category)
    {
        if (type == FinancialEventType.Transfer)
            throw new ArgumentException("Use the transfer constructor for Transfer events.");

        Id = 0;
        Description = description;
        Amount = amount;
        Date = date;
        Account = account;
        AccountId = account.Id;
        Type = type;
        Category = category;
        CategoryId = category.Id;
    }

    // Constructor for Transfer
    public FinancialEvent(string description, decimal amount, DateTime date, Account account, Account destinationAccount)
    {
        Id = 0;
        Description = description;
        Amount = amount;
        Date = date;
        Account = account;
        AccountId = account.Id;
        Type = FinancialEventType.Transfer;
        DestinationAccountId = destinationAccount.Id;
        Category = null;
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private FinancialEvent() { }
}