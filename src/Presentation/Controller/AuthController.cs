using Application.Abstractions.Services.Interface;
using Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controller;

[ApiController]
[AllowAnonymous]
[Route("[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(LoginRequestDto request)
    {
        await userService.Register(request);
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        return CreatedAtAction(nameof(Login), await userService.Login(request));
    }
}