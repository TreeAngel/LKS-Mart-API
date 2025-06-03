using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LKS_Mart_API.Entities;

public partial class User
{
    public int Id { get; set; }

    [JsonIgnore]
    public string Tipe { get; set; } = null!;

    public string Nama { get; set; } = null!;

    public string Alamat { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Telepon { get; set; } = null!;

    [JsonIgnore]
    public string Password { get; set; } = null!;

    public string Image { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    [JsonIgnore]
    public virtual ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
}
