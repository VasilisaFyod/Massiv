using LiveCharts;
using LiveCharts.Wpf;
using Massiv.Models;
using Massiv.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Massiv.ViewModels
{
    public class AdminChartsViewModel : ViewModelBase
    {
        private readonly OrderStatisticsService _statisticsService;
        private OrderStatistics _statistics = new();
        private DateTime _startDate = DateTime.Today.AddMonths(-6);
        private DateTime _endDate = DateTime.Today;
        private string _selectedChartType = "StatusDistribution";

        public SeriesCollection StatusSeries { get; set; }
        public SeriesCollection MonthlySeries { get; set; }
        public string[] MonthlyLabels { get; set; }

        private readonly Color[] _pieChartColors = new[]
        {
            Color.FromRgb(0, 128, 0),     
            Color.FromRgb(133, 204, 250), 
            Color.FromRgb(255, 165, 0),  
            Color.FromRgb(255, 107, 0),  
            Color.FromRgb(255, 0, 0),   
            Color.FromRgb(128, 128, 128)  
        };

        public OrderStatistics Statistics
        {
            get => _statistics;
            set
            {
                _statistics = value;
                OnPropertyChanged(nameof(Statistics));
                OnPropertyChanged(nameof(StatusData));
                OnPropertyChanged(nameof(MonthlyData));
            }
        }

        public ObservableCollection<ChartData> StatusData =>
            new ObservableCollection<ChartData>(Statistics.StatusDistribution);

        public ObservableCollection<ChartData> MonthlyData =>
            new ObservableCollection<ChartData>(Statistics.MonthlyTrend);

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public string SelectedChartType
        {
            get => _selectedChartType;
            set
            {
                _selectedChartType = value;
                OnPropertyChanged(nameof(SelectedChartType));
            }
        }

        public ObservableCollection<string> ChartTypes { get; } = new()
        {
            "Распределение по статусам",
            "Динамика по месяцам"
        };

        public ICommand LoadChartsCommand { get; }

        public AdminChartsViewModel(MassivContext context)
        {
            _statisticsService = new OrderStatisticsService(context);
            LoadChartsCommand = new RelayCommand(async () => await LoadChartsAsync());

            LoadChartsCommand.Execute(null);
        }

        private void UpdateCharts()
        {
            UpdateStatusChart();
            UpdateMonthlyChart();
        }

        private void UpdateStatusChart()
        {
            StatusSeries = new SeriesCollection();
            int colorIndex = 0;

            foreach (var item in Statistics.StatusDistribution)
            {
                var color = TryParseColor(item.Color) ?? _pieChartColors[colorIndex % _pieChartColors.Length];

                StatusSeries.Add(new PieSeries
                {
                    Title = $"{item.Label} ({item.Value})",
                    Values = new ChartValues<int> { item.Value },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush(color),
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 2
                });

                colorIndex++;
            }

            OnPropertyChanged(nameof(StatusSeries));
        }

        private void UpdateMonthlyChart()
        {
            MonthlySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Заказов",
                    Values = new ChartValues<int>(Statistics.MonthlyTrend.Select(x => x.Value)),
                    Fill = new SolidColorBrush(Color.FromRgb(147, 112, 219)), 
                    Stroke = new SolidColorBrush(Color.FromRgb(102, 51, 153)),
                    StrokeThickness = 1,
                    MaxColumnWidth = 60
                }
            };

            MonthlyLabels = Statistics.MonthlyTrend.Select(x => x.Label).ToArray();

            OnPropertyChanged(nameof(MonthlySeries));
            OnPropertyChanged(nameof(MonthlyLabels));
        }

        private Color? TryParseColor(string colorString)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(colorString);
            }
            catch
            {
                return null;
            }
        }

        private async Task LoadChartsAsync()
        {
            try
            {
                var start = DateOnly.FromDateTime(StartDate);
                var end = DateOnly.FromDateTime(EndDate);

                Statistics = await _statisticsService.GetStatisticsAsync(start, end);
                UpdateCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}