using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasComment("Уникальный идентификатор");

        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(256)
            .HasComment("Логин пользователя");
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(256)
            .HasComment("Хэш пароля пользователя");
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasComment("Роль пользователя, принимает значение /// Admin = 1, /// User = 2");
        
        builder
            .HasIndex(u => u.Login)
            .IsUnique();   
    }
}