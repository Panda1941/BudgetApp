using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Dtos;

namespace BudgetApp.Controllers;

public class CategoriesMvcController : Controller
{
    private readonly AppDbContext _context;

    public CategoriesMvcController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<int> GetDefaultUserIdAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync();
        return user?.Id ?? 1;
    }

    // GET /CategoriesMvc
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        var categoryDtos = categories
            .Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name })
            .ToList();

        ViewBag.UserId = await GetDefaultUserIdAsync();
        return View(categoryDtos);
    }

    // GET /CategoriesMvc/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.UserId = await GetDefaultUserIdAsync();
        return View(new CreateCategoryDto());
    }

    // POST /CategoriesMvc/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        var category = new Category(dto.Name);
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST /CategoriesMvc/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}