using System;
using System.Collections.Generic;

namespace ProjectIvy.Jobs.DistanceCovered.DbModels;

public partial class Tracking
{
    public int Id { get; set; }

    public double? Accuracy { get; set; }

    public double? Altitude { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public DateTime Timestamp { get; set; }

    public double? Speed { get; set; }

    public int UserId { get; set; }

    public string Geohash { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
