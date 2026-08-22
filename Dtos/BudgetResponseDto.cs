namespace BudgetApp.Dtos;

public class BudgetResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UserId { get; set; }
}