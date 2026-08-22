namespace BudgetApp.Models;

public class Category
{
    // Basic category information
    public int Id { get; private set; }
    public string Name { get; private set; }

    // Constructor
    public Category(string name)
    {
        Id = 0;
        Name = name;
    }

    // EF Core needs a way to construct this object when reading rows back from the database
    private Category() { }

    public void UpdateName(string name)
    {
        Name = name;
    }
}