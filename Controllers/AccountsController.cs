using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountResponseDto>> Get(int id)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
            return NotFound();

        var balance = await _context.FinancialEvents
            .Where(fe => fe.AccountId == id)
            .SumAsync(fe => (decimal?)fe.Amount) ?? 0;

        var responseDto = new AccountResponseDto { Id = account.Id, Name = account.Name, Balance = balance };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountResponseDto>>> GetAll()
    {
        var accounts = await _context.Accounts.ToListAsync();

        var accountDtos = new List<AccountResponseDto>();
        foreach (var account in accounts)
        {
            var balance = await _context.FinancialEvents
                .Where(fe => fe.AccountId == account.Id)
                .SumAsync(fe => (decimal?)fe.Amount) ?? 0;
            accountDtos.Add(new AccountResponseDto { Id = account.Id, Name = account.Name, Balance = balance });
        }
        
        return Ok(accountDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<AccountResponseDto>> Post(CreateAccountDto accountDto)
    {
        var user = await _context.Users.FindAsync(accountDto.UserId);
        if (user == null)
            return BadRequest("User not found");

        var newAccount = new Account(accountDto.Name, user);

        _context.Accounts.Add(newAccount);
        
        await _context.SaveChangesAsync();

        var responseDto = new AccountResponseDto {Id = newAccount.Id, Name = newAccount.Name, Balance = newAccount.GetBalance()};
        
        return CreatedAtAction(nameof(Get), new {id = newAccount.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountResponseDto>> Update(int id, CreateAccountDto edit)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
            return NotFound();

        account.UpdateName(edit.Name);
        
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
        
        var balance = await _context.FinancialEvents
            .Where(fe => fe.AccountId == id)
            .SumAsync(fe => (decimal?)fe.Amount) ?? 0;
        
        var responseDto = new AccountResponseDto {Id = account.Id, Name = account.Name, Balance = balance};
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
            return NotFound();
        
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}