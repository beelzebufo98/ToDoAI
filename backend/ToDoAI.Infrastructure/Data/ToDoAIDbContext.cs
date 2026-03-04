using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Domain.Entities;

namespace ToDoAI.ToDoAI.Infrastructure.Data;

public class ToDoAIDbContext : DbContext
{
    public virtual DbSet<TaskEntity> Tasks { get; set; }
    
    public virtual DbSet<UserEntity> Users { get; set; }
    
    public virtual DbSet<StateEntity> States { get; set; }
    
    public virtual DbSet<ScheduleEntity> Schedules { get; set; }
    
    public virtual DbSet<DayScheduleEntity>  DaySchedules { get; set; }
    
    public virtual DbSet<TaskExecutionEntity>  TaskExecutions { get; set; }
    
    public ToDoAIDbContext(DbContextOptions<ToDoAIDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ToDoAIService");
        modelBuilder.Entity<UserEntity>().ToTable("Users", "ToDoAIService");
        modelBuilder.Entity<TaskEntity>().ToTable("Tasks", "ToDoAIService");
        modelBuilder.Entity<DayScheduleEntity>().ToTable("DaySchedules", "ToDoAIService");
        modelBuilder.Entity<StateEntity>().ToTable("States", "ToDoAIService");
        modelBuilder.Entity<ScheduleEntity>().ToTable("Schedules", "ToDoAIService");
        modelBuilder.Entity<TaskExecutionEntity>().ToTable("TaskExecutions", "ToDoAIService");
    }
}