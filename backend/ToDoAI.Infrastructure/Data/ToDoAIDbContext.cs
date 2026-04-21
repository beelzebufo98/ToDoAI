using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data;

public class ToDoAIDbContext : DbContext
{
    public virtual DbSet<TaskEntity> Tasks { get; set; }
    
    public virtual DbSet<UserEntity> Users { get; set; }
    
    public virtual DbSet<UserStateEntity> States { get; set; }
    
    public virtual DbSet<ScheduleEntity> Schedules { get; set; }
    
    public virtual DbSet<DayScheduleEntity>  DaySchedules { get; set; }
    
    public virtual DbSet<TaskExecutionEntity>  TaskExecutions { get; set; }
    
    public virtual DbSet<RefreshSessionEntity>  RefreshSessions { get; set; }
    
    public virtual DbSet<TaskWorkSessionEntity>   TaskWorkSessions { get; set; }
    public ToDoAIDbContext(DbContextOptions<ToDoAIDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ToDoAIService");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
