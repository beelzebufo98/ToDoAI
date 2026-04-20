using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.DalProviders.UserStateDalProvider.Mappers;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.UserStateDalProvider;

public sealed class UserStateDalProvider : IUserStateDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext>  _dbContextFactory;

    public UserStateDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UserStateDal> CreateUserState(UserStateDalRequest request, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var userStateEntity = request.ToUserStateEntity();
        
        await dbContext.States.AddAsync(userStateEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var result = userStateEntity.ToUserStateDal();
        return result;
    }

    public async Task<UserStateDal> UpdateUserState(UpdateUserStateDalRequest request, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var stateEntity = await dbContext.States.Where(s => s.Id == request.StateId && s.UserId == request.UserId).OrderByDescending(x => x.CreatedAt).FirstAsync(cancellationToken);

        stateEntity.SleepMinutes = request.SleepMinutes;
        stateEntity.EnergyLevel = request.EnergyLevel;
        stateEntity.StressLevel = request.StressLevel;
        stateEntity.MotivationLevel = request.MotivationLevel;
        stateEntity.ConcentrationLevel = request.ConcentrationLevel;
        await dbContext.SaveChangesAsync(cancellationToken);
        var result = stateEntity.ToUserStateDal();
        return result;
    }
    public async Task<UserStateDal?> GetLatestUserState(Guid userId, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var stateEntity = await dbContext.States.AsNoTracking().Where(s => s.UserId == userId).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);
        var result = stateEntity?.ToUserStateDal();
        return result;
    }

    public async Task<IReadOnlyList<UserStateDal>> GetUserStates(Guid userId, int limit,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var stateEntity = await dbContext.States.AsNoTracking().Where(s => s.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(limit).ToListAsync(cancellationToken);
        var result = stateEntity.Select(s => s.ToUserStateDal()).ToList();
        return result;
    }
}