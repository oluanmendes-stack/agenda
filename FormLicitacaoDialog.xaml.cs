using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgendaLicitacoes
{
    public sealed partial class FormLicitacaoDialog : ContentDialog
    {
        private Licitacao _licitacao;
        private DataService _dataService;
        private bool _isEdit;

        public FormLicitacaoDialog(Licitacao source, DataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            
            if (source != null)
            {
                _licitacao = JsonConvert.DeserializeObject<Licitacao>(
                    JsonConvert.SerializeObject(source));
                _isEdit = true;
                Title = "Editar Licitação";
            }
            else
            {
                _licitacao = new Licitacao { Ano = DateTime.Now.Year.ToString() };
                _isEdit = false;
                Title = "Nova Licitação";
            }

            InitializeDropdowns();
            LoadData();
        }

        private void InitializeDropdowns()
        {
            // Years
            var currentYear = DateTime.Now.Year;
            for (int i = currentYear - 5; i <= currentYear + 2; i++)
            {
                CmbAno.Items.Add(i.ToString());
            }

            // States
            var states = new[] { "SP", "RJ", "MG", "BA", "CE", "SC", "RS", "DF", "GO", "MT", "PR", "ES", "PE", "PA", "PB", "MA", "AL", "RN", "PI", "AM", "RO", "AC", "AP", "TO", "MS" };
            foreach (var state in states)
            {
                CmbEstado.Items.Add(state);
            }

            // Types
            foreach (var type in Enum.GetNames(typeof(TipoLicitacao)))
            {
                CmbTipo.Items.Add(type);
            }

            // Status
            foreach (var status in Enum.GetNames(typeof(StatusLicitacao)))
            {
                CmbStatus.Items.Add(status);
            }
        }

        private void LoadData()
        {
            TxtAno.Text = _licitacao.Ano;
            CmbEstado.SelectedItem = _licitacao.Estado;
            CmbTipo.SelectedItem = _licitacao.Tipo.ToString();
            TxtNumero.Text = _licitacao.Numero;
            TxtMunicipio.Text = _licitacao.Municipio;
            TxtPortal.Text = _licitacao.Portal;
            TxtOrgao.Text = _licitacao.Orgao;
            TxtCodigoBB.Text = _licitacao.CodigoBB;
            DtDisputa.Date = _licitacao.DataDisputa;
            TimeDisputa.Time = _licitacao.DataDisputa.TimeOfDay;
            CmbStatus.SelectedItem = _licitacao.Status.ToString();
            TxtValor.Text = _licitacao.ValorEstimado.ToString("0.00");
            TxtCodigoEffecti.Text = _licitacao.CodigoEffecti;
            TxtUASG.Text = _licitacao.UASG;
            TxtProdutos.Text = _licitacao.Produtos;

            UpdateCodigoBBVisibility();
        }

        private void UpdateCodigoBBVisibility()
        {
            var portal = TxtPortal.Text ?? "";
            CodigoBBPanel.Visibility = portal.Contains("LICITACOES-E") || portal.Contains("LICITAÇÕES-E") 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void TxtPortal_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCodigoBBVisibility();
        }

        private void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                ValidateAndSave();
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                ShowError(ex.Message);
            }
        }

        private void ValidateAndSave()
        {
            if (string.IsNullOrWhiteSpace(TxtAno.Text)) throw new Exception("Ano é obrigatório");
            if (CmbEstado.SelectedItem == null) throw new Exception("Estado é obrigatório");
            if (CmbTipo.SelectedItem == null) throw new Exception("Tipo é obrigatório");
            if (string.IsNullOrWhiteSpace(TxtNumero.Text)) throw new Exception("Número é obrigatório");
            if (string.IsNullOrWhiteSpace(TxtMunicipio.Text)) throw new Exception("Município é obrigatório");
            if (string.IsNullOrWhiteSpace(TxtPortal.Text)) throw new Exception("Portal é obrigatório");

            _licitacao.Ano = TxtAno.Text;
            _licitacao.Estado = CmbEstado.SelectedItem.ToString();
            _licitacao.Tipo = (TipoLicitacao)Enum.Parse(typeof(TipoLicitacao), CmbTipo.SelectedItem.ToString());
            _licitacao.Numero = TxtNumero.Text;
            _licitacao.Municipio = TxtMunicipio.Text;
            _licitacao.Portal = TxtPortal.Text;
            _licitacao.Orgao = TxtOrgao.Text;
            _licitacao.CodigoBB = TxtCodigoBB.Text;
            
            var date = DtDisputa.Date?.DateTime ?? DateTime.Now;
            var time = TimeDisputa.Time;
            _licitacao.DataDisputa = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
            
            if (CmbStatus.SelectedItem != null)
                _licitacao.Status = (StatusLicitacao)Enum.Parse(typeof(StatusLicitacao), CmbStatus.SelectedItem.ToString());
            
            if (decimal.TryParse(TxtValor.Text, out var valor))
                _licitacao.ValorEstimado = valor;
            
            _licitacao.CodigoEffecti = TxtCodigoEffecti.Text;
            _licitacao.UASG = TxtUASG.Text;
            _licitacao.Produtos = TxtProdutos.Text;

            if (_isEdit)
            {
                _dataService.Atualizar(_licitacao);
            }
            else
            {
                _dataService.Adicionar(_licitacao);
            }
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
