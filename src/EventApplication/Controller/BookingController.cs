using EventApplication.Mapper;
using EventApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Controller;

[ApiController]
[Route("[controller]")]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("get-booking/{id:Guid}")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid id)
    {
        var booking = await bookingService.GetBookingByIdAsync(id);
        return Ok(BookingMapper.MapToDto(booking));
    }
}