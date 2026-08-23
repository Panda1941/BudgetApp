using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Data;
using BudgetApp.Models;
using BudgetApp.Dtos;

namespace BudgetApp.Controllers;

public class AccountsMvcController : Controller
{
    private readonly AppDbContext _context;

    public AccountsMvcController(AppDbContext context)
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

    private async Task<Category> GetOrCreateUncategorizedCategoryAsync()
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Uncategorized");
        if (category == null)
        {
            category = new Category("Uncategorized");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
        return category;
    }

    // GET /AccountsMvc
    public async Task<IActionResult> Index()
    {
        var userId = await GetDefaultUserIdAsync();
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        // Load financial events for each account to calculate balance
        foreach (var account in accounts)
        {
            var events = await _context.FinancialEvents
                .Where(fe => fe.AccountId == account.Id)
                .ToListAsync();
            // The events are tracked by EF Core, but the private collection isn't populated
            // We'll calculate balance directly from the query
        }

        var accountDtos = accounts
            .Select(a => new AccountResponseDto 
            { 
                Id = a.Id, 
                Name = a.Name, 
                Balance = _context.FinancialEvents.Where(fe => fe.AccountId == a.Id).Sum(fe => (decimal?)fe.Amount) ?? 0 
            })
            .ToList();

        ViewBag.UserId = userId;
        return View(accountDtos);
    }

    // GET /AccountsMvc/Detail/5
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var events = await _context.FinancialEvents
            .Where(fe => fe.AccountId == id)
            .Include(fe => fe.Category)
            .OrderByDescending(fe => fe.Date)
            .Select(fe => new FinancialEventResponseDto
            {
                Id = fe.Id,
                Description = fe.Description,
                Amount = fe.Amount,
                Date = fe.Date,
                AccountId = fe.AccountId,
                Type = fe.Type,
                CategoryId = fe.CategoryId,
                CategoryName = fe.Category != null ? fe.Category.Name : null,
                TransferPairId = fe.TransferPairId,
                IsTransfer = fe.IsTransfer
            })
            .ToListAsync();

        var balance = await _context.FinancialEvents
            .Where(fe => fe.AccountId == id)
            .SumAsync(fe => (decimal?)fe.Amount) ?? 0;

