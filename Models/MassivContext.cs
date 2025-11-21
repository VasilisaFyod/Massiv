using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Massiv.Models;

public partial class MassivContext : DbContext
{
    public MassivContext()
    {
    }

    public MassivContext(DbContextOptions<MassivContext> options)
        : base(options)
    {
    }

    public static MassivContext GetContext()
    {
        return new MassivContext();
    }
    public virtual DbSet<LogistTable> LogistTables { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WorkshopOrder> WorkshopOrders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString());
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogistTable>(entity =>
        {
            entity.HasKey(e => e.LogistTableId).HasName("PK__LogistTa__09816FE45AAE68D0");

            entity.Property(e => e.LogistTableId).HasColumnName("LogistTableID");
            entity.Property(e => e.Anchor)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Base)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ColorMark)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasDefaultValue("#F1BCFF");
            entity.Property(e => e.Furniture)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Glass)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Hands)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.Kr1)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Kr3D)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Ldsp)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("LDSP");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Panel)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RangeHood)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ShipmentDate)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Side)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Table)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TableType)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Wash)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__C3905BAF3AAFD621");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ClientPhone)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Color)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Constructor)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Designer)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Facade)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Material)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NumberOrder)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Product)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ReadyDate)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A152FB7AB");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Name)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC24AB63A8");

            entity.ToTable("User");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Login)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleID__267ABA7A");
        });

        modelBuilder.Entity<WorkshopOrder>(entity =>
        {
            entity.HasKey(e => e.WorkshopOrderId).HasName("PK__Workshop__88073B2B00F6C7EB");

            entity.ToTable("WorkshopOrder");

            entity.Property(e => e.WorkshopOrderId).HasColumnName("WorkshopOrderID");
            entity.Property(e => e.NumberWorkshopOrder)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Order).WithMany(p => p.WorkshopOrders)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__WorkshopO__Order__300424B4");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
