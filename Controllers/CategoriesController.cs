using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> Get(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        var responseDto = new CategoryResponseDto { Id = category.Id, Name = category.Name };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAll()
    {
        var categories = await _context.Categories.ToListAsync();

        IEnumerable<CategoryResponseDto> responseDtos = categories.Select(c => new CategoryResponseDto{Id = c.Id, Name = c.Name});
        
        return Ok(responseDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Post(CreateCategoryDto categoryDto)
    {
        var newCategory = new Category(categoryDto.Name);

        _context.Categories.Add(newCategory);
        
        await _context.SaveChangesAsync();

        var responseDto = new CategoryResponseDto {Id = newCategory.Id, Name = newCategory.Name};
        
        return CreatedAtAction(nameof(Get), new {id = newCategory.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(int id, CreateCategoryDto edit)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        category.UpdateName(edit.Name);
        
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        
        var responseDto = new CategoryResponseDto {Id = category.Id, Name = category.Name};
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}