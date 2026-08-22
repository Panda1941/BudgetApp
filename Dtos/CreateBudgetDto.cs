namespace BudgetApp.Dtos;

public class CreateBudgetDto
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UserId { get; set; }
}