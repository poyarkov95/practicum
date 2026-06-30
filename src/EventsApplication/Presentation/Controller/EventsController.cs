using System.Security.Claims;
using Application.Abstractions.Mapper;
using Application.Common.DTOs;
using Application.Events.DTOs;
using Application.Services.Interface;
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
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await eventService.GetByIdAsync(id));
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
    
    [HttpPost("{id:Guid}/book")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookEvent(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //var booking = await bookingService.CreateBookingAsync(id, Guid.Parse(userId));
        // return AcceptedAtAction( actionName: nameof(BookingsController.GetBookingByIdAsync), controllerName:  nameof(BookingsController).Replace("Controller", ""), routeValues: new { id = booking.Id }, value: BookingMapper.MapToDto(booking));
        return Ok();
    }
}