using System;
using System.Collections.Generic;

namespace LKS_Mart_API.Entities;

public partial class Log
{
    public int Id { get; set; }

    public DateTime Waktu { get; set; }

    public string Aktivitas { get; set; } = null!;

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
