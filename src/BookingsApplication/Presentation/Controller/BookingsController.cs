using System.Security.Claims;
using Application.Abstractions.Mapper;
using Application.Abstractions.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controller;

[ApiController]
// [Authorize]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost("{id:Guid}/book")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BookEvent(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // await bookingService.CreateBookingAsync(id, Guid.Parse(userId));
        await bookingService.CreateBookingAsync(id, Guid.Parse("816c5295-f886-49e1-a86b-906d7f21b10b"));
        return Ok();
    }
    
    [HttpGet("{id:Guid}")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid id)
    {
        var booking = await bookingService.GetBookingByIdAsync(id);
        return Ok(BookingMapper.MapToDto(booking));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelBookingAsync(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await bookingService.CancelBookingAsync(id, Guid.Parse(userId));
        return NoContent();
    }
}