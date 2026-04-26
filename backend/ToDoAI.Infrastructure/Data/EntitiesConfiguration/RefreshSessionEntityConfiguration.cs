using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class RefreshSessionEntityConfiguration : IEntityTypeConfiguration<RefreshSessionEntity> 
{
    public void Configure(EntityTypeBuilder<RefreshSessionEntity> builder)
    {
        builder.ToTable("RefreshSessions", "ToDoAIService");

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}