using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class RefreshSessionEntityConfiguration : IEntityTypeConfiguration<RefreshSessionEntity> 
{
    public void Configure(EntityTypeBuilder<RefreshSessionEntity> builder)
    {
        builder.ToTable("RefreshSessions", "ToDoAIService");

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();
    }
}