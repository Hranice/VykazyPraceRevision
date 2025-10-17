using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // pro konverzi DateTime -> UTC
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models.OutlookEvents;

namespace VykazyPrace.Core.Database.Models;

public partial class VykazyPraceContext : DbContext
{
    public VykazyPraceContext()
    {
    }

    public VykazyPraceContext(DbContextOptions<VykazyPraceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<TimeEntry> TimeEntries { get; set; }
    public virtual DbSet<TimeEntrySubType> TimeEntrySubTypes { get; set; }
    public virtual DbSet<TimeEntryType> TimeEntryTypes { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserGroup> UserGroups { get; set; }
    public virtual DbSet<SpecialDay> SpecialDays { get; set; }
    public virtual DbSet<ArrivalDeparture> ArrivalsDepartures { get; set; }
    public virtual DbSet<CalendarItem> CalendarItems { get; set; }
    public virtual DbSet<UserItemState> UserItemStates { get; set; }
    public virtual DbSet<ItemChangeLog> ItemChangeLogs { get; set; }
    public virtual DbSet<ItemAttendee> ItemAttendees { get; set; }
    public virtual DbSet<SyncSession> SyncSessions { get; set; }
    public virtual DbSet<SyncState> SyncStates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService.Load();
            optionsBuilder.UseSqlite($"Data Source={config.DatabasePath}");
            // Pozn.: Pro sdílené použití SQLite mezi více klienty zvaž:
            //  - PRAGMA journal_mode=WAL
            //  - PRAGMA busy_timeout=5000
            // Nastav tyto hodnoty při otevření připojení (např. přes custom interceptor / init script).
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dtToUtc = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var dtNullableToUtc = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_Projects_Id").IsUnique();

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_TimeEntries_Id").IsUnique();

            entity.Property(e => e.EntryMinutes).HasDefaultValue(30);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("DATETIME");

            entity.HasOne(d => d.EntryType).WithMany(p => p.TimeEntries).HasForeignKey(d => d.EntryTypeId);

            entity.HasOne(d => d.Project).WithMany(p => p.TimeEntries).HasForeignKey(d => d.ProjectId);

            entity.HasOne(d => d.User).WithMany(p => p.TimeEntries).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<TimeEntrySubType>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.TimeEntrySubTypes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TimeEntryType>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Color).HasDefaultValue("#ADD8E6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_Users_ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.UserGroup).WithMany(p => p.Users).HasForeignKey(d => d.UserGroupId);
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_UserGroups_Id").IsUnique();
        });

        modelBuilder.Entity<SpecialDay>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_SpecialDays_Id").IsUnique();

            entity.Property(e => e.Date)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.Title)
                .HasColumnType("TEXT")
                .IsRequired()
                .HasDefaultValue("Default");

            entity.Property(e => e.Locked)
                .HasColumnType("INTEGER")
                .HasDefaultValue(0);

            entity.Property(e => e.Color)
                .HasColumnType("TEXT")
                .HasDefaultValue("#FFCDC7");
        });

        modelBuilder.Entity<ArrivalDeparture>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_ArrivalsDepartures_Id").IsUnique();

            entity.Property(e => e.WorkDate)
                .HasColumnType("TEXT")
                .IsRequired();

            entity.Property(e => e.ArrivalTimestamp)
                .HasColumnType("TEXT");

            entity.Property(e => e.DepartureTimestamp)
                .HasColumnType("TEXT");

            entity.Property(e => e.DepartureReason)
                .HasColumnType("TEXT");

            entity.Property(e => e.HoursWorked)
                .HasColumnType("REAL")
                .HasDefaultValue(0);

            entity.Property(e => e.HoursOvertime)
                .HasColumnType("REAL")
                .HasDefaultValue(0);

            entity.HasOne(d => d.User)
                .WithMany(p => p.ArrivalDepartures)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CalendarItems
        modelBuilder.Entity<CalendarItem>(e =>
        {
            e.Property(x => x.LastSeenAtUtc).HasConversion(dtToUtc);
            e.Property(x => x.LastModifiedUtc).HasConversion(dtNullableToUtc);
            e.Property(x => x.OccurrenceStartUtc).HasConversion(dtNullableToUtc);
            e.Property(x => x.StartUtc).HasConversion(dtNullableToUtc);
            e.Property(x => x.EndUtc).HasConversion(dtNullableToUtc);

            // Unikátní identita fyzické instance v Outlooku
            e.HasIndex(x => new { x.StoreId, x.EntryId, x.OccurrenceStartUtc })
             .IsUnique();

            // Časté dotazy
            e.HasIndex(x => new { x.StoreId, x.EntryId });
            e.HasIndex(x => x.StartUtc);
        });

        // UserItemStates
        modelBuilder.Entity<UserItemState>(e =>
        {
            e.Property(x => x.UpdatedAtUtc).HasConversion(dtToUtc);

            // Každý uživatel má max. 1 stav k jedné položce
            e.HasIndex(x => new { x.UserId, x.ItemId }).IsUnique();

            e.HasOne(x => x.Item)
             .WithMany(i => i.UserStates)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ItemChangeLogs
        modelBuilder.Entity<ItemChangeLog>(e =>
        {
            e.Property(x => x.WhenUtc).HasConversion(dtToUtc);
            e.HasIndex(x => new { x.ItemId, x.WhenUtc });

            e.HasOne(x => x.Item)
             .WithMany(i => i.ChangeLogs)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ItemAttendees
        modelBuilder.Entity<ItemAttendee>(e =>
        {
            e.HasIndex(x => x.ItemId);
            e.HasIndex(x => x.Email);

            e.HasOne(x => x.Item)
             .WithMany(i => i.Attendees)
             .HasForeignKey(x => x.ItemId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // SyncSessions
        modelBuilder.Entity<SyncSession>(e =>
        {
            e.Property(x => x.StartedUtc).HasConversion(dtToUtc);
            e.Property(x => x.FinishedUtc).HasConversion(dtNullableToUtc);
        });

        // SyncState
        modelBuilder.Entity<SyncState>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.MachineName, x.Key }).IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
