using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AgendaLicitacoes
{
    public sealed partial class MainWindow : Window
    {
        private DataService _dataService;
        private FiltroState _currentFilter;
        private DateTime _currentDate;
        private string _currentView = "Month";

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _currentFilter = new FiltroState();
            _currentDate = DateTime.Now;
            
            InitializeFilters();
            ApplyBackdrop();
            RefreshAll();
        }

        private void ApplyBackdrop()
        {
            var accentColor = (Color)this.Resources["AccentColor"];
            var bgBrush = (SolidColorBrush)this.Resources["BgBrush"];
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
        }

        private void InitializeFilters()
        {
            // Years
            var currentYear = DateTime.Now.Year;
            for (int i = currentYear - 5; i <= currentYear + 2; i++)
            {
                CmbAno.Items.Add(i.ToString());
            }
            CmbAno.SelectedValue = currentYear.ToString();

            // States
            var states = new[] { "SP", "RJ", "MG", "BA", "CE", "SC", "RS", "DF", "GO", "MT", "PR", "ES", "PE", "PA", "PB", "MA", "AL", "RN", "PI", "AM", "RO", "AC", "AP", "TO", "MS" };
            foreach (var state in states)
            {
                CmbEst.Items.Add(state);
            }

            // Status
            foreach (var status in Enum.GetNames(typeof(StatusLicitacao)))
            {
                CmbSt.Items.Add(status);
            }
        }

        private void RefreshAll()
        {
            try
            {
                var results = _dataService.Filtrar(_currentFilter);
                UpdateStats();
                
                if (_currentView == "List" || _currentFilter.HasActiveFilter())
                {
                    ShowListView(results);
                }
                else
                {
                    ShowCalendarView();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erro ao atualizar: {ex.Message}");
            }
        }

        private void ShowListView(List<Licitacao> items)
        {
            CalendarView.Visibility = Visibility.Collapsed;
            ListViewGrid.Visibility = Visibility.Visible;
            
            var gridItems = items.Select(l => new
            {
                Status = l.Status.ToString(),
                Tipo = l.Tipo.ToString(),
                Numero = l.Numero,
                MunicipioUF = $"{l.Municipio}/{l.Estado}",
                DataDisputa = l.DataDisputa.ToString("dd/MM/yyyy"),
                Portal = l.Portal,
                Orgao = l.Orgao,
                Produtos = l.Produtos,
                Source = l
            }).ToList();

            ListViewGrid.ItemsSource = gridItems;
        }

        private void ShowCalendarView()
        {
            CalendarView.Visibility = Visibility.Visible;
            ListViewGrid.Visibility = Visibility.Collapsed;
            CalendarView.Refresh(_currentDate, _currentView, _dataService);
        }

        private void UpdateStats()
        {
            var stats = _dataService.EstatisticasMes();
            StatGanho.Text = stats.ContainsKey(StatusLicitacao.Ganho) ? stats[StatusLicitacao.Ganho].ToString() : "0";
            StatPerdido.Text = stats.ContainsKey(StatusLicitacao.Perdido) ? stats[StatusLicitacao.Perdido].ToString() : "0";
            StatSuspenso.Text = stats.ContainsKey(StatusLicitacao.Suspenso) ? stats[StatusLicitacao.Suspenso].ToString() : "0";
            StatNaoCodificado.Text = stats.ContainsKey(StatusLicitacao.NaoCodificado) ? stats[StatusLicitacao.NaoCodificado].ToString() : "0";
            
            var total = stats.Values.Sum();
            StatTotal.Text = total.ToString();
            LblResultados.Text = $"{total} resultados";
        }

        // Event handlers
        private void BtnNovaLicitacao_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FormLicitacaoDialog(null, _dataService);
            _ = dialog.ShowAsync();
            RefreshAll();
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentView switch
            {
                "Month" => _currentDate.AddMonths(-1),
                "Week" => _currentDate.AddDays(-7),
                "Year" => _currentDate.AddYears(-1),
                _ => _currentDate
            };
            RefreshAll();
        }

        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Now;
            RefreshAll();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentView switch
            {
                "Month" => _currentDate.AddMonths(1),
                "Week" => _currentDate.AddDays(7),
                "Year" => _currentDate.AddYears(1),
                _ => _currentDate
            };
            RefreshAll();
        }

        private void ViewToggle_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            _currentView = btn?.Tag?.ToString() ?? "Month";
            RefreshAll();
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfigDialog(_dataService);
            _ = dialog.ShowAsync();
        }

        private void TxtBusca_TextChanged(TextBox sender, TextBoxTextChangedEventArgs args)
        {
            _currentFilter.Busca = sender.Text;
            RefreshAll();
        }

        private void CmbEst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentFilter.Estado = CmbEst.SelectedItem?.ToString();
            RefreshAll();
        }

        private void CmbMun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentFilter.Municipio = CmbMun.SelectedItem?.ToString();
            RefreshAll();
        }

        private void CmbAno_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentFilter.Ano = CmbAno.SelectedItem?.ToString();
            RefreshAll();
        }

        private void CmbSt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = CmbSt.SelectedItem?.ToString();
            _currentFilter.Status = selected != null && Enum.TryParse<StatusLicitacao>(selected, out var status) ? status : null;
            RefreshAll();
        }

        private void DtInicio_DateChanged(DatePicker sender, DatePickerValueChangedEventArgs args)
        {
            _currentFilter.DataInicio = args.NewDate?.DateTime;
            RefreshAll();
        }

        private void DtFim_DateChanged(DatePicker sender, DatePickerValueChangedEventArgs args)
        {
            _currentFilter.DataFim = args.NewDate?.DateTime;
            RefreshAll();
        }

        private void TxtItem_TextChanged(TextBox sender, TextBoxTextChangedEventArgs args)
        {
            _currentFilter.FiltroItem = sender.Text;
            RefreshAll();
        }

        private void BtnLimparFiltros_Click(object sender, RoutedEventArgs e)
        {
            _currentFilter = new FiltroState();
            TxtBusca.Text = "";
            CmbEst.SelectedIndex = -1;
            CmbMun.SelectedIndex = -1;
            CmbAno.SelectedIndex = -1;
            CmbSt.SelectedIndex = -1;
            DtInicio.SelectedDate = null;
            DtFim.SelectedDate = null;
            TxtItem.Text = "";
            RefreshAll();
        }

        private void ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Erro",
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }
    }
}
