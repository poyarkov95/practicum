using Application.Abstractions.Mapper;
using Application.Abstractions.Services.Interface;
using Application.Common.DTOs;
using Application.Events.DTOs;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controller;

[ApiController]
[Authorize]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        return Ok(await eventService.GetAllAsync(title, from, to, page, pageSize));
    }

    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await eventService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateEventDto newEvent)
    {
        var model = EventMapper.MapToEvent(newEvent);
        return CreatedAtAction(nameof(Create), await eventService.CreateAsync(model));
    }

    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, CreateEventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
                .ToList();
            
            return BadRequest(new ErrorResponse
            {
                Message = "Ошибка валидации",
                Errors = errors
            });
        }
        
        if (id != eventDto.Id)
        {
            throw new EventNotFoundException("Идентификатор не совпадает с идентификатором из модели");
        }

        var model = EventMapper.MapToEvent(eventDto);
        return Ok(await eventService.UpdateAsync(model));
    }

    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eventService.DeleteAsync(id);
        return Ok();
    }
    
    [HttpGet("top10-events")]
    public async Task<IActionResult> GetTop10Events(CancellationToken ct)
    {
        var events = await eventService.GetTop10Events(ct);
        return Ok(events);
    }
}