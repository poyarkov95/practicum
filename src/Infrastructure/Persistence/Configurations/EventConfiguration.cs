using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedNever()
            .HasComment("Уникальный идентификатор");;
        
        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Название события");
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000).
            HasComment("Описание события");
        
        builder.Property(e => e.StartAt)
            .IsRequired()
            .HasComment("Дата и время начала события");
        
        builder.Property(e => e.EndAt)
            .IsRequired()
            .HasComment("Дата и время окончания события");
        
        builder.Property(e => e.TotalSeats)
            .IsRequired()
            .HasComment("Общее количество мест");
        
        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}