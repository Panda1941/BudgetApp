using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Dtos;

namespace BudgetApp.Controllers;

public class BudgetsMvcController : Controller
{
    private readonly AppDbContext _context;

    public BudgetsMvcController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<int> GetDefaultUserIdAsync()
    {
        var user = await _context.Users.FirstOrDefaultAsync();
        return user?.Id ?? 1;
    }

    private static DateTime ToUtc(DateTime dt)
    {
        return dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt.ToUniversalTime();
    }

    // GET /BudgetsMvc
    public async Task<IActionResult> Index()
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.UserId == userId)
            .FirstOrDefaultAsync();

        List<BudgetItem> items = new();
        if (budget != null)
        {
            items = await _context.BudgetItems
                .Where(i => i.BudgetId == budget.Id)
                .Include(i => i.Category)
                .ToListAsync();
        }

        ViewBag.UserId = userId;
        ViewBag.BudgetItems = items;
        return View(budget);
    }

    // GET /BudgetsMvc/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = await GetDefaultUserIdAsync();
        var existingBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId);
        if (existingBudget != null)
            return RedirectToAction(nameof(Index));

        ViewBag.UserId = userId;
        return View(new CreateBudgetDto 
        { 
            UserId = userId, 
            StartDate = DateTime.Today, 
            EndDate = DateTime.Today.AddMonths(1) 
        });
    }

    // POST /BudgetsMvc/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBudgetDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var existingBudget = await _context.Budgets.FirstOrDefaultAsync(b => b.UserId == userId);
        if (existingBudget != null)
            return BadRequest("User already has a budget");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("User not found");

        var budget = new Budget(dto.Name, user, ToUtc(dto.StartDate), ToUtc(dto.EndDate));
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET /BudgetsMvc/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.Id == id && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return NotFound();

        ViewBag.BudgetId = id;
        ViewBag.UserId = userId;
        return View(new CreateBudgetDto 
        { 
            Name = budget.Name, 
            StartDate = budget.StartDate, 
            EndDate = budget.EndDate,
            UserId = userId 
        });
    }

    // POST /BudgetsMvc/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBudgetDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.Id == id && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return NotFound();

        budget.UpdateDetails(dto.Name, ToUtc(dto.StartDate), ToUtc(dto.EndDate));
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST /BudgetsMvc/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.Id == id && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return NotFound();

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET /BudgetsMvc/AddItem/5
    [HttpGet]
    public async Task<IActionResult> AddItem(int budgetId)
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.Id == budgetId && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return NotFound();

        var categories = await _context.Categories.ToListAsync();

        ViewBag.Budget = new BudgetResponseDto 
        { 
            Id = budget.Id, 
            Name = budget.Name, 
            StartDate = budget.StartDate, 
            EndDate = budget.EndDate,
            UserId = budget.UserId
        };
        ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
        ViewBag.UserId = userId;

        return View(new CreateBudgetItemDto { BudgetId = budgetId });
    }

    // POST /BudgetsMvc/AddItem/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int budgetId, CreateBudgetItemDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var budget = await _context.Budgets
            .Where(b => b.Id == budgetId && b.UserId == userId)
            .FirstOrDefaultAsync();

        if (budget == null)
            return NotFound();

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            ModelState.AddModelError("CategoryId", "Category not found");

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Budget = new BudgetResponseDto 
            { 
                Id = budget.Id, 
                Name = budget.Name, 
                StartDate = budget.StartDate, 
                EndDate = budget.EndDate,
                UserId = budget.UserId
            };
            ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
            ViewBag.UserId = userId;
            return View(dto);
        }

        var item = new BudgetItem(dto.Name, dto.Amount, dto.Type, category!, budget);
        _context.BudgetItems.Add(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET /BudgetsMvc/EditItem/5
    [HttpGet]
    public async Task<IActionResult> EditItem(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var item = await _context.BudgetItems
            .Include(i => i.Budget)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id && i.Budget.UserId == userId);

        if (item == null)
            return NotFound();

        var categories = await _context.Categories.ToListAsync();

        ViewBag.Item = new BudgetItemResponseDto
        {
            Id = item.Id,
            Name = item.Name,
            Amount = item.Amount,
            Type = item.Type,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.Name ?? ""
        };
        ViewBag.BudgetId = item.BudgetId;
        ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
        ViewBag.UserId = userId;

        return View(new CreateBudgetItemDto
        {
            Name = item.Name,
            Amount = item.Amount,
            Type = item.Type,
            CategoryId = item.CategoryId,
            BudgetId = item.BudgetId
        });
    }

    // POST /BudgetsMvc/EditItem/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditItem(int id, CreateBudgetItemDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var item = await _context.BudgetItems
            .Include(i => i.Budget)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id && i.Budget.UserId == userId);

        if (item == null)
            return NotFound();

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category == null)
            ModelState.AddModelError("CategoryId", "Category not found");

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Item = new BudgetItemResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Amount = item.Amount,
                Type = item.Type,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name ?? ""
            };
            ViewBag.BudgetId = item.BudgetId;
            ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
            ViewBag.UserId = userId;
            return View(dto);
        }

        item.UpdateDetails(dto.Name, dto.Amount, dto.Type, category!);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST /BudgetsMvc/DeleteItem/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var item = await _context.BudgetItems
            .Include(i => i.Budget)
            .FirstOrDefaultAsync(i => i.Id == id && i.Budget.UserId == userId);

        if (item == null)
            return NotFound();

        var budgetId = item.BudgetId;
        _context.BudgetItems.Remove(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}