using System;
using System.Collections.Generic;

namespace ProjectIvy.Jobs.DistanceCovered.DbModels;

public partial class DistanceCovered
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public int Distance { get; set; }

    public virtual User User { get; set; } = null!;
}
