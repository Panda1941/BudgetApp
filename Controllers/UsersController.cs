using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> Get(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var responseDto = new UserResponseDto { Id = user.Id, Name = user.Name };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _context.Users.ToListAsync();

        IEnumerable<UserResponseDto> responseDtos = users.Select(u => new UserResponseDto{Id = u.Id, Name = u.Name});
        
        return Ok(responseDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Post(CreateUserDto user)
    {
        var passwordHash = user.Password;   // TODO: Hash password

        var newUser = new User(user.Name, passwordHash);

        _context.Users.Add(newUser);
        
        await _context.SaveChangesAsync();

        var responseDto = new UserResponseDto {Id = newUser.Id, Name = newUser.Name};
        
        return CreatedAtAction(nameof(Get), new {id = newUser.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> Update(int id, CreateUserDto edit)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var passwordHash = edit.Password;

        user.UpdateDetails(edit.Name, passwordHash);
        
        _context.Users.Update(user);    // Technically not needed since EF Core will detect the change
        await _context.SaveChangesAsync();
        
        var responseDto = new UserResponseDto {Id = user.Id, Name = user.Name};
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}