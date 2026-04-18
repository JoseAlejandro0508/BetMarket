using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Contracts.Bets;
using ApuestasDeportivas.Domain.Entities;
using ApuestasDeportivas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApuestasDeportivas.Application.Services;

/// <summary>
/// Caso de uso de creación y administración de apuestas.
/// </summary>
public class BetsService
{
    private readonly IApplicationDbContext _dbContext;

    public BetsService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserBetDto> PlaceBetAsync(string userId, PlaceBetRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Stake <= 0)
        {
            throw new InvalidOperationException("El monto apostado debe ser mayor que 0.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (user.Balance < request.Stake)
        {
            throw new InvalidOperationException("Balance insuficiente.");
        }

        user.Balance -= request.Stake;

        var bet = new UserBet
        {
            UserId = userId,
            EventId = request.EventId,
            SportKey = request.SportKey,
            HomeTeam = request.HomeTeam,
            AwayTeam = request.AwayTeam,
            CommenceTime = request.CommenceTime,
            BookmakerKey = request.BookmakerKey,
            BookmakerTitle = request.BookmakerTitle,
            MarketKey = request.MarketKey,
            SelectedOutcomeName = request.SelectedOutcomeName,
            SelectedOutcomePrice = request.SelectedOutcomePrice,
            Stake = request.Stake,
            Status = BetStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.UserBets.Add(bet);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(bet, user);
    }

    public async Task<IReadOnlyCollection<UserBetDto>> GetMyBetsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var bets = await _dbContext.UserBets
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        return bets
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x, x.User))
            .ToList();
    }

    public async Task<IReadOnlyCollection<UserBetDto>> GetPendingBetsAsync(CancellationToken cancellationToken = default)
    {
        var bets = await _dbContext.UserBets
            .Include(x => x.User)
            .Where(x => x.Status == BetStatus.Pending)
            .ToListAsync(cancellationToken);

        return bets
            .OrderBy(x => x.CreatedAt)
            .Select(x => MapToDto(x, x.User))
            .ToList();
    }

    public async Task<UserBetDto> ResolveBetAsync(Guid betId, bool isWon, CancellationToken cancellationToken = default)
    {
        var bet = await _dbContext.UserBets
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == betId, cancellationToken)
            ?? throw new InvalidOperationException("Apuesta no encontrada.");

        if (bet.Status != BetStatus.Pending)
        {
            throw new InvalidOperationException("La apuesta ya fue resuelta.");
        }

        bet.Status = isWon ? BetStatus.Won : BetStatus.Lost;
        bet.ResolvedAt = DateTimeOffset.UtcNow;

        if (isWon)
        {
            var payout = bet.Stake * bet.SelectedOutcomePrice;
            bet.User!.Balance += payout;
        }

        if (bet.User is not null)
        {
            bet.User.SecurityStamp = Guid.NewGuid().ToString();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToDto(bet, bet.User);
    }

    private static UserBetDto MapToDto(UserBet bet, AppUser? user)
    {
        return new UserBetDto(
            bet.Id,
            bet.EventId,
            bet.SportKey,
            bet.HomeTeam,
            bet.AwayTeam,
            bet.CommenceTime,
            bet.BookmakerKey,
            bet.BookmakerTitle,
            bet.MarketKey,
            bet.SelectedOutcomeName,
            bet.SelectedOutcomePrice,
            bet.Stake,
            bet.Status.ToString(),
            bet.CreatedAt,
            bet.ResolvedAt,
            bet.UserId,
            user?.DisplayName ?? string.Empty,
            user?.Email ?? string.Empty);
    }
}
