using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class TaskExecutionEntityConfiguration : IEntityTypeConfiguration<TaskExecutionEntity>
{
    public void Configure(EntityTypeBuilder<TaskExecutionEntity> builder)
    {
        builder.ToTable("TaskExecutions", "ToDoAIService");

        builder.HasOne(te => te.Task)
            .WithMany()
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}