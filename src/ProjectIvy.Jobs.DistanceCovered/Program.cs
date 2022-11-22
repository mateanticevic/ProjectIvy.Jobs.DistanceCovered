// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using ProjectIvy.Jobs.DistanceCovered.DbModels;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using GeoCoordinatePortable;

Configure();
Log.Logger.Information("Application started");

await ExecuteJob();

Log.Logger.Information("Application finished");

static void Configure()
{
    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                      .MinimumLevel.Override(nameof(Microsoft), LogEventLevel.Information)
                                      .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                                      .Enrich.FromLogContext()
                                      .WriteTo.Console()
                                      .WriteTo.Graylog(new GraylogSinkOptions()
                                      {
                                          Facility = "project-ivy-jobs-distancecovered",
                                          HostnameOrAddress = Environment.GetEnvironmentVariable("GRAYLOG_HOST"),
                                          Port = Convert.ToInt32(Environment.GetEnvironmentVariable("GRAYLOG_PORT")),
                                          TransportType = TransportType.Udp
                                      })
                                      .CreateLogger();
}

static async Task ExecuteJob()
{
    var db = new ProjectIvyContext();
    var userIds = db.Trackings.Select(x => x.UserId).Distinct().ToList();
    Log.Logger.Information("Found {UserCount} users", userIds.Count);

    foreach (int userId in userIds)
    {
        await ProcessUser(db, userId);
    }
}

static async Task ProcessUser(ProjectIvyContext db, int userId)
{
    Log.Logger.Information("Processing userId {UserId}", userId);
    var user = db.Users.Single(x => x.Id == userId);

    if (!user.IsTrackingEnabled || !user.TrackingStartDate.HasValue)
    {
        Log.Logger.Information("Tracking not enabled for {UserId}", userId);
        return;
    }

    var distanceCovered = db.DistanceCovereds.Where(x => x.UserId == userId)
                                     .OrderByDescending(x => x.To)
                                     .FirstOrDefault();

    var startDay = distanceCovered?.To.Date ?? user.TrackingStartDate.Value;
    Log.Logger.Information("Starting processing days for user {UserId} starting from {StartDay}", userId, startDay.ToString("yyyy-MM-dd"));

    var processingDay = startDay.AddDays(1);

    while(processingDay < DateTime.Now.Date.AddDays(-30))
    {
        await ProcessDay(db, userId, processingDay);
        processingDay = processingDay.AddDays(1);
    }

    return;
}

static async Task ProcessDay(ProjectIvyContext db, int userId, DateTime day)
{
    var trackings = await db.Trackings.Where(x => x.UserId == userId)
                                      .Where(x => x.Timestamp > day)
                                      .Where(x => x.Timestamp < day.AddDays(1))
                                      .OrderBy(x => x.Timestamp)
                                      .ToListAsync();

    Log.Logger.Information("Found {TrackingCount}, for user {UserId} on {Day}", trackings.Count, userId, day.ToString("yyyy-MM-dd"));

    if (trackings.Count == 0)
        return;

    var firstPreviousTracking = await db.Trackings.Where(x => x.UserId == userId)
                                                  .Where(x => x.Timestamp < trackings.First().Timestamp)
                                                  .OrderByDescending(x => x.Timestamp)
                                                  .FirstAsync();

    for (int hour = 0; hour < 24; hour++)
    {
        var hourTrackings = trackings.Where(x => x.Timestamp > day.Date.AddHours(hour))
                                     .Where(x => x.Timestamp < day.Date.AddHours(hour + 1))
                                     .OrderBy(x => x.Timestamp)
                                     .ToList();

        if (hourTrackings.Count == 0)
            continue;

        var previousTracking = hourTrackings[0].Id == trackings[0].Id ? firstPreviousTracking : trackings.Where(x => x.Timestamp < hourTrackings[0].Timestamp).OrderByDescending(x => x.Timestamp).First();

        var hourTrackingsWithPrevious = new Tracking[] { previousTracking }.Concat(hourTrackings).ToList();

        var dateTimeHour = hourTrackings.First().Timestamp.Date.AddHours(hourTrackings.First().Timestamp.Hour);

        await ProcessHour(db, userId, dateTimeHour, hourTrackingsWithPrevious);
    }
    await db.SaveChangesAsync();
}

static async Task ProcessHour(ProjectIvyContext db, int userId, DateTime dateTimeHour, IList<Tracking> trackings)
{
    double sum = 0;
    for (int i = 0; i < trackings.Count - 1; i++)
    {
        var a = new GeoCoordinate((double)trackings[i].Latitude, (double)trackings[i].Longitude, (double)(trackings[i].Altitude ?? 0));
        var b = new GeoCoordinate((double)trackings[i + 1].Latitude, (double)trackings[i + 1].Longitude, (double)(trackings[i + 1].Altitude ?? 0));

        sum += a.GetDistanceTo(b);
    }

    Log.Logger.Information("{Distance}m {TrackingCount} trackings for {UserId} for {DateTimeHour}", sum, trackings.Count, userId, dateTimeHour.ToString("yyyy-MM-dd HH"));

    var entity = new DistanceCovered()
    {
        Distance = (int)sum,
        From = dateTimeHour,
        To = dateTimeHour.AddHours(1),
        UserId = userId
    };

    await db.DistanceCovereds.AddAsync(entity);
    return;
}