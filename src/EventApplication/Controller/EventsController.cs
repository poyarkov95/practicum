using EventApplication.Exception;
using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Controller;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService, IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        return Ok(eventService.GetAllAsync(title, from, to, page, pageSize));
    }

    [HttpGet("{id:Guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(eventService.GetByIdAsync(id));
    }

    [HttpPost]
    public IActionResult Create(CreateEventDto newEvent)
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
        
        var model = EventMapper.MapToEvent(newEvent);
        return CreatedAtAction(nameof(Create), eventService.CreateAsync(model));
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Update(Guid id, CreateEventDto eventDto)
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
        return Ok(eventService.UpdateAsync(model));
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        eventService.DeleteAsync(id);
        return Ok();
    }
    
    [HttpPost("{id:Guid}/book")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookEvent(Guid id)
    {
        var booking = await bookingService.CreateBookingAsync(id);
        return AcceptedAtAction( actionName: nameof(BookingsController.GetBookingByIdAsync), controllerName:  nameof(BookingsController).Replace("Controller", ""), routeValues: new { id = booking.Id }, value: BookingMapper.MapToDto(booking));
    }
}