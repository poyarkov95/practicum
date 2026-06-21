using System.Security.Claims;
using Application.Abstractions.Mapper;
using Application.Abstractions.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controller;

[ApiController]
[Authorize]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
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
        await bookingService.CancelBookingAsync(id, userId);
        return Ok();
    }
}