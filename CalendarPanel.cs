using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgendaLicitacoes
{
    public class CalendarPanel : Grid
    {
        private DataService _dataService;
        private DateTime _currentDate = DateTime.Now;
        private string _currentView = "Month";
        private Grid _contentGrid;

        public CalendarPanel()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 255, 255));
        }

        public void Refresh(DateTime date, string view, DataService dataService)
        {
            _currentDate = date;
            _currentView = view;
            _dataService = dataService;
            
            this.Children.Clear();
            
            switch (view)
            {
                case "Month":
                    RenderMonthView();
                    break;
                case "Week":
                    RenderWeekView();
                    break;
                case "Year":
                    RenderYearView();
                    break;
                default:
                    RenderMonthView();
                    break;
            }
        }

        private void RenderMonthView()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Header with weekdays
            var weekdays = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sab" };
            for (int col = 0; col < 7; col++)
            {
                var header = new TextBlock
                {
                    Text = weekdays[col],
                    FontWeight = Windows.UI.Text.FontWeights.Bold,
                    FontSize = 12,
                    Padding = new Thickness(4),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, col);
                grid.Children.Add(header);
            }

            // Month title
            var monthTitle = new TextBlock
            {
                Text = _currentDate.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")),
                FontSize = 18,
                FontWeight = Windows.UI.Text.FontWeights.Bold,
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(monthTitle, 0);
            Grid.SetColumnSpan(monthTitle, 7);

            var titleContainer = new Border
            {
                Child = monthTitle,
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 218, 220, 224)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 8, 0, 8)
            };
            Grid.SetColumnSpan(titleContainer, 7);
            grid.Children.Add(titleContainer);

            // Days
            var firstDay = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var startDay = firstDay.AddDays(-(int)firstDay.DayOfWeek);

            var events = _dataService.Filtrar(new FiltroState());
            var eventsByDate = events.GroupBy(e => e.DataDisputa.Date).ToDictionary(g => g.Key, g => g.ToList());

            int row = 1;
            int col = 0;
            var date = startDay;

            while (date <= lastDay || col > 0)
            {
                var dayCell = CreateDayCell(date, _currentDate.Month, eventsByDate.ContainsKey(date) ? eventsByDate[date] : new List<Licitacao>());
                Grid.SetRow(dayCell, row);
                Grid.SetColumn(dayCell, col);
                grid.Children.Add(dayCell);

                date = date.AddDays(1);
                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
            }

            this.Children.Add(grid);
        }

        private Border CreateDayCell(DateTime date, int currentMonth, List<Licitacao> dayEvents)
        {
            var isCurrentMonth = date.Month == currentMonth;
            var isToday = date.Date == DateTime.Now.Date;

            var dayNumber = new TextBlock
            {
                Text = date.Day.ToString(),
                FontSize = 14,
                FontWeight = isToday ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Normal,
                Foreground = isCurrentMonth ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 60, 64, 67)) : new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 200, 200, 200)),
                Margin = new Thickness(4),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var eventStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(4, 20, 4, 4),
                Spacing = 2
            };

            foreach (var evt in dayEvents.Take(3))
            {
                var eventBar = new TextBlock
                {
                    Text = $"{evt.GetSigla()} {evt.Numero}",
                    FontSize = 10,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                    Padding = new Thickness(2),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Background = GetStatusBrush(evt.Status)
                };
                eventStack.Children.Add(eventBar);
            }

            if (dayEvents.Count > 3)
            {
                var moreText = new TextBlock
                {
                    Text = $"+{dayEvents.Count - 3} mais",
                    FontSize = 9,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 26, 115, 232)),
                    Padding = new Thickness(2)
                };
                eventStack.Children.Add(moreText);
            }

            var container = new Grid();
            container.Children.Add(dayNumber);
            container.Children.Add(eventStack);

            var border = new Border
            {
                Child = container,
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 218, 220, 224)),
                BorderThickness = new Thickness(1),
                Background = isToday ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 245, 245, 245)) : new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 255, 255))
            };

            return border;
        }

        private void RenderWeekView()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            for (int i = 0; i < 7; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var startDay = _currentDate.AddDays(-(int)_currentDate.DayOfWeek);
            var events = _dataService.Filtrar(new FiltroState());
            var eventsByDate = events.GroupBy(e => e.DataDisputa.Date).ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < 7; i++)
            {
                var date = startDay.AddDays(i);
                var dayCard = CreateWeekDayCard(date, eventsByDate.ContainsKey(date) ? eventsByDate[date] : new List<Licitacao>());
                Grid.SetColumn(dayCard, i);
                grid.Children.Add(dayCard);
            }

            this.Children.Add(grid);
        }

        private Border CreateWeekDayCard(DateTime date, List<Licitacao> dayEvents)
        {
            var dayName = date.ToString("ddd", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            var dayNumber = date.Day.ToString();

            var header = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(8)
            };
            header.Children.Add(new TextBlock { Text = dayName, FontWeight = Windows.UI.Text.FontWeights.Bold, FontSize = 12 });
            header.Children.Add(new TextBlock { Text = dayNumber, FontSize = 16, FontWeight = Windows.UI.Text.FontWeights.Bold });

            var eventStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Padding = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            foreach (var evt in dayEvents)
            {
                var eventCard = new Border
                {
                    Child = new TextBlock
                    {
                        Text = $"{evt.GetSigla()} {evt.Numero}\n{evt.Municipio}",
                        FontSize = 9,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                        Padding = new Thickness(4),
                        TextWrapping = TextWrapping.Wrap
                    },
                    Background = GetStatusBrush(evt.Status),
                    Padding = new Thickness(2),
                    CornerRadius = new CornerRadius(2)
                };
                eventStack.Children.Add(eventCard);
            }

            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 0
            };
            container.Children.Add(header);
            container.Children.Add(eventStack);

            var border = new Border
            {
                Child = container,
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 218, 220, 224)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0)
            };

            return border;
        }

        private void RenderYearView()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var events = _dataService.Filtrar(new FiltroState());

            for (int month = 1; month <= 12; month++)
            {
                var monthEvents = events.Where(e => e.DataDisputa.Month == month && e.DataDisputa.Year == _currentDate.Year).ToList();
                var monthCard = CreateMonthCard(month, _currentDate.Year, monthEvents.Count > 0);
                Grid.SetRow(monthCard, (month - 1) / 4);
                Grid.SetColumn(monthCard, (month - 1) % 4);
                grid.Children.Add(monthCard);

                if (Grid.GetRowSpan(grid) == 0 && month % 4 == 0)
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < 3; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            this.Children.Add(grid);
        }

        private Border CreateMonthCard(int month, int year, bool hasEvents)
        {
            var monthName = System.Globalization.CultureInfo.GetCultureInfo("pt-BR").DateTimeFormat.GetMonthName(month);
            var border = new Border
            {
                Child = new TextBlock
                {
                    Text = monthName,
                    FontSize = 16,
                    FontWeight = Windows.UI.Text.FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = hasEvents ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 26, 115, 232)) : new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 112, 117, 122))
                },
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 218, 220, 224)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4),
                Padding = new Thickness(16)
            };

            return border;
        }

        private Microsoft.UI.Xaml.Media.SolidColorBrush GetStatusBrush(StatusLicitacao status)
        {
            return status switch
            {
                StatusLicitacao.Ganho => new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 52, 168, 83)),
                StatusLicitacao.Perdido => new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 211, 59, 39)),
                StatusLicitacao.Suspenso => new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 234, 134, 0)),
                StatusLicitacao.Ata => new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 66, 133, 244)),
                _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 112, 117, 122))
            };
        }
    }
}
