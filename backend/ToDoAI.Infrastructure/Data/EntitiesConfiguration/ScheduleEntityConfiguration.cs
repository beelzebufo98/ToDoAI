using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class ScheduleEntityConfiguration : IEntityTypeConfiguration<ScheduleEntity>
{
    public void Configure(EntityTypeBuilder<ScheduleEntity> builder)
    {
        builder.ToTable("Schedules", "ToDoAIService");

        builder.HasOne(s => s.Task)
            .WithMany()
            .HasForeignKey(s => s.TaskId);
    }
}