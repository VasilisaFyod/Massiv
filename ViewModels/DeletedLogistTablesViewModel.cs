using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class DeletedLogistTablesViewModel : BaseLogistTablesViewModel
    {
        public override bool IsButtonAddVisible => false;
        public override bool IsButtonExportVisible => false;

        public DeletedLogistTablesViewModel(MassivContext context, string tableType = null)
            : base(context, tableType) { }

        protected override IQueryable<LogistTable> GetBaseQuery()
        {
            return _context.LogistTables
                .Where(lt => lt.IsDeleted == true);
        }

        protected override int GetExportType() => 4;
    }
}