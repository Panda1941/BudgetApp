namespace BudgetApp.Models;

public class User
{
    // Basic user information
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string PasswordHash { get; private set; }

    // User financial data
    private List<Account> Accounts { get; } = new List<Account>();

    public Budget Budget { get; private set; }

    // Constructor
    public User(string name, string passwordHash)
    {
        Id = 0;
        Name = name;
        PasswordHash = passwordHash;

        Budget = new Budget("Example Budget", this);
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private User() { }

    // Methods
    public void AddAccount(Account account)
    {
        Accounts.Add(account);
    }

    public IReadOnlyList<Account> GetAccounts()
    {
        return Accounts.AsReadOnly();
    }

    public Account? GetAccountById(int id)
    {
        return Accounts.FirstOrDefault(a => a.Id == id);
    }
}