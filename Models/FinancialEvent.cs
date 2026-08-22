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

    // Links the two legs of a transfer together (both source and destination events share the same TransferPairId)
    public Guid? TransferPairId { get; private set; }

    // Indicates this event is part of a transfer (for reporting: exclude from budget/category spending stats)
    public bool IsTransfer { get; private set; }

    // Constructor for Income/Expense
    public FinancialEvent(string description, decimal amount, DateTime date, Account account, FinancialEventType type, Category category)
    {
        if (type == FinancialEventType.Transfer)
            throw new ArgumentException("Use CreateTransferPair for Transfer events.");
        
        Id = 0;
        Description = description;
        Amount = type == FinancialEventType.Expense ? -Math.Abs(amount) : Math.Abs(amount);
        Date = date;
        Account = account;
        AccountId = account.Id;
        Type = type;
        Category = category;
        CategoryId = category.Id;
        TransferPairId = null;
        IsTransfer = false;
    }

    // Constructor for Transfer (source account leg - amount should be negative)
    public static (FinancialEvent SourceEvent, FinancialEvent DestinationEvent) CreateTransferPair(
        string description,
        decimal amount,
        DateTime date,
        Account sourceAccount,
        Account destinationAccount)
    {
        var transferPairId = Guid.NewGuid();

        var sourceEvent = new FinancialEvent
        {
            Id = 0,
            Description = description,
            Amount = -Math.Abs(amount),
            Date = date,
            Account = sourceAccount,
            AccountId = sourceAccount.Id,
            Type = FinancialEventType.Transfer,
            Category = null,
            CategoryId = null,
            TransferPairId = transferPairId,
            IsTransfer = true
        };

        var destinationEvent = new FinancialEvent
        {
            Id = 0,
            Description = description,
            Amount = Math.Abs(amount),
            Date = date,
            Account = destinationAccount,
            AccountId = destinationAccount.Id,
            Type = FinancialEventType.Transfer,
            Category = null,
            CategoryId = null,
            TransferPairId = transferPairId,
            IsTransfer = true
        };

        return (sourceEvent, destinationEvent);
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private FinancialEvent() { }

    public void UpdateDetails(string description, decimal amount, DateTime date, FinancialEventType type, Category? category = null, Guid? transferPairId = null, bool isTransfer = false)
    {
        Description = description;
        Amount = type == FinancialEventType.Expense ? -Math.Abs(amount) : Math.Abs(amount);
        Date = date;
        Type = type;

        if (type == FinancialEventType.Transfer)
        {
            Category = null;
            CategoryId = null;
            TransferPairId = transferPairId;
            IsTransfer = isTransfer;
        }
        else
        {
            Category = category;
            CategoryId = category?.Id;
            TransferPairId = null;
            IsTransfer = false;
        }
    }
}