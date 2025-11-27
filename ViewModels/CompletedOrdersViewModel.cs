using Massiv.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace Massiv.ViewModels
{
    public class CompletedOrdersViewModel : BaseOrdersViewModel
    {
        public override bool IsButtonAddVisible => false;
        public override bool IsButtonExportVisible => true;

        public CompletedOrdersViewModel(MassivContext context) : base(context) { }

        protected override IQueryable<Order> GetBaseQuery()
        {
            return _context.Orders
                .Where(o => !o.IsDeleted.Value && o.IsCompleted.Value);
        }

        protected override async Task LoadOrdersAsync()
        {
            try
            {
                var orders = await GetBaseQuery()
                    .OrderByDescending(o => o.OrderDate)
                    .AsNoTracking()
                    .ToListAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        protected override int GetExportType() => 2;
    }
}