using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public BudgetsController(AppDbContext context)
    {
        _context = context;
    }

    private static DateTime ToUtc(DateTime dt)
    {
        return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BudgetResponseDto>> Get(int id)
    {
        var budget = await _context.Budgets
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (budget == null)
            return NotFound();

        var responseDto = new BudgetResponseDto 
        { 
            Id = budget.Id, 
            Name = budget.Name,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            UserId = budget.UserId
        };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetResponseDto>>> GetAll()
    {
        var budgets = await _context.Budgets
            .Include(b => b.User)
            .ToListAsync();

        IEnumerable<BudgetResponseDto> responseDtos = budgets.Select(b => new BudgetResponseDto
        {
            Id = b.Id, 
            Name = b.Name,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            UserId = b.UserId
        });
        
        return Ok(responseDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<BudgetResponseDto>> Post(CreateBudgetDto budgetDto)
    {
        var user = await _context.Users.FindAsync(budgetDto.UserId);
        if (user == null)
            return BadRequest("User not found");

        // Guard: one budget per user
        var existingBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == budgetDto.UserId);
        if (existingBudget != null)
            return BadRequest("User already has a budget");

        var newBudget = new Budget(budgetDto.Name, user, ToUtc(budgetDto.StartDate), ToUtc(budgetDto.EndDate));

        _context.Budgets.Add(newBudget);
        
        await _context.SaveChangesAsync();

        var responseDto = new BudgetResponseDto 
        { 
            Id = newBudget.Id, 
            Name = newBudget.Name,
            StartDate = newBudget.StartDate,
            EndDate = newBudget.EndDate,
            UserId = newBudget.UserId
        };
        
        return CreatedAtAction(nameof(Get), new {id = newBudget.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetResponseDto>> Update(int id, CreateBudgetDto edit)
    {
        var budget = await _context.Budgets.FindAsync(id);

        if (budget == null)
            return NotFound();

        budget.UpdateDetails(edit.Name, ToUtc(edit.StartDate), ToUtc(edit.EndDate));
        
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
        
        var responseDto = new BudgetResponseDto 
        { 
            Id = budget.Id, 
            Name = budget.Name,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate,
            UserId = budget.UserId
        };
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var budget = await _context.Budgets.FindAsync(id);

        if (budget == null)
            return NotFound();
        
        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}