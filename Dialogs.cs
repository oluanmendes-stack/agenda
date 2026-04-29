using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgendaLicitacoes
{
    // Configuration Dialog
    public sealed partial class ConfigDialog : ContentDialog
    {
        private DataService _dataService;
        private Config _config;

        public ConfigDialog(DataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            _config = _dataService.ObterConfig();
            
            Title = "Configurações";
            PrimaryButtonText = "Salvar";
            CloseButtonText = "Fechar";
            
            LoadConfig();
        }

        private void LoadConfig()
        {
            // Load configuration values
            // This is a basic implementation - expand based on your needs
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _dataService.SalvarConfig(_config);
        }
    }

    // Add Note Dialog
    public sealed class AddNoteDialog : ContentDialog
    {
        private TextBox _noteTextBox;

        public string NoteText { get; private set; }

        public AddNoteDialog() : base()
        {
            Title = "Adicionar Nota ao Diário";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";

            _noteTextBox = new TextBox
            {
                PlaceholderText = "Escreva sua nota aqui...",
                Height = 100,
                AcceptsReturn = true
            };

            this.Content = _noteTextBox;
        }

        protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            NoteText = _noteTextBox.Text;
            base.OnPrimaryButtonClick(args);
        }
    }

    // Add Item Dialog
    public sealed class AddItemDialog : ContentDialog
    {
        private TextBox _numeroBox;
        private TextBox _codigoBox;
        private TextBox _descricaoBox;
        private TextBox _quantidadeBox;
        private TextBox _unidadeBox;
        private TextBox _valorBox;

        public Item ResultItem { get; private set; }

        public AddItemDialog() : base()
        {
            Title = "Adicionar Item";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";

            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(16) };

            _numeroBox = new TextBox { PlaceholderText = "Número" };
            _codigoBox = new TextBox { PlaceholderText = "Código" };
            _descricaoBox = new TextBox { PlaceholderText = "Descrição" };
            _quantidadeBox = new TextBox { PlaceholderText = "Quantidade" };
            _unidadeBox = new TextBox { PlaceholderText = "Unidade" };
            _valorBox = new TextBox { PlaceholderText = "Valor Unitário" };

            stackPanel.Children.Add(new TextBlock { Text = "Número", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_numeroBox);

            stackPanel.Children.Add(new TextBlock { Text = "Código", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_codigoBox);

            stackPanel.Children.Add(new TextBlock { Text = "Descrição", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_descricaoBox);

            stackPanel.Children.Add(new TextBlock { Text = "Quantidade", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_quantidadeBox);

            stackPanel.Children.Add(new TextBlock { Text = "Unidade", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_unidadeBox);

            stackPanel.Children.Add(new TextBlock { Text = "Valor Unitário", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_valorBox);

            this.Content = new ScrollViewer { Content = stackPanel };
        }

        protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_numeroBox.Text) || string.IsNullOrWhiteSpace(_descricaoBox.Text))
            {
                args.Cancel = true;
                return;
            }

            ResultItem = new Item
            {
                Numero = _numeroBox.Text,
                Codigo = _codigoBox.Text,
                Descricao = _descricaoBox.Text,
                Quantidade = _quantidadeBox.Text,
                Unidade = _unidadeBox.Text,
                ValorUnitario = decimal.TryParse(_valorBox.Text, out var val) ? val : 0
            };

            base.OnPrimaryButtonClick(args);
        }
    }

    // Edit Item Dialog
    public sealed class EditItemDialog : ContentDialog
    {
        private Item _item;
        private TextBox _numeroBox;
        private TextBox _codigoBox;
        private TextBox _descricaoBox;
        private TextBox _quantidadeBox;
        private TextBox _unidadeBox;
        private TextBox _valorBox;
        private ComboBox _resultadoBox;

        public EditItemDialog(Item item) : base()
        {
            _item = item;
            Title = "Editar Item";
            PrimaryButtonText = "Salvar";
            CloseButtonText = "Cancelar";

            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(16) };

            _numeroBox = new TextBox { PlaceholderText = "Número", Text = item.Numero };
            _codigoBox = new TextBox { PlaceholderText = "Código", Text = item.Codigo };
            _descricaoBox = new TextBox { PlaceholderText = "Descrição", Text = item.Descricao };
            _quantidadeBox = new TextBox { PlaceholderText = "Quantidade", Text = item.Quantidade };
            _unidadeBox = new TextBox { PlaceholderText = "Unidade", Text = item.Unidade };
            _valorBox = new TextBox { PlaceholderText = "Valor Unitário", Text = item.ValorUnitario.ToString() };

            _resultadoBox = new ComboBox();
            _resultadoBox.Items.Add("Não Definido");
            _resultadoBox.Items.Add("Ganho");
            _resultadoBox.Items.Add("Perdido");

            if (item.Ganho.HasValue)
                _resultadoBox.SelectedIndex = item.Ganho.Value ? 1 : 2;
            else
                _resultadoBox.SelectedIndex = 0;

            stackPanel.Children.Add(new TextBlock { Text = "Número", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_numeroBox);

            stackPanel.Children.Add(new TextBlock { Text = "Código", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_codigoBox);

            stackPanel.Children.Add(new TextBlock { Text = "Descrição", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_descricaoBox);

            stackPanel.Children.Add(new TextBlock { Text = "Quantidade", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_quantidadeBox);

            stackPanel.Children.Add(new TextBlock { Text = "Unidade", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_unidadeBox);

            stackPanel.Children.Add(new TextBlock { Text = "Valor Unitário", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_valorBox);

            stackPanel.Children.Add(new TextBlock { Text = "Resultado", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_resultadoBox);

            this.Content = new ScrollViewer { Content = stackPanel };
        }

        protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            _item.Numero = _numeroBox.Text;
            _item.Codigo = _codigoBox.Text;
            _item.Descricao = _descricaoBox.Text;
            _item.Quantidade = _quantidadeBox.Text;
            _item.Unidade = _unidadeBox.Text;
            _item.ValorUnitario = decimal.TryParse(_valorBox.Text, out var val) ? val : 0;

            switch (_resultadoBox.SelectedIndex)
            {
                case 0: _item.Ganho = null; break;
                case 1: _item.Ganho = true; break;
                case 2: _item.Ganho = false; break;
            }

            base.OnPrimaryButtonClick(args);
        }
    }

    // Add Attachment Dialog
    public sealed class AddAttachmentDialog : ContentDialog
    {
        private Licitacao _licitacao;
        private ComboBox _tipoBox;
        private TextBox _caminhoBox;

        public AddAttachmentDialog(Licitacao licitacao) : base()
        {
            _licitacao = licitacao;
            Title = "Adicionar Anexo";
            PrimaryButtonText = "Adicionar";
            CloseButtonText = "Cancelar";

            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(16) };

            _tipoBox = new ComboBox();
            var tipos = new[] { "Edital", "Proposta Inicial", "Proposta Final", "Resultado", "ATA", "Outros" };
            foreach (var tipo in tipos)
                _tipoBox.Items.Add(tipo);
            _tipoBox.SelectedIndex = 0;

            _caminhoBox = new TextBox { PlaceholderText = "Caminho do arquivo..." };

            var btnBrowse = new Button { Content = "Procurar..." };
            btnBrowse.Click += async (s, e) => 
            {
                // File picker implementation
            };

            stackPanel.Children.Add(new TextBlock { Text = "Tipo", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_tipoBox);

            stackPanel.Children.Add(new TextBlock { Text = "Arquivo", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { _caminhoBox, btnBrowse }
            });

            this.Content = stackPanel;
        }

        protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(_caminhoBox.Text))
            {
                args.Cancel = true;
                return;
            }

            var anexo = new Anexo
            {
                Nome = System.IO.Path.GetFileName(_caminhoBox.Text),
                Caminho = _caminhoBox.Text,
                Tipo = _tipoBox.SelectedItem.ToString(),
                DataAdd = DateTime.Now
            };

            _licitacao.Anexos.Add(anexo);
            base.OnPrimaryButtonClick(args);
        }
    }

    // ATA Dates Dialog
    public sealed class FormAtaDatesDialog : ContentDialog
    {
        private DatePicker _dataInicio;
        private DatePicker _dataFim;

        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }

        public FormAtaDatesDialog() : base()
        {
            Title = "Datas da ATA";
            PrimaryButtonText = "Confirmar";
            CloseButtonText = "Cancelar";

            var stackPanel = new StackPanel { Spacing = 8, Padding = new Thickness(16) };

            _dataInicio = new DatePicker();
            _dataFim = new DatePicker();

            stackPanel.Children.Add(new TextBlock { Text = "Data Início", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_dataInicio);

            stackPanel.Children.Add(new TextBlock { Text = "Data Fim", FontWeight = Windows.UI.Text.FontWeights.Bold });
            stackPanel.Children.Add(_dataFim);

            this.Content = stackPanel;
        }

        protected override void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
        {
            if (!_dataInicio.Date.HasValue || !_dataFim.Date.HasValue)
            {
                args.Cancel = true;
                return;
            }

            DataInicio = _dataInicio.Date.Value.DateTime;
            DataFim = _dataFim.Date.Value.DateTime;

            base.OnPrimaryButtonClick(args);
        }
    }
}
