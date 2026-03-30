using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Domain.Entities;

namespace ToDoAI.ToDoAI.Infrastructure.Data;

public class ToDoAIDbContext : DbContext
{
    public virtual DbSet<TaskEntity> Tasks { get; set; }
    
    public virtual DbSet<UserEntity> Users { get; set; }
    
    public virtual DbSet<UserStateEntity> States { get; set; }
    
    public virtual DbSet<ScheduleEntity> Schedules { get; set; }
    
    public virtual DbSet<DayScheduleEntity>  DaySchedules { get; set; }
    
    public virtual DbSet<TaskExecutionEntity>  TaskExecutions { get; set; }
    
    public ToDoAIDbContext(DbContextOptions<ToDoAIDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ToDoAIService");
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users", "ToDoAIService");

            entity.HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.States)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks", "ToDoAIService");
        });

        modelBuilder.Entity<DayScheduleEntity>(entity =>
        {
            entity.ToTable("DaySchedules", "ToDoAIService");

            entity.HasOne(ds => ds.User)
                .WithMany()
                .HasForeignKey(ds => ds.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(ds => ds.Blocks)
                .WithOne(s => s.DaySchedule)
                .HasForeignKey(s => s.DayScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserStateEntity>(entity =>
        {
            entity.ToTable("States", "ToDoAIService");
        });

        modelBuilder.Entity<ScheduleEntity>(entity =>
        {
            entity.ToTable("Schedules", "ToDoAIService");

            entity.HasOne(s => s.Task)
                .WithMany()
                .HasForeignKey(s => s.TaskId);
        });

        modelBuilder.Entity<TaskExecutionEntity>(entity =>
        {
            entity.ToTable("TaskExecutions", "ToDoAIService");

            entity.HasOne(te => te.Task)
                .WithMany()
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
