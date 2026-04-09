using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.Data.EntitiesConfiguration;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
       builder.ToTable("Users", "ToDoAIService");
       
       builder.HasMany(u => u.Tasks)
           .WithOne(t => t.User)
           .HasForeignKey(t => t.UserId)
           .OnDelete(DeleteBehavior.Cascade);
       
       builder.HasMany(u => u.States)
           .WithOne(s => s.User)
           .HasForeignKey(s => s.UserId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}