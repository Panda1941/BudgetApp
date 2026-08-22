using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public BudgetItemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BudgetItemResponseDto>> Get(int id)
    {
        var budgetItem = await _context.BudgetItems
            .Include(bi => bi.Category)
            .FirstOrDefaultAsync(bi => bi.Id == id);

        if (budgetItem == null)
            return NotFound();

        var responseDto = new BudgetItemResponseDto 
        { 
            Id = budgetItem.Id, 
            Name = budgetItem.Name,
            Amount = budgetItem.Amount,
            Type = budgetItem.Type,
            CategoryId = budgetItem.CategoryId,
            CategoryName = budgetItem.Category?.Name ?? ""
        };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetItemResponseDto>>> GetAll()
    {
        var budgetItems = await _context.BudgetItems
            .Include(bi => bi.Category)
            .ToListAsync();

        IEnumerable<BudgetItemResponseDto> responseDtos = budgetItems.Select(bi => new BudgetItemResponseDto
        {
            Id = bi.Id, 
            Name = bi.Name,
            Amount = bi.Amount,
            Type = bi.Type,
            CategoryId = bi.CategoryId,
            CategoryName = bi.Category?.Name ?? ""
        });
        
        return Ok(responseDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<BudgetItemResponseDto>> Post(CreateBudgetItemDto budgetItemDto)
    {
        var category = await _context.Categories.FindAsync(budgetItemDto.CategoryId);
        if (category == null)
            return BadRequest("Category not found");

        var budget = await _context.Budgets.FindAsync(budgetItemDto.BudgetId);
        if (budget == null)
            return BadRequest("Budget not found");

        var newBudgetItem = new BudgetItem(
            budgetItemDto.Name, 
            budgetItemDto.Amount, 
            budgetItemDto.Type, 
            category, 
            budget);

        _context.BudgetItems.Add(newBudgetItem);
        
        await _context.SaveChangesAsync();

        var responseDto = new BudgetItemResponseDto 
        { 
            Id = newBudgetItem.Id, 
            Name = newBudgetItem.Name,
            Amount = newBudgetItem.Amount,
            Type = newBudgetItem.Type,
            CategoryId = newBudgetItem.CategoryId,
            CategoryName = category.Name
        };
        
        return CreatedAtAction(nameof(Get), new {id = newBudgetItem.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BudgetItemResponseDto>> Update(int id, CreateBudgetItemDto edit)
    {
        var budgetItem = await _context.BudgetItems
            .Include(bi => bi.Category)
            .FirstOrDefaultAsync(bi => bi.Id == id);

        if (budgetItem == null)
            return NotFound();

        var category = await _context.Categories.FindAsync(edit.CategoryId);
        if (category == null)
            return BadRequest("Category not found");

        budgetItem.UpdateDetails(edit.Name, edit.Amount, edit.Type, category);
        
        _context.BudgetItems.Update(budgetItem);
        await _context.SaveChangesAsync();
        
        var responseDto = new BudgetItemResponseDto 
        { 
            Id = budgetItem.Id, 
            Name = budgetItem.Name,
            Amount = budgetItem.Amount,
            Type = budgetItem.Type,
            CategoryId = budgetItem.CategoryId,
            CategoryName = category.Name
        };
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var budgetItem = await _context.BudgetItems.FindAsync(id);

        if (budgetItem == null)
            return NotFound();
        
        _context.BudgetItems.Remove(budgetItem);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}