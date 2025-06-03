using System;
using System.Collections.Generic;

namespace LKS_Mart_API.Entities;

public partial class Pelanggan
{
    public int Id { get; set; }

    public string Nama { get; set; } = null!;

    public string Telepon { get; set; } = null!;

    public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
}
