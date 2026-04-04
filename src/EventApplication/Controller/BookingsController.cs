using EventApplication.Mapper;
using EventApplication.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Controller;

[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet("{id:Guid}", Name = "get-booking")]
    public async Task<IActionResult> GetBookingByIdAsync(Guid id)
    {
        var booking = await bookingService.GetBookingByIdAsync(id);
        return Ok(BookingMapper.MapToDto(booking));
    }
}