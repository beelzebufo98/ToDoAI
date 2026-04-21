using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class TaskWorkSessionEntityConfiguration : IEntityTypeConfiguration<TaskWorkSessionEntity>
{
    public void Configure(EntityTypeBuilder<TaskWorkSessionEntity> builder)
    {
        builder.ToTable("TaskWorkSessions", "ToDoAIService");

        builder.HasIndex(s => new { s.UserId, s.Status });

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)TaskWorkSessionStatus.Open}");

        builder.HasIndex(s => new { s.TaskId, s.StartedAt });

        builder.HasIndex(s => new { s.UserId, s.StartedAt });
        
        builder.HasOne(s => s.Task)
            .WithMany(t => t.TaskWorkSessions)
            .HasForeignKey(s => s.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.ScheduleBlock)
            .WithMany()
            .HasForeignKey(s => s.ScheduleBlockId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}