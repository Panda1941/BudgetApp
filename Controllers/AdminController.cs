using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetApp.Data;
using BudgetApp.Dtos;

namespace BudgetApp.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET /Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();

        var userDtos = users
            .Select(u => new UserResponseDto { Id = u.Id, Name = u.Name })
            .ToList();

        return View(userDtos);
    }

    // GET /Admin/EditUser/5
    [HttpGet]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var dto = new CreateUserDto { Name = user.Name, Password = "" };
        ViewBag.UserId = id;

        return View(dto);
    }

    // POST /Admin/EditUser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, CreateUserDto edit)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Leave password unchanged if the field was left blank
        var passwordHash = string.IsNullOrWhiteSpace(edit.Password) ? user.PasswordHash : edit.Password; // TODO: real hashing

        user.UpdateDetails(edit.Name, passwordHash);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }

    // POST /Admin/DeleteUser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }
}