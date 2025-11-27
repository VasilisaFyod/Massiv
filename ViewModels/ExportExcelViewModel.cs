using Massiv.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace Massiv.ViewModels
{
    public class ExportExcelViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private readonly Window _window;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string? _validationMessage;
        private readonly int _choice;
        private readonly string _tableType;

        public ICommand CloseCommand { get; }
        public ICommand ExportCommand { get; }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                ValidationMessage = string.Empty;
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                ValidationMessage = string.Empty;
            }
        }

        public string? ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }

        public ExportExcelViewModel(Window window, MassivContext context, int choice, string tableType = null)
        {
            _window = window;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _choice = choice;
            _tableType = tableType;

            CloseCommand = new RelayCommand(OnClose);
            ExportCommand = new RelayCommand(async () => await ExportToExcelAsync());

            EndDate = DateTime.Today;
            StartDate = DateTime.Today.AddDays(-6);
        }

        private void OnClose()
        {
            _window.Close();
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                ValidationMessage = string.Empty;

                if (!StartDate.HasValue || !EndDate.HasValue)
                {
                    ValidationMessage = "Пожалуйста, укажите обе даты";
                    return;
                }

                if (StartDate > EndDate)
                {
                    ValidationMessage = "Дата начала не может быть позже даты окончания";
                    return;
                }

                if (_choice <= 2)
                {
                    await ExportOrdersAsync();
                }
                else
                {
                    await ExportLogistTablesAsync();
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка при экспорте: {ex.Message}";
                Debug.WriteLine($"Export error: {ex}");
            }
        }

        private async Task ExportOrdersAsync()
        {
            List<Order> orders;

            if (_choice == 1) 
            {
                orders = await _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .Where(o => (o.IsDeleted == false || o.IsDeleted == null) && o.IsCompleted == false)
                    .Where(o => o.OrderDate != null)
                    .Where(o => o.OrderDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                             && o.OrderDate.Value <= DateOnly.FromDateTime(EndDate.Value))
                    .OrderByDescending(o => o.OrderDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else if (_choice == 2) 
            {
                orders = await _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .Where(o => (o.IsDeleted == false || o.IsDeleted == null) && o.IsCompleted == true)
                    .Where(o => o.OrderDate != null)
                    .Where(o => o.OrderDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                             && o.OrderDate.Value <= DateOnly.FromDateTime(EndDate.Value))
                    .OrderByDescending(o => o.OrderDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                ValidationMessage = "Неверный выбор типа экспорта";
                return;
            }

            if (orders == null || orders.Count == 0)
            {
                ValidationMessage = "Нет заказов за выбранный период";
                return;
            }

            await ExportToExcelFile(orders, "Заказы");
        }

        private async Task ExportLogistTablesAsync()
        {
            List<LogistTable> tables;

            if (_choice == 3) 
            {
                var query = _context.LogistTables
                    .Where(o => (o.IsDeleted == false || o.IsDeleted == null) && o.IsCompleted == false)
                    .Where(o => o.PlanDate != null)
                    .Where(o => o.PlanDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                             && o.PlanDate.Value <= DateOnly.FromDateTime(EndDate.Value));
                    

                if (!string.IsNullOrEmpty(_tableType))
                {
                    query = query.Where(o => o.TableType == _tableType);
                }

                tables = await query
                    .OrderByDescending(o => o.PlanDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else if (_choice == 4) 
            {
                var query = _context.LogistTables
                    .Where(o => (o.IsDeleted == false || o.IsDeleted == null) && o.IsCompleted == true)
                    .Where(o => o.PlanDate != null)
                    .Where(o => o.PlanDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                             && o.PlanDate.Value <= DateOnly.FromDateTime(EndDate.Value));

                
                if (!string.IsNullOrEmpty(_tableType))
                {
                    query = query.Where(o => o.TableType == _tableType);
                }

                tables = await query
                    .OrderByDescending(o => o.PlanDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                ValidationMessage = "Неверный выбор типа экспорта";
                return;
            }

            if (tables == null || tables.Count == 0)
            {
                ValidationMessage = "Нет данных за выбранный период";
                return;
            }

            await ExportLogistToExcelFile(tables, "Логистические_таблицы");
        }

        private async Task ExportToExcelFile(List<Order> orders, string baseFileName)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Заказы");

                var headers = new string[]
                {
                    "№ заказа", "Дата заключения договора", "Дата оформления заказа",
                    "Дата отдачи заказа в производство", "Изделие", "Лист", "Материал",
                    "Стол", "Фасады", "м2", "Цвет", "№ заказов", "Дизайнер", "Конструктор",
                    "Дата сдачи", "Дата готовности", "Дата отгрузки", "Телефон клиента", "Дата оповещения"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    worksheet.Cell(1, col + 1).Value = headers[col];
                }

                for (int i = 0; i < orders.Count; i++)
                {
                    var order = orders[i];
                    if (order == null) continue;

                    worksheet.Cell(i + 2, 1).Value = order.NumberOrder ?? string.Empty;
                    worksheet.Cell(i + 2, 2).Value = order.ContractDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 3).Value = order.OrderDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 4).Value = order.ProductionStartDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 5).Value = order.Product ?? string.Empty;
                    worksheet.Cell(i + 2, 6).Value = order.List?.ToString() ?? string.Empty;
                    worksheet.Cell(i + 2, 7).Value = order.Material ?? string.Empty;
                    worksheet.Cell(i + 2, 8).Value = order.TableAvailable.HasValue ? (order.TableAvailable.Value ? "Да" : "Нет") : string.Empty;
                    worksheet.Cell(i + 2, 9).Value = order.Facade ?? string.Empty;
                    worksheet.Cell(i + 2, 10).Value = order.SquareMeters?.ToString() ?? string.Empty;
                    worksheet.Cell(i + 2, 11).Value = order.Color ?? string.Empty;

                    var workshopOrders = order.WorkshopOrders?
                        .Where(wo => wo != null)
                        .Select(wo => $"№ заказа - {wo.NumberWorkshopOrder} ({(wo.IsReady ? "готов" : "в работе")})")
                        ?? Enumerable.Empty<string>();
                    worksheet.Cell(i + 2, 12).Value = string.Join(", ", workshopOrders);

                    worksheet.Cell(i + 2, 13).Value = order.Designer ?? string.Empty;
                    worksheet.Cell(i + 2, 14).Value = order.Constructor ?? string.Empty;
                    worksheet.Cell(i + 2, 15).Value = order.CompletionDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 16).Value = order.ReadyDate ?? string.Empty;
                    worksheet.Cell(i + 2, 17).Value = order.ShipmentDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 18).Value = order.ClientPhone ?? string.Empty;
                    worksheet.Cell(i + 2, 19).Value = order.NotificationDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                }

                await SaveWorkbook(workbook, baseFileName);
            }
        }

        private async Task ExportLogistToExcelFile(List<LogistTable> tables, string baseFileName)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Логистические таблицы");

                var headers = new string[]
                {
                    "№ заказа", "Плановая дата отгрузки", "ЛДСП",
                    "Кр. 1.0", "Фурнитура", "Ручки", "Крепеж",
                    "Стол.", "Кр. к. ст.", "Кр. 3D", "Борт", "Цоколь", "Стекла", "Вытяжка",
                    "Мойка", "Ст. панель", "Дата отгрузки", 
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    worksheet.Cell(1, col + 1).Value = headers[col];
                }

                for (int i = 0; i < tables.Count; i++)
                {
                    var table = tables[i];
                    if (table == null) continue;

                    worksheet.Cell(i + 2, 1).Value = table.OrderNumber ?? string.Empty;
                    worksheet.Cell(i + 2, 2).Value = table.PlanDate?.ToString("dd.MM.yyyy") ?? string.Empty;
                    worksheet.Cell(i + 2, 3).Value = table.Ldsp ?? string.Empty;
                    worksheet.Cell(i + 2, 4).Value = table.Kr1 ?? string.Empty;
                    worksheet.Cell(i + 2, 5).Value = table.Furniture ?? string.Empty;
                    worksheet.Cell(i + 2, 6).Value = table.Hands ?? string.Empty;
                    worksheet.Cell(i + 2, 7).Value = table.Anchor ?? string.Empty;
                    worksheet.Cell(i + 2, 8).Value = table.Table ?? string.Empty;
                    worksheet.Cell(i + 2, 9).Value = table.KrTable ?? string.Empty;
                    worksheet.Cell(i + 2, 10).Value = table.Kr3D ?? string.Empty;
                    worksheet.Cell(i + 2, 11).Value = table.Side ?? string.Empty;
                    worksheet.Cell(i + 2, 12).Value = table.Base ?? string.Empty;
                    worksheet.Cell(i + 2, 13).Value = table.Glass ?? string.Empty;
                    worksheet.Cell(i + 2, 14).Value = table.RangeHood ?? string.Empty;
                    worksheet.Cell(i + 2, 15).Value = table.Wash ?? string.Empty;
                    worksheet.Cell(i + 2, 16).Value = table.Panel ?? string.Empty;
                    worksheet.Cell(i + 2, 17).Value = table.ShipmentDate ?? string.Empty;
                }

                await SaveWorkbook(workbook, baseFileName);
            }
        }

        private async Task SaveWorkbook(ClosedXML.Excel.XLWorkbook workbook, string baseFileName)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"{baseFileName}_{StartDate.Value:yyyy.MM.dd}-{EndDate.Value:yyyy.MM.dd}.xlsx",
                OverwritePrompt = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                try
                {
                    workbook.SaveAs(filePath);

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                            _window.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Файл сохранен, но не открыт: {ex.Message}",
                                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                            _window.Close();
                        }
                    }
                    else
                    {
                        ValidationMessage = "Не удалось сохранить файл";
                    }
                }
                catch (IOException ioEx)
                {
                    ValidationMessage = $"Ошибка сохранения файла: {ioEx.Message}";
                }
                catch (UnauthorizedAccessException)
                {
                    ValidationMessage = "Нет прав для сохранения файла в выбранной локации";
                }
            }
        }
    }
}