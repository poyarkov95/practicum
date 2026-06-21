using Application.Abstractions.Persistence.Repositories;
using Application.Abstractions.Services.Interface;
using Application.User;
using Domain.Entities;
using Domain.Exceptions;

namespace Infrastructure.Services.Implementation;

public class UserService(IUserRepository userRepository, IJWTGenerator jwtGenerator, IPasswordHashGenerator passwordHashGenerator) : IUserService
{
    public async Task<Guid> Register(LoginRequestDto loginRequestDto)
    {
        var user = new User
        {   
            Id = Guid.NewGuid(),
            Login = loginRequestDto.Login,
            Role = loginRequestDto.Role,
            PasswordHash = passwordHashGenerator.GenerateHash(loginRequestDto.Password)
        };
        
        await userRepository.AddUserAsync(user);
        return user.Id;
    }

    public async Task<string> Login(LoginRequestDto loginRequestDto)
    {
        var existingUser = await userRepository.GetUserByLogin(loginRequestDto.Login);

        if (existingUser == null || !passwordHashGenerator.VerifyPassword(loginRequestDto.Password, existingUser.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid login or password");
        }

        loginRequestDto.UserId = existingUser.Id;
        return jwtGenerator.GenerateToken(loginRequestDto);
    }

    public async Task<User> GetUser(Guid userId)
    {
        return await userRepository.GetUser(userId);
    }
}