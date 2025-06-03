using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LKS_Mart_API.Entities;

public partial class MartDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MartDbContext()
    {
    }

    public MartDbContext(DbContextOptions<MartDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Barang> Barangs { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Pelanggan> Pelanggans { get; set; }

    public virtual DbSet<Transaksi> Transaksis { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost\\sqlexpress;Initial Catalog=SimpleMart;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Barang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Bareng");

            entity.ToTable("Barang");

            entity.HasIndex(e => e.Kode, "IX_Bareng").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpiredDate).HasColumnName("expired_date");
            entity.Property(e => e.HargaSatuan).HasColumnName("harga_satuan");
            entity.Property(e => e.Jumlah).HasColumnName("jumlah");
            entity.Property(e => e.Kode)
                .HasMaxLength(50)
                .HasColumnName("kode");
            entity.Property(e => e.Nama)
                .HasMaxLength(50)
                .HasColumnName("nama");
            entity.Property(e => e.Satuan)
                .HasMaxLength(50)
                .HasColumnName("satuan");
            entity.Property(e => e.Image)
                .HasMaxLength(int.MaxValue)
                .HasColumnName("image");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Aktivitas)
                .HasMaxLength(50)
                .HasColumnName("aktivitas");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Waktu)
                .HasColumnType("datetime")
                .HasColumnName("waktu");

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_User");
        });

        modelBuilder.Entity<Pelanggan>(entity =>
        {
            entity.ToTable("Pelanggan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nama)
                .HasMaxLength(50)
                .HasColumnName("nama");
            entity.Property(e => e.Telepon)
                .HasMaxLength(50)
                .HasColumnName("telepon");
        });

        modelBuilder.Entity<Transaksi>(entity =>
        {
            entity.ToTable("Transaksi");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BarangId).HasColumnName("barang_id");
            entity.Property(e => e.NamaKasir)
                .HasMaxLength(50)
                .HasColumnName("nama_kasir");
            entity.Property(e => e.No)
                .HasMaxLength(50)
                .HasColumnName("no");
            entity.Property(e => e.PelangganId).HasColumnName("pelanggan_id");
            entity.Property(e => e.Tanggal).HasColumnName("tanggal");
            entity.Property(e => e.TotalBayar).HasColumnName("total_bayar");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Barang).WithMany(p => p.Transaksis)
                .HasForeignKey(d => d.BarangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaksi_Bareng");

            entity.HasOne(d => d.Pelanggan).WithMany(p => p.Transaksis)
                .HasForeignKey(d => d.PelangganId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaksi_Pelanggan");

            entity.HasOne(d => d.User).WithMany(p => p.Transaksis)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaksi_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "IX_User").IsUnique();

            entity.HasIndex(e => e.Telepon, "IX_User_1").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Alamat).HasColumnName("alamat");
            entity.Property(e => e.Nama)
                .HasMaxLength(50)
                .HasColumnName("nama");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.Telepon)
                .HasMaxLength(50)
                .HasColumnName("telepon");
            entity.Property(e => e.Tipe)
                .HasMaxLength(50)
                .HasColumnName("tipe");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.Image)
                .HasMaxLength(int.MaxValue)
                .HasColumnName("image");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
