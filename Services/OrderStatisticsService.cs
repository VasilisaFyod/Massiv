using Massiv.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Massiv.Services
{
    public class OrderStatisticsService
    {
        private readonly MassivContext _context;

        public OrderStatisticsService(MassivContext context)
        {
            _context = context;
        }

        public async Task<OrderStatistics> GetStatisticsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var statistics = new OrderStatistics();

            var query = _context.Orders
                .Where(o => !o.IsDeleted.Value)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var orders = await query.ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);

            statistics.TotalOrders = orders.Count;
            statistics.CompletedOrders = orders.Count(o => o.IsCompleted == true);
            statistics.ActiveOrders = orders.Count(o => o.IsCompleted == false);

            statistics.OverdueOrders = orders.Count(o =>
                !o.IsCompleted.Value &&
                o.CompletionDate.HasValue &&
                o.CompletionDate.Value < today);

            statistics.OnTimeOrders = orders.Count(o =>
                o.IsCompleted.Value &&
                o.CompletionDate.HasValue &&
                o.ShipmentDate.HasValue &&
                o.CompletionDate.Value <= o.ShipmentDate.Value);

            statistics.OnTimePercentage = statistics.CompletedOrders > 0
                ? Math.Round((decimal)statistics.OnTimeOrders / statistics.CompletedOrders * 100, 1)
                : 0;

            statistics.StatusDistribution = CalculateStatusDistribution(orders);
            statistics.MonthlyTrend = CalculateMonthlyTrend(orders, startDate, endDate);

            return statistics;
        }

        private List<Models.ChartData> CalculateStatusDistribution(List<Order> orders)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var statusGroups = new Dictionary<string, (int Count, string Color)>
            {
                ["Выполнено"] = (orders.Count(o => o.IsCompleted == true), "#008000"),
                ["В работе"] = (orders.Count(o => !o.IsCompleted.Value &&
                    (!o.CompletionDate.HasValue || o.CompletionDate.Value > today.AddDays(21))), "#85ccfa"),
                ["Средний приоритет"] = (orders.Count(o => !o.IsCompleted.Value &&
                    o.CompletionDate.HasValue &&
                    o.CompletionDate.Value > today &&
                    o.CompletionDate.Value <= today.AddDays(21)), "#ffa500"),
                ["Срочно"] = (orders.Count(o => !o.IsCompleted.Value &&
                    o.CompletionDate.HasValue &&
                    o.CompletionDate.Value > today &&
                    o.CompletionDate.Value <= today.AddDays(7)), "#FF6B00"),
                ["Просрочено"] = (orders.Count(o => !o.IsCompleted.Value &&
                    o.CompletionDate.HasValue &&
                    o.CompletionDate.Value < today), "#ff0000"),
                ["Нет даты"] = (orders.Count(o => !o.IsCompleted.Value && !o.CompletionDate.HasValue), "#808080")
            };

            return statusGroups
                .Where(g => g.Value.Count > 0)
                .Select(g => new Models.ChartData
                {
                    Label = g.Key,
                    Value = g.Value.Count,
                    Percentage = orders.Count > 0 ? (double)g.Value.Count / orders.Count * 100 : 0,
                    Color = g.Value.Color
                })
                .OrderByDescending(x => x.Value)
                .ToList();
        }

        private List<Models.ChartData> CalculateMonthlyTrend(List<Order> orders, DateOnly? startDate, DateOnly? endDate)
        {
            var actualStartDate = startDate ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(-5));
            var actualEndDate = endDate ?? DateOnly.FromDateTime(DateTime.Today);

            var monthsInRange = GetMonthsBetween(actualStartDate, actualEndDate);

            var monthlyData = orders
                .Where(o => o.OrderDate.HasValue)
                .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Completed = g.Count(o => o.IsCompleted == true)
                })
                .ToList();

            return monthsInRange
                .Select(month =>
                {
                    var data = monthlyData.FirstOrDefault(m =>
                        m.Year == month.Year && m.Month == month.Month);

                    var monthName = new DateTime(month.Year, month.Month, 1)
                        .ToString("MMM yyyy", new System.Globalization.CultureInfo("ru-RU"));

                    return new Models.ChartData
                    {
                        Label = monthName,
                        Value = data?.Count ?? 0,
                        Percentage = data?.Count > 0 ? (double)data.Completed / data.Count * 100 : 0
                    };
                })
                .ToList();
        }

        private IEnumerable<(int Year, int Month)> GetMonthsBetween(DateOnly startDate, DateOnly endDate)
        {
            var current = new DateTime(startDate.Year, startDate.Month, 1);
            var end = new DateTime(endDate.Year, endDate.Month, 1);

            while (current <= end)
            {
                yield return (current.Year, current.Month);
                current = current.AddMonths(1);
            }
        }
    }
}