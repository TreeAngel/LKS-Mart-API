using System;
using System.Collections.Generic;

namespace LKS_Mart_API.Entities;

public partial class Transaksi
{
    public int Id { get; set; }

    public string No { get; set; } = null!;

    public DateOnly Tanggal { get; set; }

    public string? NamaKasir { get; set; }

    public long TotalBayar { get; set; }

    public int UserId { get; set; }

    public int PelangganId { get; set; }

    public int BarangId { get; set; }

    public virtual Barang Barang { get; set; } = null!;

    public virtual Pelanggan Pelanggan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
