using EventApplication.Models;
using EventApplication.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace EventApplication.Tests;

public class FluentValidatorTest
{
    private readonly EventValidator _validator = new();

    [Fact]
    public void ValidateEmptyTitleTest()
    {
        var eventDto = new CreateEventDto
        {
            Title = string.Empty,
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };
        
         var result = _validator.TestValidate(eventDto);
         result.ShouldHaveValidationErrorFor(x => x.Title)
               .WithErrorMessage("Заголовок обязателен для заполнения.");
    }
    
    [Fact]
    public void ValidateIsValidTest()
    {
        var eventDto = new CreateEventDto()
        {
            Title = "Название события",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };
        
         var result = _validator.TestValidate(eventDto);
         result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
    
    [Fact]
    public void ValidateStartAtIsEmptyTest()
    {
        var eventDto = new CreateEventDto
        {
            Title = "Название события",
            StartAt = default,
            EndAt = DateTime.Now
        };
        
         var result = _validator.TestValidate(eventDto);
         result.ShouldHaveValidationErrorFor(x => x.StartAt)
             .WithErrorMessage("Дата начала события обязателена для заполнения.");
    }
    
    [Fact]
    public void ValidateEndAtIsEmptyTest()
    {
        var eventDto = new CreateEventDto
        {
            Title = "Название события",
            StartAt = DateTime.Now,
            EndAt = default
        };
        
         var result = _validator.TestValidate(eventDto);
         result.ShouldHaveValidationErrorFor(x => x.EndAt)
             .WithErrorMessage("Дата окончания события обязателена для заполнения.");
    }
    
    [Fact]
    public void ValidateStartAtIsLaterThanDateEndTest()
    {
        var eventDto = new CreateEventDto
        {
            Title = "Название события",
            StartAt = DateTime.Now.AddHours(1),
            EndAt = DateTime.Now
        };
        
         var result = _validator.TestValidate(eventDto);
         result.ShouldHaveValidationErrorFor(x => x.EndAt)
             .WithErrorMessage("Дата окончания события должна быть позже даты начала");
    }

    [Fact]
    public void ValidateTotalSeatsGreaterThanZeroTest()
    {
        var eventDto = new CreateEventDto
        {
            Title = "Название события",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = -1
        };
        
        var result = _validator.TestValidate(eventDto);
        result.ShouldHaveValidationErrorFor(x => x.TotalSeats)
            .WithErrorMessage("Количетсво мест на событии не может быть больше 0.");
    }
}