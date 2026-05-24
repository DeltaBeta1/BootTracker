using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BootTracker.Services
{
    public static class SystemBootLogService
    {
        public static Dictionary<string, int> GetBootCountsByDay(DateTime dateFrom, DateTime dateTo)
        {
            var result = new Dictionary<string, int>();

            try
            {
                var endDate = dateTo.Date.AddDays(1);
                using (var log = new EventLog("System"))
                {
                    int count = log.Entries.Count;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        EventLogEntry entry;
                        try { entry = log.Entries[i]; }
                        catch { continue; }

                        if (entry.TimeGenerated >= endDate)
                            continue;
                        if (entry.TimeGenerated < dateFrom.Date)
                            break;

                        if (entry.InstanceId == 12)
                        {
                            string day = entry.TimeGenerated.ToString("yyyy-MM-dd");
                            if (result.ContainsKey(day))
                                result[day]++;
                            else
                                result[day] = 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EventLog error: " + ex.Message);
            }

            return result;
        }

        public static List<CompareItem> Compare(DateTime dateFrom, DateTime dateTo)
        {
            var sysCounts = GetBootCountsByDay(dateFrom, dateTo);
            var db = DatabaseService.Instance;
            var recData = db.GetStatsByDay(dateFrom.ToString("yyyy-MM-dd"), dateTo.ToString("yyyy-MM-dd"));

            var recCounts = new Dictionary<string, int>();
            foreach (var d in recData)
                recCounts[d.Day] = d.Count;

            var allDays = new SortedSet<string>(sysCounts.Keys);
            foreach (var day in recCounts.Keys)
                allDays.Add(day);

            var result = new List<CompareItem>();
            foreach (var day in allDays)
            {
                result.Add(new CompareItem
                {
                    Day = day,
                    SystemCount = sysCounts.ContainsKey(day) ? sysCounts[day] : 0,
                    RecordedCount = recCounts.ContainsKey(day) ? recCounts[day] : 0,
                });
            }
            return result;
        }
    }

    public class CompareItem
    {
        public string Day { get; set; }
        public int SystemCount { get; set; }
        public int RecordedCount { get; set; }
        public bool Match { get { return SystemCount == RecordedCount; } }
    }
}
