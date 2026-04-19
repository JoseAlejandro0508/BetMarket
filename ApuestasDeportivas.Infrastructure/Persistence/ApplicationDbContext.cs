using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Domain.Entities;
using ApuestasDeportivas.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApuestasDeportivas.Infrastructure.Persistence;

/// <summary>
/// Contexto principal de base de datos para Identity + entidades del dominio.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Apuestas creadas por usuarios de la plataforma.
    /// </summary>
    public DbSet<UserBet> UserBets => Set<UserBet>();

    /// <summary>
    /// Ofertas de apuestas persistidas localmente.
    /// </summary>
    public DbSet<StoredOddsOffer> StoredOddsOffers => Set<StoredOddsOffer>();

    /// <summary>
    /// Configuración persistida de la sincronización de odds.
    /// </summary>
    public DbSet<OddsSyncSettings> OddsSyncSettings => Set<OddsSyncSettings>();

    /// <summary>
    /// Solicitudes de depósito.
    /// </summary>
    public DbSet<DepositRequest> DepositRequests => Set<DepositRequest>();

    /// <summary>
    /// Solicitudes de retiro.
    /// </summary>
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();

    /// <summary>
    /// Settings de retiro por usuario.
    /// </summary>
    public DbSet<UserWithdrawalSettings> UserWithdrawalSettings => Set<UserWithdrawalSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
        });

        builder.Entity<UserBet>(entity =>
        {
            entity.ToTable("UserBets");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventId).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SportKey).HasMaxLength(120).IsRequired();
            entity.Property(x => x.HomeTeam).HasMaxLength(160).IsRequired();
            entity.Property(x => x.AwayTeam).HasMaxLength(160).IsRequired();
            entity.Property(x => x.BookmakerKey).HasMaxLength(80).IsRequired();
            entity.Property(x => x.BookmakerTitle).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MarketKey).HasMaxLength(40).IsRequired();
            entity.Property(x => x.SelectedOutcomeName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SelectedOutcomePrice).HasPrecision(18, 4);
            entity.Property(x => x.Stake).HasPrecision(18, 2);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Bets)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StoredOddsOffer>(entity =>
        {
            entity.ToTable("StoredOddsOffers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventId).HasMaxLength(120).IsRequired();
            entity.Property(x => x.SportKey).HasMaxLength(120).IsRequired();
            entity.Property(x => x.HomeTeam).HasMaxLength(160).IsRequired();
            entity.Property(x => x.AwayTeam).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PayloadJson).IsRequired();
        });

        builder.Entity<OddsSyncSettings>(entity =>
        {
            entity.ToTable("OddsSyncSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefreshIntervalSeconds).HasDefaultValue(60);
            entity.Property(x => x.CurrentSportKey).HasMaxLength(120).HasDefaultValue("multi");
            entity.Property(x => x.LastError).HasMaxLength(1000);
            entity.Property(x => x.SelectedSportKeysJson).HasDefaultValue("[]");
        });

        builder.Entity<DepositRequest>(entity =>
        {
            entity.ToTable("DepositRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.TransactionId).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Status).HasConversion<int>();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Deposits)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WithdrawalRequest>(entity =>
        {
            entity.ToTable("WithdrawalRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Account).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.Method).HasConversion<int>();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Withdrawals)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserWithdrawalSettings>(entity =>
        {
            entity.ToTable("UserWithdrawalSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Account).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Method).HasConversion<int>();

            entity.HasOne(x => x.User)
                .WithOne(x => x.WithdrawalSettings)
                .HasForeignKey<UserWithdrawalSettings>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.UserId).IsUnique();
        });
    }
}
