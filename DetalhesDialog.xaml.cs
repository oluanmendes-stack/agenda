using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AgendaLicitacoes
{
    public sealed partial class DetalhesDialog : Window
    {
        private Licitacao _licitacao;
        private DataService _dataService;
        private ObservableCollection<Item> _itens;
        private ObservableCollection<Anexo> _anexos;
        private ObservableCollection<HistoricoItem> _historico;

        public DetalhesDialog(Licitacao licitacao, DataService dataService)
        {
            InitializeComponent();
            _licitacao = licitacao;
            _dataService = dataService;
            
            LoadData();
            UpdateHeaderColor();
        }

        private void LoadData()
        {
            HeaderTitle.Text = $"{_licitacao.GetSigla()} {_licitacao.Numero}";
            HeaderSubtitle.Text = $"{_licitacao.Municipio}/{_licitacao.Estado} - {_licitacao.DataDisputa:dd/MM/yyyy}";
            InfoStrip.Text = $"Status: {_licitacao.Status} | Órgão: {_licitacao.Orgao} | Portal: {_licitacao.Portal}";
            FooterInfo.Text = $"Criado: {_licitacao.DataCriacao:dd/MM/yyyy HH:mm} | Atualizado: {_licitacao.DataAtualizacao:dd/MM/yyyy HH:mm}";

            // Carregue o diário
            if (File.Exists(_licitacao.PastaServidor != null ? Path.Combine(_licitacao.PastaServidor, "Docs", "diario_do_processo.txt") : ""))
            {
                var diarioText = File.ReadAllText(Path.Combine(_licitacao.PastaServidor, "Docs", "diario_do_processo.txt"));
                DiarioxEdit.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, diarioText);
            }

            // Load items
            _itens = new ObservableCollection<Item>(_licitacao.Itens);
            ItensGrid.ItemsSource = _itens;

            // Load attachments
            _anexos = new ObservableCollection<Anexo>(_licitacao.Anexos);
            AnexosGrid.ItemsSource = _anexos;

            // Load history (reversed)
            _historico = new ObservableCollection<HistoricoItem>(_licitacao.Historico.OrderByDescending(h => h.DataHora));
            HistoricoGrid.ItemsSource = _historico;
        }

        private void UpdateHeaderColor()
        {
            var color = _licitacao.Status switch
            {
                StatusLicitacao.Ganho => new Windows.UI.Color { A = 255, R = 52, G = 168, B = 83 },
                StatusLicitacao.Perdido => new Windows.UI.Color { A = 255, R = 211, G = 59, B = 39 },
                StatusLicitacao.Suspenso => new Windows.UI.Color { A = 255, R = 234, G = 134, B = 0 },
                StatusLicitacao.Ata => new Windows.UI.Color { A = 255, R = 66, G = 133, B = 244 },
                _ => new Windows.UI.Color { A = 255, R = 26, G = 115, B = 232 }
            };
            HeaderBorder.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FormLicitacaoDialog(_licitacao, _dataService);
            _ = dialog.ShowAsync();
            // Refresh data after edit
            LoadData();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirmar Exclusão",
                Content = "Tem certeza que deseja excluir esta licitação?",
                PrimaryButtonText = "Excluir",
                CloseButtonText = "Cancelar"
            };
            _ = dialog.ShowAsync();
            
            _dataService.Remover(_licitacao.Id);
            this.Close();
        }

        private void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_licitacao.PastaServidor) && Directory.Exists(_licitacao.PastaServidor))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _licitacao.PastaServidor,
                    UseShellExecute = true
                });
            }
        }

        private void BtnLinkAta_Click(object sender, RoutedEventArgs e)
        {
            // Open file picker for ATA file
            ShowMessage("Funcionalidade de Linkar ATA será implementada em breve");
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNoteDialog();
            _ = dialog.ShowAsync();
        }

        private void SaveDiary_Click(object sender, RoutedEventArgs e)
        {
            DiarioxEdit.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out var text);
            _licitacao.Diario = text;
            _dataService.Atualizar(_licitacao);
            ShowMessage("Diário salvo com sucesso!");
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddItemDialog();
            _ = dialog.ShowAsync();
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItensGrid.SelectedItem is Item item)
            {
                var dialog = new EditItemDialog(item);
                _ = dialog.ShowAsync();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItensGrid.SelectedItem is Item item)
            {
                _licitacao.Itens.Remove(item);
                _itens.Remove(item);
                _dataService.Atualizar(_licitacao);
            }
        }

        private void MarkItemsWon_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _licitacao.Itens)
            {
                item.Ganho = true;
            }
            _dataService.Atualizar(_licitacao);
            LoadData();
        }

        private void MarkItemsLost_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _licitacao.Itens)
            {
                item.Ganho = false;
            }
            _dataService.Atualizar(_licitacao);
            LoadData();
        }

        private void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAttachmentDialog(_licitacao);
            _ = dialog.ShowAsync();
        }

        private void OpenAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (AnexosGrid.SelectedItem is Anexo anexo && File.Exists(anexo.Caminho))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = anexo.Caminho,
                    UseShellExecute = true
                });
            }
        }

        private void RemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (AnexosGrid.SelectedItem is Anexo anexo)
            {
                _licitacao.Anexos.Remove(anexo);
                _anexos.Remove(anexo);
                _dataService.Atualizar(_licitacao);
            }
        }

        private void ShowMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Informação",
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }
    }

    // Mini-dialogs
    public sealed partial class AddNoteDialog : ContentDialog
    {
        public string Note { get; private set; }

        public AddNoteDialog()
        {
            InitializeComponent();
            Title = "Adicionar Nota";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";
        }
    }

    public sealed partial class AddItemDialog : ContentDialog
    {
        public Item Item { get; private set; }

        public AddItemDialog()
        {
            InitializeComponent();
            Title = "Adicionar Item";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";
            Item = new Item();
        }
    }

    public sealed partial class EditItemDialog : ContentDialog
    {
        private Item _item;

        public EditItemDialog(Item item)
        {
            InitializeComponent();
            Title = "Editar Item";
            PrimaryButtonText = "Salvar";
            CloseButtonText = "Cancelar";
            _item = item;
        }
    }

    public sealed partial class AddAttachmentDialog : ContentDialog
    {
        private Licitacao _licitacao;

        public AddAttachmentDialog(Licitacao licitacao)
        {
            InitializeComponent();
            Title = "Adicionar Anexo";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";
            _licitacao = licitacao;
        }
    }
}
