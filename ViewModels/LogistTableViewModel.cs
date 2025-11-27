using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class LogistTablesViewModel : BaseLogistTablesViewModel
    {
        public override bool IsButtonAddVisible => true;
        public override bool IsButtonExportVisible => true;

        public LogistTablesViewModel(MassivContext context, string tableType = null)
            : base(context, tableType) { }

        protected override IQueryable<LogistTable> GetBaseQuery()
        {
            return _context.LogistTables
                .Where(lt => lt.IsDeleted != true && lt.IsCompleted != true);
        }

        protected override int GetExportType() => 4; 
    }
}