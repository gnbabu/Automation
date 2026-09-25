namespace AutomationAPI.Repositories
{
    // Isolated as its own static class specifically so this date-math (the highest-risk
    // part of the Recurring Schedule feature) can be reasoned about and verified on its
    // own, separate from the worker loop that consumes it. Uses local DateTime throughout
    // (not UTC), matching the rest of the scheduling pipeline (GETDATE() in
    // usp_ScheduleSingleTestCase, DateTime.Now-based polling in TestQueueWorker).
    public static class RecurrenceCalculator
    {
        public static DateTime ComputeNextRunDate(string recurrenceType, string daysOfWeek, int? dayOfMonth, TimeSpan timeOfDay, DateTime after)
        {
            return recurrenceType switch
            {
                "Daily" => NextDaily(timeOfDay, after),
                "Weekly" => NextWeekly(daysOfWeek, timeOfDay, after),
                "Monthly" => NextMonthly(dayOfMonth ?? 1, timeOfDay, after),
                _ => throw new ArgumentException($"Unknown RecurrenceType '{recurrenceType}'")
            };
        }

        private static DateTime NextDaily(TimeSpan timeOfDay, DateTime after)
        {
            var candidate = after.Date + timeOfDay;
            return candidate > after ? candidate : candidate.AddDays(1);
        }

        private static DateTime NextWeekly(string daysOfWeek, TimeSpan timeOfDay, DateTime after)
        {
            var days = ParseDaysOfWeek(daysOfWeek);
            if (days.Count == 0)
                days.Add((int)after.DayOfWeek);

            for (int i = 0; i <= 7; i++)
            {
                var candidateDate = after.Date.AddDays(i);
                if (!days.Contains((int)candidateDate.DayOfWeek))
                    continue;

                var candidate = candidateDate + timeOfDay;
                if (candidate > after)
                    return candidate;
            }

            // Unreachable given the 0..7 range always covers a full week, kept as a
            // defensive fallback rather than letting the loop fall through silently.
            return after.Date.AddDays(7) + timeOfDay;
        }

        private static DateTime NextMonthly(int dayOfMonth, TimeSpan timeOfDay, DateTime after)
        {
            var candidate = BuildMonthlyDate(after.Year, after.Month, dayOfMonth, timeOfDay);
            if (candidate > after)
                return candidate;

            var (year, month) = NextMonth(after.Year, after.Month);
            return BuildMonthlyDate(year, month, dayOfMonth, timeOfDay);
        }

        // Clamps to the target month's actual last day - e.g. DayOfMonth=31 in February
        // resolves to the 28th (or 29th in a leap year), not an exception.
        private static DateTime BuildMonthlyDate(int year, int month, int dayOfMonth, TimeSpan timeOfDay)
        {
            var clampedDay = Math.Min(dayOfMonth, DateTime.DaysInMonth(year, month));
            return new DateTime(year, month, clampedDay) + timeOfDay;
        }

        private static (int Year, int Month) NextMonth(int year, int month)
        {
            return month == 12 ? (year + 1, 1) : (year, month + 1);
        }

        private static HashSet<int> ParseDaysOfWeek(string daysOfWeek)
        {
            var result = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(daysOfWeek))
                return result;

            foreach (var part in daysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out var day) && day is >= 0 and <= 6)
                    result.Add(day);
            }

            return result;
        }
    }
}
