using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class PasswordResetEntityConfiguration :  IEntityTypeConfiguration<PasswordResetEntity>
{
    public void Configure(EntityTypeBuilder<PasswordResetEntity> builder)
    {
        builder.ToTable("PasswordResets", "ToDoAIService");

        builder.Property(x => x.CodeHash)
            .IsRequired();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
        builder.HasIndex(x => new { x.UserId, x.CodeHash });
        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => new { x.UserId, x.SentAt });
    }
}