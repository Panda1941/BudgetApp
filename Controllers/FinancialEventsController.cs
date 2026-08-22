using Microsoft.AspNetCore.Mvc;
using BudgetApp.Models;
using BudgetApp.Data;
using BudgetApp.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinancialEventsController : ControllerBase
{
    private readonly AppDbContext _context;
    
    public FinancialEventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FinancialEventResponseDto>> Get(int id)
    {
        var financialEvent = await _context.FinancialEvents
            .Include(fe => fe.Account)
            .Include(fe => fe.Category)
            .FirstOrDefaultAsync(fe => fe.Id == id);

        if (financialEvent == null)
            return NotFound();

        var responseDto = new FinancialEventResponseDto 
        { 
            Id = financialEvent.Id, 
            Description = financialEvent.Description,
            Amount = financialEvent.Amount,
            Date = financialEvent.Date,
            AccountId = financialEvent.AccountId,
            Type = financialEvent.Type,
            CategoryId = financialEvent.CategoryId,
            CategoryName = financialEvent.Category?.Name,
            TransferPairId = financialEvent.TransferPairId,
            IsTransfer = financialEvent.IsTransfer
        };
        
        return Ok(responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialEventResponseDto>>> GetAll()
    {
        var financialEvents = await _context.FinancialEvents
            .Include(fe => fe.Account)
            .Include(fe => fe.Category)
            .ToListAsync();

        IEnumerable<FinancialEventResponseDto> responseDtos = financialEvents.Select(fe => new FinancialEventResponseDto
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
        });
        
        return Ok(responseDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<FinancialEventResponseDto>> Post(CreateFinancialEventDto financialEventDto)
    {
        var account = await _context.Accounts.FindAsync(financialEventDto.AccountId);
        if (account == null)
            return BadRequest("Account not found");

        Category? category = null;
        Account? destinationAccount = null;

        if (financialEventDto.Type == FinancialEventType.Transfer)
        {
            if (!financialEventDto.DestinationAccountId.HasValue)
                return BadRequest("DestinationAccountId required for Transfer");

            destinationAccount = await _context.Accounts.FindAsync(financialEventDto.DestinationAccountId.Value);
            if (destinationAccount == null)
                return BadRequest("Destination account not found");

            if (destinationAccount.Id == account.Id)
                return BadRequest("Source and destination accounts must be different");
        }
        else
        {
            if (!financialEventDto.CategoryId.HasValue)
                return BadRequest("CategoryId required for Income/Expense");

            category = await _context.Categories.FindAsync(financialEventDto.CategoryId.Value);
            if (category == null)
                return BadRequest("Category not found");
        }

        FinancialEvent newFinancialEvent;

        if (financialEventDto.Type == FinancialEventType.Transfer)
        {
            var (sourceEvent, destinationEvent) = FinancialEvent.CreateTransferPair(
                financialEventDto.Description,
                financialEventDto.Amount,
                financialEventDto.Date,
                account,
                destinationAccount!);

            _context.FinancialEvents.Add(sourceEvent);
            _context.FinancialEvents.Add(destinationEvent);

            await _context.SaveChangesAsync();

            // Return the source event (the one for the requesting account)
            newFinancialEvent = sourceEvent;
        }
        else
        {
            newFinancialEvent = new FinancialEvent(
                financialEventDto.Description,
                financialEventDto.Amount,
                financialEventDto.Date,
                account,
                financialEventDto.Type,
                category!);

            _context.FinancialEvents.Add(newFinancialEvent);
            
            await _context.SaveChangesAsync();
        }

        var responseDto = new FinancialEventResponseDto 
        { 
            Id = newFinancialEvent.Id, 
            Description = newFinancialEvent.Description,
            Amount = newFinancialEvent.Amount,
            Date = newFinancialEvent.Date,
            AccountId = newFinancialEvent.AccountId,
            Type = newFinancialEvent.Type,
            CategoryId = newFinancialEvent.CategoryId,
            CategoryName = category?.Name,
            TransferPairId = newFinancialEvent.TransferPairId,
            IsTransfer = newFinancialEvent.IsTransfer
        };
        
        return CreatedAtAction(nameof(Get), new {id = newFinancialEvent.Id}, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FinancialEventResponseDto>> Update(int id, CreateFinancialEventDto edit)
    {
        var financialEvent = await _context.FinancialEvents
            .Include(fe => fe.Category)
            .FirstOrDefaultAsync(fe => fe.Id == id);

        if (financialEvent == null)
            return NotFound();

        Category? category = null;
        
        if (edit.Type != FinancialEventType.Transfer)
        {
            if (!edit.CategoryId.HasValue)
                return BadRequest("CategoryId required for Income/Expense");

            category = await _context.Categories.FindAsync(edit.CategoryId.Value);
            if (category == null)
                return BadRequest("Category not found");
        }

        financialEvent.UpdateDetails(
            edit.Description, 
            edit.Amount, 
            edit.Date, 
            edit.Type, 
            category);
        
        _context.FinancialEvents.Update(financialEvent);
        await _context.SaveChangesAsync();
        
        var responseDto = new FinancialEventResponseDto 
        { 
            Id = financialEvent.Id, 
            Description = financialEvent.Description,
            Amount = financialEvent.Amount,
            Date = financialEvent.Date,
            AccountId = financialEvent.AccountId,
            Type = financialEvent.Type,
            CategoryId = financialEvent.CategoryId,
            CategoryName = financialEvent.Category?.Name,
            TransferPairId = financialEvent.TransferPairId,
            IsTransfer = financialEvent.IsTransfer
        };
        
        return Ok(responseDto);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var financialEvent = await _context.FinancialEvents.FindAsync(id);

        if (financialEvent == null)
            return NotFound();
        
        _context.FinancialEvents.Remove(financialEvent);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}