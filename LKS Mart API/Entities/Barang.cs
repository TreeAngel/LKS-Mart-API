using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LKS_Mart_API.Entities;

public partial class Barang
{
    public int Id { get; set; }

    public string Kode { get; set; } = null!;

    public string Nama { get; set; } = null!;

    public DateOnly ExpiredDate { get; set; }

    public long Jumlah { get; set; }

    public string Satuan { get; set; } = null!;

    public long HargaSatuan { get; set; }

    public string Image { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
}
