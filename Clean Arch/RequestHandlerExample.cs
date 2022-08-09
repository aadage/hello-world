namespace TimeTrackerCore;

public class GetLogEntriesQueryHandler : IRequestHandler<GetLogEntriesQuery, GetLogEntriesResult>
{
    private readonly ILogRepository repository;
    private readonly ITimeTrackerOptions options;
    private GetLogEntriesQueryValidator validator = new GetLogEntriesQueryValidator();

    public GetLogEntriesQueryHandler(ILogRepository repository, ITimeTrackerOptions options)
    {
        this.repository = repository;
        this.options = options;
    }

    public async Task<GetLogEntriesResult> Handle(GetLogEntriesQuery request)
    {
        var validationErrors = validator.Validate(request);
        var logEntries = new List<LogEntry>();
        bool includeArchives = request.StartDate.HasValue && request.EndDate.HasValue;

        if (validationErrors.Count == 0)
        {
            logEntries = (await repository.GetAll(includeArchives)).OrderBy(le => le.Timestamp).ToList();

            if (request.StartDate.HasValue)
            {
                logEntries = logEntries.Where(le => le.Timestamp.Date >= request.StartDate.Value.ToDateTime(new TimeOnly(0, 0))).ToList();
            }

            if (request.EndDate.HasValue)
            {
                logEntries = logEntries.Where(le => le.Timestamp.Date <= request.EndDate.Value.ToDateTime(new TimeOnly(0, 0))).ToList();
            }
        }

        var result = CalculateTimeTotals(logEntries);
        result.ValidationErrors = validationErrors;

        return result;
    }

    private GetLogEntriesResult CalculateTimeTotals(List<LogEntry> logEntries)
    {
        LogEntry blockFirstEntry = null;
        LogEntry lastEntry = null;
        DateTime? currentDate = null;
        TimeSpan currentDateTotal = TimeSpan.FromMinutes(0);
        Dictionary<DateOnly, TimeSpan> dailyTimeTotals = new Dictionary<DateOnly, TimeSpan>();
        List<DailyTotalByEntryDescription> dailyTotalsByEntryDescription = new List<DailyTotalByEntryDescription>();
        TimeSpan totalTime = TimeSpan.FromMinutes(0);

        if (logEntries.Count == 0) return new GetLogEntriesResult(logEntries, dailyTimeTotals, dailyTotalsByEntryDescription, totalTime);

        foreach (var logEntry in logEntries.OrderBy(le => le.Timestamp))
        {
            if (currentDate is not null && currentDate.Value.Date != logEntry.Timestamp.Date)
            {
                totalTime = totalTime.Add(currentDateTotal);

                dailyTimeTotals.Add(DateOnly.FromDateTime(currentDate.Value), currentDateTotal);

                currentDateTotal = TimeSpan.FromMinutes(0);

                currentDate = logEntry.Timestamp.Date;
                blockFirstEntry = null;
            }

            if (currentDate is null) currentDate = logEntry.Timestamp.Date;
            if (blockFirstEntry is null && logEntry.Description != options.OutEntryDescription) blockFirstEntry = logEntry;
            if (logEntry.Description == options.OutEntryDescription)
            {
                if (blockFirstEntry is not null)
                {
                    currentDateTotal += logEntry.Timestamp.Subtract(blockFirstEntry.Timestamp);
                    blockFirstEntry = null;
                }
            }

            #region Calculate daily totals by entry description
            if (lastEntry != null)
            {
                if (lastEntry.Timestamp.Date.Equals(logEntry.Timestamp.Date))
                {
                    var dailyTotByDesc = dailyTotalsByEntryDescription.FirstOrDefault(tot => tot.Date.Equals(DateOnly.FromDateTime(lastEntry.Timestamp)) && tot.Description == lastEntry.Description);

                    if (dailyTotByDesc == null && lastEntry.Description != options.OutEntryDescription)
                    {
                        dailyTotByDesc = new DailyTotalByEntryDescription { Date = DateOnly.FromDateTime(lastEntry.Timestamp), Description = lastEntry.Description, ElapsedTime = TimeSpan.Zero };
                        dailyTotalsByEntryDescription.Add(dailyTotByDesc);
                    }

                    if (dailyTotByDesc != null) dailyTotByDesc.ElapsedTime += logEntry.Timestamp.Subtract(lastEntry.Timestamp);
                }
            }

            lastEntry = logEntry;
            #endregion
        }

        totalTime = totalTime.Add(currentDateTotal);
        dailyTimeTotals.Add(DateOnly.FromDateTime(currentDate ?? DateTime.MinValue), currentDateTotal);

        return new GetLogEntriesResult(logEntries, dailyTimeTotals, dailyTotalsByEntryDescription, totalTime);
    }
}

