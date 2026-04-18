using EventApplication.Models;
using FluentValidation;

namespace EventApplication.Validation;

public class EventValidator : AbstractValidator<CreateEventDto>
{
    public EventValidator()
    {
        RuleFor(eventDto => eventDto.Title)
            .NotEmpty().WithMessage("Заголовок обязателен для заполнения.");
        
        RuleFor(eventDto => eventDto.StartAt)
            .NotEmpty().WithMessage("Дата начала события обязателена для заполнения.");
        
        RuleFor(eventDto => eventDto.EndAt)
            .NotEmpty().WithMessage("Дата окончания события обязателена для заполнения.")
            .GreaterThan(eventDto => eventDto.StartAt)
            .WithMessage("Дата окончания события должна быть позже даты начала");
        
        RuleFor(eventDto => eventDto.TotalSeats)
            .GreaterThan(0).WithMessage("Количетсво мест на событии не может быть больше 0.");
    }
}