namespace BudgetApp.Models;

public class User
{
    // Basic user information
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string PasswordHash { get; private set; }
    
    // User financial data
    private List<Account> Accounts { get; }
    
    public Budget Budget { get; private set; }
    
    // Constructor
    public User(string name, string passwordHash)
    {
        Id = 0;
        Name = name;
        PasswordHash = passwordHash;

        Accounts = new List<Account>();
        Budget = new Budget();
    }
    
    // Methods
    public void AddAccount(Account account)
    {
        Accounts.Add(account);
    }
}