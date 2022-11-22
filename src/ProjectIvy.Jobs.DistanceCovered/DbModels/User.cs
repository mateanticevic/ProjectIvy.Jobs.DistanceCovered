using System;
using System.Collections.Generic;

namespace ProjectIvy.Jobs.DistanceCovered.DbModels;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Email { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime PasswordModified { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? BirthCityId { get; set; }

    public int? DefaultCarId { get; set; }

    public int DefaultCurrencyId { get; set; }

    public int DefaultLanguageId { get; set; }

    public bool IsTrackingEnabled { get; set; }

    public DateTime? TrackingStartDate { get; set; }

    public string? ImdbId { get; set; }

    public string? LastFmUsername { get; set; }

    public DateTime Created { get; set; }

    public DateTime Modified { get; set; }

    public string? AuthIdentifier { get; set; }

    public virtual ICollection<DistanceCovered> DistanceCovereds { get; } = new List<DistanceCovered>();

    public virtual ICollection<Tracking> Trackings { get; } = new List<Tracking>();
}
