using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationSuite.WindowModules.BoatRaceDataBank.Store.Contract
{
    /// <summary>
    /// 最終日(last)・今日(today)・Seed から、最大 capDays までの日付レンジを決定。
    /// </summary>
    public sealed class DateRangePlanner
    {
        public sealed record Plan(DateOnly Start, DateOnly End, IReadOnlyList<DateOnly> Dates)
        {
            public bool IsEmpty => Dates.Count == 0;
        }

        public Plan Build(DateOnly? last, DateOnly today, DateOnly seed, int capDays)
        {
            if (capDays < 1) capDays = 1;

            var start = last.HasValue ? last.Value.AddDays(1) : seed;
            var maxEnd = start.AddDays(capDays - 1);
            var end = Min(today, maxEnd);

            if (start > end)
                return new Plan(start, end, Array.Empty<DateOnly>());

            var list = new List<DateOnly>();
            for (var d = start; d <= end; d = d.AddDays(1))
                list.Add(d);

            return new Plan(start, end, list);
        }

        private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;
    }
}
