using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Configuration;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService.Load();
            optionsBuilder.UseSqlite($"Data Source={config.DatabasePath}");
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
