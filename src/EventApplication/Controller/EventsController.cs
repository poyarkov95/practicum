using EventApplication.Exception;
using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Controller;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? title,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        return Ok(eventService.GetAll(title, from, to, page, pageSize));
    }

    [HttpGet("{id:Guid}")]
    public IActionResult GetById(Guid id)
    {
        return Ok(eventService.GetById(id));
    }

    [HttpPost]
    public IActionResult Create(EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
                .ToList();
            
            throw new ValidationException(string.Join(", ", errors));
        } 
        
        var model = EventMapper.MapToEvent(eventDto);
        return CreatedAtAction(nameof(Create), eventService.Create(model));
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Update(Guid id, EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
                .ToList();
            
            throw new ValidationException(string.Join(", ", errors));
        }
        
        if (id != eventDto.Id)
        {
            throw new EventNotFoundException("Идентификатор не совпадает с идентификатором из модели");
        }

        var model = EventMapper.MapToEvent(eventDto);
        return Ok(eventService.Update(model));
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        eventService.Delete(id);
        return Ok();
    }
}