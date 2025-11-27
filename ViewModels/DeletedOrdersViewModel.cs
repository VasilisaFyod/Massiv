using Massiv.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class DeletedOrdersViewModel : BaseOrdersViewModel
    {
        public override bool IsButtonAddVisible => false;
        public override bool IsButtonExportVisible => false;


        public DeletedOrdersViewModel(MassivContext context) : base(context)
        {
           
        }

        protected override IQueryable<Order> GetBaseQuery()
        {
            return _context.Orders
                .Where(o => o.IsDeleted.Value);
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

        protected override int GetExportType() => 3;

        protected override void AddOrder() { }
        protected override void ExportToExcel() { }
        protected override void Menu() { }
    }
}