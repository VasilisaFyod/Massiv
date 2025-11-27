using Massiv.Models;

namespace Massiv.ViewModels
{
    public class OrdersViewModel : BaseOrdersViewModel
    {
        public override bool IsButtonAddVisible => true;
        public override bool IsButtonExportVisible => true;

        public OrdersViewModel(MassivContext context) : base(context) { }

        protected override IQueryable<Order> GetBaseQuery()
        {
            return _context.Orders
                .Where(o => !o.IsDeleted.Value && !o.IsCompleted.Value);
        }

        protected override int GetExportType() => 1;
    }
}