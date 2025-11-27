using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class CompletedLogistTablesViewModel : BaseLogistTablesViewModel
    {
        public override bool IsButtonAddVisible => false;
        public override bool IsButtonExportVisible => true;

        public CompletedLogistTablesViewModel(MassivContext context, string tableType = null, string userRole = null)
            : base(context, tableType, userRole) { }

        protected override IQueryable<LogistTable> GetBaseQuery()
        {
            return _context.LogistTables
                .Where(lt => lt.IsCompleted == true && lt.IsDeleted == false);
        }

        protected override int GetExportType() => 4;
    }
}