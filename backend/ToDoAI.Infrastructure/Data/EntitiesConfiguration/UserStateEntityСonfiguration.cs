using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class UserStateEntityСonfiguration : IEntityTypeConfiguration<UserStateEntity>
{
    public void Configure(EntityTypeBuilder<UserStateEntity> builder)
    {
        builder.ToTable("States", "ToDoAIService");
    }
}