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
        public ExportExcelViewModel(Window window, MassivContext context, int choice)
        {
            _window = window;
            _context = context ?? throw new ArgumentNullException(nameof(context));

            CloseCommand = new RelayCommand(OnClose);
            ExportCommand = new RelayCommand(async () => await ExportToExcelAsync(choice));

            EndDate = DateTime.Today;
            StartDate = DateTime.Today.AddDays(-6);
        }

        private void OnClose()
        {
            _window.Close();
        }

        private async Task ExportToExcelAsync(int choice)
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

                List<Order> orders;

                if (choice == 1)
                {
                    orders = await _context.Orders
                        .Include(o => o.WorkshopOrders)
                        .Where(o => (o.IsDeleted == false || o.IsDeleted == null)
                            && o.IsCompleted == false) 
                        .Where(o => o.OrderDate != null)
                        .Where(o => o.OrderDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                            && o.OrderDate.Value < DateOnly.FromDateTime(EndDate.Value))
                        .OrderByDescending(o => o.OrderDate)
                        .AsNoTracking()
                        .ToListAsync();
                }
                else if (choice == 2)
                {
                    orders = await _context.Orders
                        .Include(o => o.WorkshopOrders)
                        .Where(o => (o.IsDeleted == false || o.IsDeleted == null)
                            && o.IsCompleted == true)
                        .Where(o => o.OrderDate != null)
                        .Where(o => o.OrderDate.Value >= DateOnly.FromDateTime(StartDate.Value)
                            && o.OrderDate.Value < DateOnly.FromDateTime(EndDate.Value))
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

                string filePath = string.Empty;
                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add();

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

                    var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                    worksheet.Columns().AdjustToContents();

                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = $"Заказы_{StartDate.Value:yyyy.MM.dd}-{EndDate.Value:yyyy.MM.dd}.xlsx",
                        OverwritePrompt = true
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        filePath = saveFileDialog.FileName;
                        try
                        {
                            workbook.SaveAs(filePath);

                            if (File.Exists(filePath))
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Файл сохранен, но не открыт: {ex.Message}",
                                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                ValidationMessage = "Не удалось сохранить файл";
                                return;
                            }
                        }
                        catch (IOException ioEx)
                        {
                            ValidationMessage = $"Ошибка сохранения файла: {ioEx.Message}";
                            return;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            ValidationMessage = "Нет прав для сохранения файла в выбранной локации";
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                _window.Close();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка при экспорте: {ex.Message}";
                Debug.WriteLine($"Export error: {ex}");
            }
        }
    }
}