        ViewBag.Account = new AccountResponseDto { Id = account.Id, Name = account.Name, Balance = balance };
        ViewBag.UserId = userId;
        return View(events);
    }

    // GET /AccountsMvc/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = await GetDefaultUserIdAsync();
        ViewBag.UserId = userId;
        return View(new CreateAccountDto { UserId = userId });
    }

    // POST /AccountsMvc/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return BadRequest("User not found");

        var account = new Account(dto.Name, user);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET /AccountsMvc/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        ViewBag.AccountId = id;
        ViewBag.UserId = userId;
        return View(new CreateAccountDto { Name = account.Name, UserId = userId });
    }

    // POST /AccountsMvc/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateAccountDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        account.UpdateName(dto.Name);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST /AccountsMvc/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == id && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET /AccountsMvc/AddTransaction/5
    [HttpGet]
    public async Task<IActionResult> AddTransaction(int accountId)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        var categories = await _context.Categories.ToListAsync();
        var otherAccounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.Id != accountId)
            .ToListAsync();

        ViewBag.Account = new AccountResponseDto { Id = account.Id, Name = account.Name, Balance = account.GetBalance() };
        ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
        ViewBag.OtherAccounts = otherAccounts.Select(a => new AccountResponseDto { Id = a.Id, Name = a.Name, Balance = a.GetBalance() }).ToList();
        ViewBag.UserId = userId;

        return View(new CreateFinancialEventDto { AccountId = accountId, Date = DateTime.Today });
    }

    // POST /AccountsMvc/AddTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTransaction(int accountId, CreateFinancialEventDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var account = await _context.Accounts
            .Where(a => a.Id == accountId && a.UserId == userId)
            .FirstOrDefaultAsync();

        if (account == null)
            return NotFound();

        Category? category = null;
        Account? destinationAccount = null;

        if (dto.Type == FinancialEventType.Transfer)
        {
            if (!dto.DestinationAccountId.HasValue)
            {
                ModelState.AddModelError("DestinationAccountId", "Destination account is required for transfers");
            }
            else
            {
                destinationAccount = await _context.Accounts
                    .Where(a => a.Id == dto.DestinationAccountId.Value && a.UserId == userId)
                    .FirstOrDefaultAsync();
                if (destinationAccount == null)
                    ModelState.AddModelError("DestinationAccountId", "Destination account not found");
                else if (destinationAccount.Id == account.Id)
                    ModelState.AddModelError("DestinationAccountId", "Source and destination accounts must be different");
            }
        }
        else
        {
            // Use "Uncategorized" as default if no category selected
            if (!dto.CategoryId.HasValue)
            {
                category = await GetOrCreateUncategorizedCategoryAsync();
            }
            else
            {
                category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                    ModelState.AddModelError("CategoryId", "Category not found");
            }
        }

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            var otherAccounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.Id != accountId)
                .ToListAsync();

            ViewBag.Account = new AccountResponseDto { Id = account.Id, Name = account.Name, Balance = account.GetBalance() };
            ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
            ViewBag.OtherAccounts = otherAccounts.Select(a => new AccountResponseDto { Id = a.Id, Name = a.Name, Balance = a.GetBalance() }).ToList();
            ViewBag.UserId = userId;

            return View(dto);
        }

        var date = ToUtc(dto.Date);

        if (dto.Type == FinancialEventType.Transfer)
        {
            var (sourceEvent, destinationEvent) = FinancialEvent.CreateTransferPair(
                dto.Description,
                dto.Amount,
                date,
                account,
                destinationAccount!);

            _context.FinancialEvents.Add(sourceEvent);
            _context.FinancialEvents.Add(destinationEvent);
        }
        else
        {
            var newEvent = new FinancialEvent(
                dto.Description,
                dto.Amount,
                date,
                account,
                dto.Type,
                category!);

            _context.FinancialEvents.Add(newEvent);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Detail), new { id = accountId });
    }

    // GET /AccountsMvc/EditTransaction/5
    [HttpGet]
    public async Task<IActionResult> EditTransaction(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var fe = await _context.FinancialEvents
            .Include(f => f.Account)
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.Account.UserId == userId);

        if (fe == null)
            return NotFound();

        var categories = await _context.Categories.ToListAsync();
        var otherAccounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.Id != fe.AccountId)
            .ToListAsync();

        ViewBag.FinancialEvent = new FinancialEventResponseDto
        {
            Id = fe.Id,
            Description = fe.Description,
            Amount = fe.Amount,
            Date = fe.Date,
            AccountId = fe.AccountId,
            Type = fe.Type,
            CategoryId = fe.CategoryId,
            CategoryName = fe.Category?.Name,
            TransferPairId = fe.TransferPairId,
            IsTransfer = fe.IsTransfer
        };
        ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
        ViewBag.OtherAccounts = otherAccounts.Select(a => new AccountResponseDto { Id = a.Id, Name = a.Name, Balance = a.GetBalance() }).ToList();
        ViewBag.UserId = userId;

        var dto = new CreateFinancialEventDto
        {
            Description = fe.Description,
            Amount = Math.Abs(fe.Amount),
            Date = fe.Date,
            AccountId = fe.AccountId,
            Type = fe.Type,
            CategoryId = fe.CategoryId,
            DestinationAccountId = null
        };

        return View(dto);
    }

    // POST /AccountsMvc/EditTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransaction(int id, CreateFinancialEventDto dto)
    {
        var userId = await GetDefaultUserIdAsync();
        var fe = await _context.FinancialEvents
            .Include(f => f.Account)
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id && f.Account.UserId == userId);

        if (fe == null)
            return NotFound();

        Category? category = null;

        if (dto.Type != FinancialEventType.Transfer)
        {
            // Use "Uncategorized" as default if no category selected
            if (!dto.CategoryId.HasValue)
            {
                category = await GetOrCreateUncategorizedCategoryAsync();
            }
            else
            {
                category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                    ModelState.AddModelError("CategoryId", "Category not found");
            }
        }

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            var otherAccounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.Id != fe.AccountId)
                .ToListAsync();

            ViewBag.FinancialEvent = new FinancialEventResponseDto
            {
                Id = fe.Id,
                Description = fe.Description,
                Amount = fe.Amount,
                Date = fe.Date,
                AccountId = fe.AccountId,
                Type = fe.Type,
                CategoryId = fe.CategoryId,
                CategoryName = fe.Category?.Name,
                TransferPairId = fe.TransferPairId,
                IsTransfer = fe.IsTransfer
            };
            ViewBag.Categories = categories.Select(c => new CategoryResponseDto { Id = c.Id, Name = c.Name }).ToList();
            ViewBag.OtherAccounts = otherAccounts.Select(a => new AccountResponseDto { Id = a.Id, Name = a.Name, Balance = a.GetBalance() }).ToList();
            ViewBag.UserId = userId;

            return View(dto);
        }

        fe.UpdateDetails(dto.Description, dto.Amount, ToUtc(dto.Date), dto.Type, category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Detail), new { id = fe.AccountId });
    }

    // POST /AccountsMvc/DeleteTransaction/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var userId = await GetDefaultUserIdAsync();
        var fe = await _context.FinancialEvents
            .Include(f => f.Account)
            .FirstOrDefaultAsync(f => f.Id == id && f.Account.UserId == userId);

        if (fe == null)
            return NotFound();

        var accountId = fe.AccountId;
        _context.FinancialEvents.Remove(fe);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Detail), new { id = accountId });
    }
}