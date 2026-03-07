using EventApplication.Mapper;
using EventApplication.Models;
using EventApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Controller;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var events = eventService.GetAll();
        return Ok(events);
    }

    [HttpGet("{id:Guid}")]
    public IActionResult GetById(Guid id)
    {
        var eventItem =  eventService.GetById(id);
        return eventItem == null ? NotFound() : Ok(EventMapper.MapToDto(eventItem));
    }

    [HttpPost]
    public IActionResult Create(EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        } 
        
        var model = EventMapper.MapToEvent(eventDto);
        var eventItem = eventService.Create(model);
        return eventItem == null ? BadRequest("Событие с таким идентификатором уже существует") 
            : CreatedAtAction(nameof(Create), EventMapper.MapToDto(eventItem));
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Update(Guid id, EventDto eventDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != eventDto.Id)
        {
            return BadRequest("Идентификатор не совпадает с идентификатором из модели");
        }

        var model = EventMapper.MapToEvent(eventDto);
        var eventItem = eventService.Update(model);
        return eventItem == null ? NotFound() : Ok(EventMapper.MapToDto(eventItem));
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        var result = eventService.Delete(id);
        return result ?  Ok() : NotFound();
    }
}