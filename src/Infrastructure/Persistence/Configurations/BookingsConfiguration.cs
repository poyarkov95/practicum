using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookingsConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
    
        builder.Property(b => b.Id)
            .ValueGeneratedNever()
            .HasComment("Уникальный идентификатор");
    
        builder.Property(b => b.Status)
            .IsRequired()
            .HasComment("Текущий статус брони");
    
        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasComment("Дата и время создания брони");
    
        builder.Property(b => b.ProcessedAt)
            .HasComment("Дата и время обработки брони");
    
        builder.Property(b => b.EventId)
            .IsRequired()
            .HasComment("Идентификатор события, к которому относится бронь");

        builder.Property(b => b.UserId)
            .IsRequired()
            .HasComment("Уникальный идентификатор пользователя, создашего бронь");
        
        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Booking>(o => o.UserId);
        
        builder.HasIndex(b => b.UserId)
            .IsUnique(false);
    }
}