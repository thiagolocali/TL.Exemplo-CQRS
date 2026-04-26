using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TL.ExemploCQRS.Domain.Entities;

namespace TL.ExemploCQRS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("User");

        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("IX_Users_Username");
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("IX_Users_Email");

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
