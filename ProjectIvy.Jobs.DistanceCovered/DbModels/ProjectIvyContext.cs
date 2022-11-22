using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectIvy.Jobs.DistanceCovered.DbModels;

public partial class ProjectIvyContext : DbContext
{
    public ProjectIvyContext()
    {
    }

    public ProjectIvyContext(DbContextOptions<ProjectIvyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DistanceCovered> DistanceCovereds { get; set; }

    public virtual DbSet<Tracking> Trackings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DistanceCovered>(entity =>
        {
            entity.ToTable("DistanceCovered", "Tracking");

            entity.Property(e => e.From).HasPrecision(0);
            entity.Property(e => e.To).HasPrecision(0);

            entity.HasOne(d => d.User).WithMany(p => p.DistanceCovereds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DistanceCovered_User");
        });

        modelBuilder.Entity<Tracking>(entity =>
        {
            entity.ToTable("Tracking", "Tracking");

            entity.HasIndex(e => e.Geohash, "IX_Tracking_Geohash");

            entity.HasIndex(e => e.Timestamp, "Timestamp_Tracking");

            entity.HasIndex(e => new { e.Latitude, e.Longitude }, "idx_DCh_1282_1281_Tracking");

            entity.HasIndex(e => new { e.UserId, e.Latitude, e.Longitude }, "idx_DCh_3428_3427_Tracking");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Geohash)
                .HasMaxLength(9)
                .HasDefaultValueSql("('')");
            entity.Property(e => e.Latitude).HasColumnType("numeric(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("numeric(9, 6)");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Trackings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tracking_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "User");

            entity.Property(e => e.AuthIdentifier).HasMaxLength(100);
            entity.Property(e => e.Created)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.ImdbId).HasMaxLength(50);
            entity.Property(e => e.LastFmUsername).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Modified)
                .HasPrecision(3)
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PasswordHash).HasMaxLength(128);
            entity.Property(e => e.PasswordModified).HasPrecision(3);
            entity.Property(e => e.TrackingStartDate).HasColumnType("date");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
