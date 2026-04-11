using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class DayScheduleEntityConfiguration : IEntityTypeConfiguration<DayScheduleEntity>
{
    public void Configure(EntityTypeBuilder<DayScheduleEntity> builder)
    {
        builder.ToTable("DaySchedules", "ToDoAIService");

        builder.HasOne(ds => ds.User)
            .WithMany()
            .HasForeignKey(ds => ds.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ds => ds.Blocks)
            .WithOne(s => s.DaySchedule)
            .HasForeignKey(s => s.DayScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}