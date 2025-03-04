using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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

    public virtual DbSet<TimeEntryType> TimeEntryTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite(@"Data Source=C:\Users\jprochazka\source\repos\VykazyPrace\VykazyPrace.Core\Database\VykazyPrace.db");

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

        modelBuilder.Entity<TimeEntryType>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_TimeEntryTypes_ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Id, "IX_Users_ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
