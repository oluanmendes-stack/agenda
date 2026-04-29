using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace AgendaLicitacoes
{
    public class MainForm : Form
    {
        // ── Palette (Google Calendar light) ─────────────────────────────────
        static readonly Color C_BG    = Color.FromArgb(255,255,255);
        static readonly Color C_MID   = Color.FromArgb(245,245,245);
        static readonly Color C_BRD   = Color.FromArgb(218,220,224);
        static readonly Color C_FG    = Color.FromArgb(60,64,67);
        static readonly Color C_MUT   = Color.FromArgb(112,117,122);
        static readonly Color C_ACC   = Color.FromArgb(26,115,232);
        static readonly Color C_ACC2  = Color.FromArgb(26,115,232);

        // ── State ────────────────────────────────────────────────────────────
        FiltroState      _filtro  = new();
        List<Licitacao>  _res     = new();
        enum ViewMode { Mes, Semana, Ano, Lista }
        ViewMode _view = ViewMode.Mes;
        DateTime _nav  = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // ── Sidebar controls ──────────────────────────────────────────────────
        TextBox   txtBusca=null!, txtItem=null!, txtDI=null!, txtDF=null!;
        ComboBox  cmbEst=null!, cmbMun=null!, cmbAno=null!, cmbSt=null!;
        DateTime? _dataInicio=null, _dataFim=null;
        Label     lblG=null!, lblP=null!, lblS=null!, lblN=null!, lblTot=null!, lblEnc=null!;

        // ── Main area controls ────────────────────────────────────────────────
        Panel       pnlCal=null!;
        DataGridView dgv=null!;
        Label       lblNav=null!;
        Button      btnPrev=null!, btnNext=null!, btnHoje=null!;
        Button      btnMes=null!, btnSem=null!, btnAno=null!, btnLst=null!;

        static readonly string[] ESTADOS_F = new[]{"(Todos)","AC","AL","AP","AM","BA","CE","DF","ES","GO","MA",
            "MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO"};

        public MainForm()
        {
            InitUI();
            DataService.CarregarConfig();
            DataService.CarregarDados();
            RefreshAll();
        }

        // ═════════════════════════════════════════════════════════════════════
        // UI BUILD
        // ═════════════════════════════════════════════════════════════════════
        void InitUI()
        {
            Text="Agenda de Licitações";
            Size=new Size(1320,820);MinimumSize=new Size(1100,640);
            StartPosition=FormStartPosition.CenterScreen;
            BackColor=C_BG;Font=new Font("Segoe UI",9);

            // No WinForms, Fill deve ser adicionado ANTES de Left/Right.
            // O Dock engine processa na ordem inversa de adição (último = mais externo).
            // Então: adiciona Main (Fill) primeiro, depois Sidebar (Left).
            BuildMainArea();
            BuildSidebar();
        }

        // ── Sidebar ──────────────────────────────────────────────────────────
        void BuildSidebar()
        {
            var side=new Panel{Dock=DockStyle.Left,Width=268,BackColor=Color.FromArgb(250,250,250)};
            side.Paint+=(s,e)=>e.Graphics.DrawLine(new Pen(C_BRD),side.Width-1,0,side.Width-1,side.Height);

            var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true};
            var lay=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,
                AutoSizeMode=AutoSizeMode.GrowAndShrink,
                FlowDirection=FlowDirection.TopDown,WrapContents=false,
                BackColor=Color.Transparent,Padding=new Padding(10,10,10,10),Width=266};

            int fw=240;
            void SLbl(string t)=>lay.Controls.Add(new Label{Text=t,AutoSize=true,
                Font=new Font("Segoe UI",7.5f,FontStyle.Bold),ForeColor=C_MUT,
                BackColor=Color.Transparent,Margin=new Padding(0,8,0,2)});
            TextBox SBox(string ph){
                var tb=new TextBox{Width=fw,BackColor=C_BG,ForeColor=C_FG,
                    BorderStyle=BorderStyle.FixedSingle,Font=new Font("Segoe UI",9),PlaceholderText=ph};
                return tb;}
            ComboBox SCombo(){
                var cb=new ComboBox{Width=fw,BackColor=C_BG,ForeColor=C_FG,
                    FlatStyle=FlatStyle.Flat,Font=new Font("Segoe UI",9)};
                return cb;}

            // Busca
            SLbl("🔍  BUSCA");
            txtBusca=SBox("Buscar licitações...");
            txtBusca.TextChanged+=(s,e)=>{_filtro.Busca=txtBusca.Text;RefreshAll();};
            lay.Controls.Add(txtBusca);

            // Sep
            lay.Controls.Add(new Panel{Width=fw,Height=1,BackColor=C_BRD,Margin=new Padding(0,8,0,4)});
            lay.Controls.Add(new Label{Text="FILTROS",AutoSize=true,
                Font=new Font("Segoe UI",7.5f,FontStyle.Bold),ForeColor=C_MUT,BackColor=Color.Transparent});

            SLbl("Estado (UF)");
            cmbEst=SCombo(); cmbEst.DropDownStyle=ComboBoxStyle.DropDownList;
            cmbEst.Items.AddRange(ESTADOS_F);cmbEst.SelectedIndex=0;
            cmbEst.SelectedIndexChanged+=(s,e)=>{
                _filtro.Estado=cmbEst.SelectedIndex==0?"":(string)cmbEst.SelectedItem!;RefreshAll();};
            lay.Controls.Add(cmbEst);

            SLbl("Município");
            cmbMun=SCombo();
            cmbMun.Leave+=(s,e)=>{_filtro.Municipio=cmbMun.Text;RefreshAll();};
            cmbMun.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){_filtro.Municipio=cmbMun.Text;RefreshAll();}};
            lay.Controls.Add(cmbMun);

            SLbl("Ano");
            cmbAno=SCombo();
            cmbAno.Leave+=(s,e)=>{_filtro.Ano=cmbAno.Text;RefreshAll();};
            cmbAno.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){_filtro.Ano=cmbAno.Text;RefreshAll();}};
            lay.Controls.Add(cmbAno);

            SLbl("Status");
            cmbSt=SCombo(); cmbSt.DropDownStyle=ComboBoxStyle.DropDownList;
            cmbSt.DrawMode=DrawMode.OwnerDrawFixed; cmbSt.ItemHeight=22;
            cmbSt.Items.Add("(Todos os status)");
            foreach(StatusLicitacao s in Enum.GetValues<StatusLicitacao>()) cmbSt.Items.Add(StatusInfo.GetNome(s));
            cmbSt.SelectedIndex=0;
            cmbSt.DrawItem+=StatusComboDrawItem;
            cmbSt.SelectedIndexChanged+=(s,e)=>{
                _filtro.Status=cmbSt.SelectedIndex==0?null:(StatusLicitacao)(cmbSt.SelectedIndex-1);RefreshAll();};
            lay.Controls.Add(cmbSt);

            // Date range – picker with arrows, text input and calendar button
            // Returns a Panel; also sets the corresponding _dataInicio/_dataFim state.
            Panel MkDateRow(string placeholder, bool isInicio)
            {
                // Outer container
                var pnl = new Panel
                {
                    Width = fw, Height = 54,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 2)
                };

                // ── Row 1: ‹ [text box dd/MM/yyyy] [📅 btn] › ──────────────
                int arrowW = 24, calW = 28, txtW = fw - arrowW * 2 - calW - 4;

                var btnPrevM = new Button
                {
                    Text = "‹", Width = arrowW, Height = 26, Left = 0, Top = 0,
                    FlatStyle = FlatStyle.Flat, BackColor = C_MID, ForeColor = C_FG,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand,
                    TabStop = false
                };
                btnPrevM.FlatAppearance.BorderColor = C_BRD; btnPrevM.FlatAppearance.BorderSize = 1;

                var txtData = new TextBox
                {
                    Left = arrowW + 1, Top = 2, Width = txtW, Height = 22,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 8.5f),
                    BackColor = C_BG, ForeColor = C_MUT,
                    PlaceholderText = "dd/MM/yyyy",
                    TextAlign = HorizontalAlignment.Center
                };

                var btnCal = new Button
                {
                    Text = "📅", Width = calW, Height = 26,
                    Left = arrowW + 1 + txtW + 1, Top = 0,
                    FlatStyle = FlatStyle.Flat, BackColor = C_MID, ForeColor = C_FG,
                    Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand, TabStop = false
                };
                btnCal.FlatAppearance.BorderColor = C_BRD; btnCal.FlatAppearance.BorderSize = 1;

                var btnNextM = new Button
                {
                    Text = "›", Width = arrowW, Height = 26,
                    Left = arrowW + 1 + txtW + 1 + calW + 1, Top = 0,
                    FlatStyle = FlatStyle.Flat, BackColor = C_MID, ForeColor = C_FG,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand,
                    TabStop = false
                };
                btnNextM.FlatAppearance.BorderColor = C_BRD; btnNextM.FlatAppearance.BorderSize = 1;

                // ── Row 2: hint label ─────────────────────────────────────────
                var lblHint = new Label
                {
                    Left = 0, Top = 30, Width = fw, Height = 16, AutoSize = false,
                    Text = "Setas: mês anterior/próximo  •  Dir: limpar",
                    Font = new Font("Segoe UI", 6.8f), ForeColor = C_MUT,
                    BackColor = Color.Transparent
                };

                pnl.Controls.AddRange(new Control[] { btnPrevM, txtData, btnCal, btnNextM, lblHint });

                // Store reference for external access (clear filters etc.)
                if (isInicio) txtDI = txtData;
                else          txtDF = txtData;

                // ── Helper: apply a chosen date ───────────────────────────────
                void ApplyDate(DateTime d)
                {
                    if (isInicio) { _dataInicio = d; _filtro.DataInicio = d; }
                    else          { _dataFim    = d; _filtro.DataFim    = d; }
                    txtData.Text      = d.ToString("dd/MM/yyyy");
                    txtData.ForeColor = C_ACC;
                    txtData.Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                    RefreshAll();
                }

                void ClearDate()
                {
                    if (isInicio) { _dataInicio = null; _filtro.DataInicio = null; }
                    else          { _dataFim    = null; _filtro.DataFim    = null; }
                    txtData.Text      = "";
                    txtData.ForeColor = C_MUT;
                    txtData.Font      = new Font("Segoe UI", 8.5f);
                    RefreshAll();
                }

                DateTime CurrentBase() => isInicio
                    ? (_dataInicio ?? DateTime.Today)
                    : (_dataFim    ?? DateTime.Today);

                // ‹ prev month
                btnPrevM.Click += (s, e) => ApplyDate(CurrentBase().AddMonths(-1).Date);

                // › next month
                btnNextM.Click += (s, e) => ApplyDate(CurrentBase().AddMonths(1).Date);

                // 📅 calendar popup
                btnCal.Click += (s, e) =>
                {
                    var cal = new MiniCalendario(CurrentBase());
                    cal.StartPosition = FormStartPosition.Manual;
                    var pt = pnl.PointToScreen(new Point(0, 28));
                    cal.Location = pt;
                    if (cal.ShowDialog(this) == DialogResult.OK)
                        ApplyDate(cal.DataSelecionada);
                };

                // Right-click on any button → clear
                btnPrevM.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) ClearDate(); };
                btnNextM.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) ClearDate(); };
                btnCal.MouseDown   += (s, e) => { if (e.Button == MouseButtons.Right) ClearDate(); };

                // Manual text entry: validate on Leave and Enter
                void TryParseText()
                {
                    var t = txtData.Text.Trim();
                    if (string.IsNullOrEmpty(t)) { ClearDate(); return; }
                    if (DateTime.TryParseExact(t, new[]{"dd/MM/yyyy","d/M/yyyy","ddMMyyyy"},
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var dt))
                        ApplyDate(dt);
                    else
                    {
                        txtData.BackColor = Color.FromArgb(255, 235, 235);
                        System.Threading.Tasks.Task.Delay(600).ContinueWith(_ =>
                            txtData.Invoke(() => txtData.BackColor = C_BG));
                    }
                }
                txtData.Leave        += (s, e) => TryParseText();
                txtData.KeyDown      += (s, e) => { if (e.KeyCode == Keys.Enter) { TryParseText(); e.SuppressKeyPress = true; } };

                return pnl;
            }

            SLbl("Data Disputa – Início");
            lay.Controls.Add(MkDateRow("Qualquer data de início", true));

            SLbl("Data Disputa – Fim");
            lay.Controls.Add(MkDateRow("Qualquer data de fim", false));

            SLbl("Filtrar por Item");
            txtItem=SBox("Número, código ou descrição");
            txtItem.TextChanged+=(s,e)=>{_filtro.FiltroItem=txtItem.Text;RefreshAll();};
            lay.Controls.Add(txtItem);

            // Limpar
            lay.Controls.Add(new Panel{Width=fw,Height=6,BackColor=Color.Transparent});
            var bLmp=new Button{Text="✕  Limpar Filtros",Width=fw,Height=28,FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(252,235,235),ForeColor=Color.FromArgb(200,50,50),
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold)};
            bLmp.FlatAppearance.BorderColor=Color.FromArgb(220,150,150);bLmp.Click+=LimparFiltros;
            lay.Controls.Add(bLmp);

            // Sep
            lay.Controls.Add(new Panel{Width=fw,Height=1,BackColor=C_BRD,Margin=new Padding(0,10,0,8)});

            // Stats
            lay.Controls.Add(new Label{Text="MÊS ATUAL",AutoSize=true,
                Font=new Font("Segoe UI",7.5f,FontStyle.Bold),ForeColor=C_MUT,BackColor=Color.Transparent});
            lay.Controls.Add(new Panel{Width=fw,Height=4,BackColor=Color.Transparent});

            void StatRow(string nome,Color cor,out Label lbl){
                var p=new FlowLayoutPanel{Width=fw,Height=20,Margin=new Padding(0,1,0,1),
                    BackColor=Color.Transparent,AutoSize=false};
                p.Controls.Add(new Panel{Width=10,Height=10,BackColor=cor,Margin=new Padding(0,4,4,0)});
                p.Controls.Add(new Label{Text=nome,ForeColor=C_MUT,AutoSize=true,
                    Font=new Font("Segoe UI",8.5f),BackColor=Color.Transparent,Margin=new Padding(0,1,0,0)});
                lbl=new Label{ForeColor=cor,Font=new Font("Segoe UI",8.5f,FontStyle.Bold),
                    AutoSize=true,Text="0",BackColor=Color.Transparent,Margin=new Padding(4,1,0,0)};
                p.Controls.Add(lbl);
                lay.Controls.Add(p);}
            StatRow("Ganho",   StatusInfo.GetCor(StatusLicitacao.Ganho),  out lblG);
            StatRow("Perdido", StatusInfo.GetCor(StatusLicitacao.Perdido), out lblP);
            StatRow("Suspenso",StatusInfo.GetCor(StatusLicitacao.Suspenso),out lblS);
            StatRow("Não Codi.",StatusInfo.GetCor(StatusLicitacao.NaoCodificado),out lblN);
            lay.Controls.Add(new Panel{Width=fw,Height=1,BackColor=C_BRD,Margin=new Padding(0,4,0,4)});
            var pTot=new FlowLayoutPanel{Width=fw,Height=20,BackColor=Color.Transparent,AutoSize=false};
            pTot.Controls.Add(new Label{Text="Total mês:",ForeColor=C_FG,AutoSize=true,
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold),BackColor=Color.Transparent});
            lblTot=new Label{ForeColor=C_ACC2,Font=new Font("Segoe UI",8.5f,FontStyle.Bold),
                AutoSize=true,Text="0",BackColor=Color.Transparent,Margin=new Padding(4,0,0,0)};
            pTot.Controls.Add(lblTot);
            lay.Controls.Add(pTot);

            scroll.Controls.Add(lay);
            side.Controls.Add(scroll);

            // Bottom found count
            var bot=new Panel{Dock=DockStyle.Bottom,Height=28,BackColor=Color.FromArgb(240,240,240)};
            lblEnc=new Label{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,
                Font=new Font("Segoe UI",8),ForeColor=C_MUT,Padding=new Padding(10,0,0,0),
                BackColor=Color.Transparent};
            bot.Controls.Add(lblEnc);
            side.Controls.Add(bot);
            Controls.Add(side);
        }

        // ── Status combobox drawing (dot-style) ──────────────────────────────
        void StatusComboDrawItem(object? sender,DrawItemEventArgs e)
        {
            if(e.Index<0) return;
            bool sel=(e.State&DrawItemState.Selected)!=0;
            using var bg=new SolidBrush(sel?Color.FromArgb(232,240,254):Color.White);
            e.Graphics.FillRectangle(bg,e.Bounds);

            if(e.Index==0){
                using var tb=new SolidBrush(C_MUT);
                e.Graphics.DrawString("(Todos os status)",e.Font??Font,tb,e.Bounds.Left+6,e.Bounds.Top+3);
            } else {
                var st=(StatusLicitacao)(e.Index-1);
                int ds=10,dx=e.Bounds.Left+6,dy=e.Bounds.Top+(e.Bounds.Height-ds)/2;
                e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
                using var dot=new SolidBrush(StatusInfo.GetCor(st));
                e.Graphics.FillEllipse(dot,dx,dy,ds,ds);
                using var tb=new SolidBrush(C_FG);
                e.Graphics.DrawString(StatusInfo.GetNome(st),e.Font??Font,tb,e.Bounds.Left+22,e.Bounds.Top+3);
            }
            if((e.State&DrawItemState.Focus)!=0) e.DrawFocusRectangle();
        }

        // ── Main area ─────────────────────────────────────────────────────────
        void BuildMainArea()
        {
            var main = new Panel { Dock = DockStyle.Fill, BackColor = C_BG };

            // ── TOOLBAR: Panel simples com posicionamento por Anchor ──────────
            // Usamos um Panel normal com altura fixa. Dentro dele, dois FlowLayoutPanels
            // ancorados (esquerda e direita) e um Label central ancorado dos dois lados.
            // Isso é mais robusto que TableLayoutPanel para este cenário.
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.White
            };
            toolbar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(C_BRD), 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);

            // ── GRUPO ESQUERDA: Nova + ‹ btnPrev › btnNext ───────────────────
            var leftPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Location = new Point(8, 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Padding = new Padding(0)
            };

            var bNova = new Button
            {
                Text = "＋ Nova Licitação",
                Width = 148,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = C_ACC,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            bNova.FlatAppearance.BorderSize = 0;
            bNova.Click += NovaClick;
            leftPanel.Controls.Add(bNova);

            btnPrev = new Button
            {
                Text = "‹",
                Width = 34,
                Height = 36,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 2, 0)
            };
            btnPrev.FlatAppearance.BorderColor = C_BRD;
            btnPrev.FlatAppearance.BorderSize = 1;
            btnPrev.Click += PrevClick;
            leftPanel.Controls.Add(btnPrev);

            btnNext = new Button
            {
                Text = "›",
                Width = 34,
                Height = 36,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btnNext.FlatAppearance.BorderColor = C_BRD;
            btnNext.FlatAppearance.BorderSize = 1;
            btnNext.Click += NextClick;
            leftPanel.Controls.Add(btnNext);

            // ── GRUPO DIREITA: Hoje + Mês + Semana + Ano + Lista + Config ─────
            var rightPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Padding = new Padding(0),
                Top = 10
            };

            btnHoje = new Button
            {
                Text = "Hoje",
                Width = 60,
                Height = 36,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnHoje.FlatAppearance.BorderColor = C_BRD;
            btnHoje.FlatAppearance.BorderSize = 1;
            btnHoje.Click += HojeClick;
            rightPanel.Controls.Add(btnHoje);

            btnMes = new Button
            {
                Text = "📅 Mês",
                Width = 82,
                Height = 36,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = C_ACC,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 2, 0)
            };
            btnMes.FlatAppearance.BorderSize = 0;
            btnMes.Click += (s, e) => SwitchView(ViewMode.Mes);
            rightPanel.Controls.Add(btnMes);

            btnSem = new Button
            {
                Text = "📋 Semana",
                Width = 98,
                Height = 36,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 2, 0)
            };
            btnSem.FlatAppearance.BorderColor = C_BRD;
            btnSem.FlatAppearance.BorderSize = 1;
            btnSem.Click += (s, e) => SwitchView(ViewMode.Semana);
            rightPanel.Controls.Add(btnSem);

            btnAno = new Button
            {
                Text = "📊 Ano",
                Width = 82,
                Height = 36,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 2, 0)
            };
            btnAno.FlatAppearance.BorderColor = C_BRD;
            btnAno.FlatAppearance.BorderSize = 1;
            btnAno.Click += (s, e) => SwitchView(ViewMode.Ano);
            rightPanel.Controls.Add(btnAno);

            btnLst = new Button
            {
                Text = "📑 Lista",
                Width = 82,
                Height = 36,
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 243, 244),
                ForeColor = C_FG,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnLst.FlatAppearance.BorderColor = C_BRD;
            btnLst.FlatAppearance.BorderSize = 1;
            btnLst.Click += (s, e) => SwitchView(ViewMode.Lista);
            rightPanel.Controls.Add(btnLst);

            var bConf = new Button
            {
                Text = "⚙ Config",
                Width = 96,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            bConf.FlatAppearance.BorderSize = 0;
            bConf.Click += (s, e) => new FormConfig().ShowDialog(this);
            rightPanel.Controls.Add(bConf);

            // ── CENTRO: label de navegação (ancorado entre os dois grupos) ────
            lblNav = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = C_FG,
                BackColor = Color.Transparent,
                AutoSize = false,
                Height = 36,
                Top = 10,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            // Posiciona rightPanel e lblNav após o form ser carregado
            toolbar.Controls.Add(leftPanel);
            toolbar.Controls.Add(rightPanel);
            toolbar.Controls.Add(lblNav);

            // Calcula posição do rightPanel e lblNav sempre que a toolbar muda de tamanho
            void PositionToolbarControls()
            {
                if (toolbar.Width < 10) return;
                int rightW = rightPanel.PreferredSize.Width + 8;
                rightPanel.Left = toolbar.Width - rightW;
                int leftRight = leftPanel.Left + leftPanel.PreferredSize.Width + 8;
                lblNav.Left = leftRight;
                lblNav.Width = Math.Max(0, rightPanel.Left - leftRight);
            }
            toolbar.Resize += (s, e) => PositionToolbarControls();
            // Dispara o posicionamento inicial após o handle ser criado (toolbar já tem Width real)
            toolbar.HandleCreated += (s, e) => PositionToolbarControls();

            // ── CONTENT AREA ─────────────────────────────────────────────────
            pnlCal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(4),
                AutoScroll = true
            };
            pnlCal.Resize += (s, e) => RedrawCal();

            dgv = BuildGrid();
            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _res.Count)
                    OpenDetalhe(_res[e.RowIndex]);
            };
            dgv.CellFormatting += DgvFormat;
            dgv.Visible = false;

            // Context Menu
            var ctx = new ContextMenuStrip { BackColor = C_MID, ForeColor = C_FG, Renderer = new DarkMenuRenderer() };
            ctx.Items.Add("📂 Abrir Detalhe", null, (s, e) =>
            {
                if (dgv.CurrentRow is { } row && row.Index >= 0 && row.Index < _res.Count)
                    OpenDetalhe(_res[row.Index]);
            });
            ctx.Items.Add("✏ Editar", null, (s, e) =>
            {
                if (dgv.CurrentRow is { } row && row.Index >= 0 && row.Index < _res.Count)
                    EditarLic(_res[row.Index]);
            });
            ctx.Items.Add("📁 Abrir Pasta", null, (s, e) =>
            {
                if (dgv.CurrentRow is { } row && row.Index >= 0 && row.Index < _res.Count)
                    AbrirPasta(_res[row.Index]);
            });
            ctx.Items.Add(new ToolStripSeparator());
            ctx.Items.Add("🗑 Excluir", null, (s, e) =>
            {
                if (dgv.CurrentRow is { } row && row.Index >= 0 && row.Index < _res.Count)
                {
                    var l = _res[row.Index];
                    if (MessageBox.Show($"Excluir:\n{l.GetTitulo()}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        DataService.Remover(l.Id);
                        RefreshAll();
                    }
                }
            });
            dgv.ContextMenuStrip = ctx;

            // Ordem de adição: toolbar primeiro (Top), depois conteúdo (Fill)
            main.Controls.Add(pnlCal);
            main.Controls.Add(dgv);
            main.Controls.Add(toolbar);
            Controls.Add(main);
        }

        void SwitchView(ViewMode m)
        {
            _view = m;
            dgv.Visible = m == ViewMode.Lista;
            pnlCal.Visible = m != ViewMode.Lista;

            btnMes.BackColor = m == ViewMode.Mes ? C_ACC : Color.FromArgb(241, 243, 244);
            btnMes.ForeColor = m == ViewMode.Mes ? Color.White : C_FG;

            btnSem.BackColor = m == ViewMode.Semana ? C_ACC : Color.FromArgb(241, 243, 244);
            btnSem.ForeColor = m == ViewMode.Semana ? Color.White : C_FG;

            btnAno.BackColor = m == ViewMode.Ano ? C_ACC : Color.FromArgb(241, 243, 244);
            btnAno.ForeColor = m == ViewMode.Ano ? Color.White : C_FG;

            btnLst.BackColor = m == ViewMode.Lista ? C_ACC : Color.FromArgb(241, 243, 244);
            btnLst.ForeColor = m == ViewMode.Lista ? Color.White : C_FG;

            UpdateNavLabel();
            if (m != ViewMode.Lista) RedrawCal();
        }

        void UpdateNavLabel()
        {
            var ci = new System.Globalization.CultureInfo("pt-BR");
            lblNav.Text = _view switch
            {
                ViewMode.Mes => char.ToUpper(_nav.ToString("MMMM", ci)[0]) + _nav.ToString("MMMM", ci)[1..] + _nav.ToString(" 'de' yyyy", ci),
                ViewMode.Semana => $"Semana de {WeekStart(_nav):dd/MM} a {WeekStart(_nav).AddDays(6):dd/MM/yyyy}",
                ViewMode.Ano => _nav.Year.ToString(),
                ViewMode.Lista => "Lista de Licitações",
                _ => ""
            };
        }

        static DateTime WeekStart(DateTime d) { int diff = (int)d.DayOfWeek; return d.AddDays(-diff).Date; }

        void PrevClick(object? s, EventArgs e)
        {
            _nav = _view switch
            {
                ViewMode.Mes => _nav.AddMonths(-1),
                ViewMode.Semana => _nav.AddDays(-7),
                ViewMode.Ano => _nav.AddYears(-1),
                _ => _nav
            };
            UpdateNavLabel();
            if (_view != ViewMode.Lista) RedrawCal();
        }

        void NextClick(object? s, EventArgs e)
        {
            _nav = _view switch
            {
                ViewMode.Mes => _nav.AddMonths(1),
                ViewMode.Semana => _nav.AddDays(7),
                ViewMode.Ano => _nav.AddYears(1),
                _ => _nav
            };
            UpdateNavLabel();
            if (_view != ViewMode.Lista) RedrawCal();
        }

        void HojeClick(object? s, EventArgs e)
        {
            _nav = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            UpdateNavLabel();
            if (_view != ViewMode.Lista) RedrawCal();
        }

        // ═════════════════════════════════════════════════════════════════════
        // CALENDAR DRAWING
        // ═════════════════════════════════════════════════════════════════════
        void RedrawCal()
        {
            if (pnlCal == null || !pnlCal.Visible) return;
            pnlCal.SuspendLayout();
            pnlCal.Controls.Clear();
            switch (_view)
            {
                case ViewMode.Mes: DrawMes(); break;
                case ViewMode.Semana: DrawSemana(); break;
                case ViewMode.Ano: DrawAno(); break;
            }
            pnlCal.ResumeLayout(true);
        }

        public class CalEvent {
            public Licitacao Lic;
            public string TextPrefix;
        }

        Dictionary<DateTime, List<CalEvent>> GroupByDay()
        {
            var list = new List<(DateTime Date, CalEvent Ev)>();
            foreach(var l in _res) {
                if(l.DataDisputa.HasValue) 
                    list.Add((l.DataDisputa.Value.Date, new CalEvent{Lic=l, TextPrefix=""}));
                if(l.DataInicioAta.HasValue)
                    list.Add((l.DataInicioAta.Value.Date, new CalEvent{Lic=l, TextPrefix="⏰ Início ATA "}));
                if(l.DataFimAta.HasValue)
                    list.Add((l.DataFimAta.Value.Date, new CalEvent{Lic=l, TextPrefix="⏰ Fim ATA "}));
            }
            return list.GroupBy(x => x.Date)
                       .ToDictionary(g => g.Key, g => g.Select(x => x.Ev).ToList());
        }

        // ── Month view ───────────────────────────────────────────────────────
        void DrawMes()
        {
            var byDay = GroupByDay();
            var today = DateTime.Today;
            var first = new DateTime(_nav.Year, _nav.Month, 1);
            int startDow = (int)first.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_nav.Year, _nav.Month);
            string[] dayNames = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

            int cols = 7, totalW = pnlCal.ClientSize.Width - 8;
            int cellW = totalW / cols;
            int hdrH = 22;
            int gridH = pnlCal.ClientSize.Height - 8 - hdrH;
            int cellH = gridH / 6;
            if (cellH < 80) cellH = 80;

            for (int d = 0; d < 7; d++)
            {
                var lbl = new Label
                {
                    Text = dayNames[d],
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = C_MUT,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Bounds = new Rectangle(d * cellW, 0, cellW, hdrH),
                    BackColor = Color.Transparent
                };
                pnlCal.Controls.Add(lbl);
            }

            for (int cell = 0; cell < 42; cell++)
            {
                int day = cell - startDow + 1;
                int cr = cell / 7, cc = cell % 7;
                bool valid = day >= 1 && day <= daysInMonth;
                var date = valid ? new DateTime(_nav.Year, _nav.Month, day) : DateTime.MinValue;
                bool isToday = valid && date == today;

                var p = new Panel
                {
                    Bounds = new Rectangle(cc * cellW, hdrH + cr * cellH, cellW - 2, cellH - 2),
                    BackColor = isToday ? Color.FromArgb(232, 240, 254) :
                              valid ? Color.White : Color.FromArgb(245, 245, 245),
                    Cursor = valid ? Cursors.Hand : Cursors.Default
                };

                var borderColor = isToday ? C_ACC : C_BRD;
                p.Paint += (s2, e2) =>
                {
                    using var pen = new Pen(borderColor, isToday ? 2 : 1);
                    e2.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                };

                if (valid)
                {
                    var lDay = new Label
                    {
                        Text = day.ToString(),
                        Font = isToday ? new Font("Segoe UI", 9, FontStyle.Bold) : new Font("Segoe UI", 8.5f),
                        ForeColor = isToday ? C_ACC2 : C_FG,
                        Bounds = new Rectangle(4, 2, 24, 18),
                        BackColor = Color.Transparent
                    };
                    p.Controls.Add(lDay);

                    int cap = day;
                    EventHandler cellClick = (s2, e2) =>
                    {
                        var clickDate = new DateTime(_nav.Year, _nav.Month, cap);
                        var dlg = new FormLicitacao(null, clickDate);
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            DataService.Adicionar(dlg.Resultado);
                            RefreshAll();
                        }
                    };
                    p.Click += cellClick;
                    lDay.Click += cellClick;

                    if (byDay.TryGetValue(date, out var evs))
                    {
                        int ey = 22;
                        int maxHeight = cellH - 30;
                        int eventHeight = 17;
                        int spaceBetween = 2;

                        int visibleCount = Math.Min(evs.Count, Math.Max(1, (maxHeight - 4) / (eventHeight + spaceBetween)));

                        for (int ei = 0; ei < visibleCount; ei++)
                        {
                            var ev = evs[ei];
                            var cor = StatusInfo.GetCor(ev.Lic.Status);
                            var ep = new Panel
                            {
                                Bounds = new Rectangle(2, ey, cellW - 8, eventHeight),
                                BackColor = cor,
                                Cursor = Cursors.Hand,
                                Tag = ev.Lic
                            };
                            var evCopy = ev;
                            var txt = $"{evCopy.TextPrefix}{evCopy.Lic.HoraDisputa} {evCopy.Lic.GetSigla()} {evCopy.Lic.Numero}";
                            ep.Paint += (s2, e2) =>
                            {
                                using var tb = new SolidBrush(StatusInfo.GetCorTexto(evCopy.Lic.Status));
                                e2.Graphics.DrawString(txt, new Font("Segoe UI", 6.5f, FontStyle.Bold), tb, 3, 1);
                            };
                            ep.Click += (s2, e2) => { OpenDetalhe(evCopy.Lic); };
                            ep.MouseDown += (s2, e2) => { };
                            new ToolTip().SetToolTip(ep, evCopy.Lic.GetTitulo());
                            p.Controls.Add(ep);
                            ey += eventHeight + spaceBetween;
                        }

                        if (evs.Count > visibleCount)
                        {
                            var lblMore = new Label
                            {
                                Text = $"+{evs.Count - visibleCount} mais",
                                Bounds = new Rectangle(2, ey, cellW - 8, 14),
                                Font = new Font("Segoe UI", 6.5f, FontStyle.Bold),
                                ForeColor = C_ACC,
                                BackColor = Color.Transparent,
                                Cursor = Cursors.Hand
                            };
                            lblMore.Click += (s2, e2) =>
                            {
                                var clickDate2 = new DateTime(_nav.Year, _nav.Month, cap);
                                _dataInicio = clickDate2; _filtro.DataInicio = clickDate2;
                                _dataFim    = clickDate2; _filtro.DataFim    = clickDate2;
                                if(txtDI!=null){txtDI.Text=clickDate2.ToString("dd/MM/yyyy");txtDI.ForeColor=C_ACC;txtDI.Font=new Font("Segoe UI",8.5f,FontStyle.Bold);}
                                if(txtDF!=null){txtDF.Text=clickDate2.ToString("dd/MM/yyyy");txtDF.ForeColor=C_ACC;txtDF.Font=new Font("Segoe UI",8.5f,FontStyle.Bold);}
                                SwitchView(ViewMode.Lista);
                            };
                            p.Controls.Add(lblMore);
                        }
                    }
                }
                pnlCal.Controls.Add(p);
            }
        }

        // ── Week view ────────────────────────────────────────────────────────
        void DrawSemana()
        {
            var byDay = GroupByDay();
            var today = DateTime.Today;
            var ws = WeekStart(_nav);
            string[] dayNames = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };
            int totalW = pnlCal.ClientSize.Width - 8;
            int cellW = totalW / 7;
            int hdrH = 32;

            for (int d = 0; d < 7; d++)
            {
                var date = ws.AddDays(d);
                bool isToday = date == today;
                var col = new Panel
                {
                    Bounds = new Rectangle(d * cellW, 0, cellW - 2, pnlCal.ClientSize.Height - 8),
                    BackColor = isToday ? Color.FromArgb(232, 240, 254) : Color.White
                };
                col.Paint += (s2, e2) =>
                {
                    using var pen = new Pen(C_BRD);
                    e2.Graphics.DrawRectangle(pen, 0, 0, col.Width - 1, col.Height - 1);
                };

                var hdr2 = new Label
                {
                    Text = $"{dayNames[d]}\n{date.Day:00}/{date.Month:00}",
                    Font = isToday ? new Font("Segoe UI", 8, FontStyle.Bold) : new Font("Segoe UI", 8),
                    ForeColor = isToday ? C_ACC2 : C_MUT,
                    Bounds = new Rectangle(0, 0, cellW - 2, hdrH),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                col.Controls.Add(hdr2);

                int ey = hdrH + 4;
                if (byDay.TryGetValue(date, out var evs))
                {
                    foreach (var ev in evs)
                    {
                        var cor = StatusInfo.GetCor(ev.Lic.Status);
                        var ep = new Panel
                        {
                            Bounds = new Rectangle(2, ey, cellW - 8, 40),
                            BackColor = cor,
                            Cursor = Cursors.Hand,
                            Tag = ev.Lic
                        };
                        var evCopy = ev;
                        ep.Paint += (s2, e2) =>
                        {
                            using var tb = new SolidBrush(StatusInfo.GetCorTexto(evCopy.Lic.Status));
                            e2.Graphics.DrawString(
                                $"{evCopy.TextPrefix}{evCopy.Lic.HoraDisputa} {evCopy.Lic.GetSigla()} {evCopy.Lic.Numero}\n{evCopy.Lic.Municipio}",
                                new Font("Segoe UI", 7, FontStyle.Bold), tb, 3, 3);
                        };
                        ep.Click += (s2, e2) => OpenDetalhe(evCopy.Lic);
                        new ToolTip().SetToolTip(ep, evCopy.Lic.GetTitulo());
                        col.Controls.Add(ep);
                        ey += 44;
                    }
                }
                pnlCal.Controls.Add(col);
            }
        }

        // ── Year view ─────────────────────────────────────────────────────────
        void DrawAno()
        {
            var byDay = GroupByDay();
            var today = DateTime.Today;
            int year = _nav.Year;
            int totalW = pnlCal.ClientSize.Width - 8;
            int totalH = pnlCal.ClientSize.Height - 8;
            int mw = totalW / 4, mh = totalH / 3;

            for (int month = 0; month < 12; month++)
            {
                int mr = month / 4, mc = month % 4;
                var mp = new Panel { Bounds = new Rectangle(mc * mw, mr * mh, mw - 4, mh - 4), BackColor = Color.White };
                mp.Paint += (s2, e2) =>
                {
                    using var pen = new Pen(C_BRD);
                    e2.Graphics.DrawRectangle(pen, 0, 0, mp.Width - 1, mp.Height - 1);
                };
                var ci = new System.Globalization.CultureInfo("pt-BR");
                int monthCopy = month + 1;
                var lblMes = new Label
                {
                    Text = new DateTime(year, month + 1, 1).ToString("MMMM", ci),
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = C_ACC2,
                    Bounds = new Rectangle(0, 0, mw - 4, 18),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                // Click on month name → go to that month in month view
                lblMes.Click += (s2, e2) =>
                {
                    _nav = new DateTime(year, monthCopy, 1);
                    SwitchView(ViewMode.Mes);
                };
                mp.Controls.Add(lblMes);

                int days = DateTime.DaysInMonth(year, month + 1);
                int startDow = (int)new DateTime(year, month + 1, 1).DayOfWeek;
                int cw = (mw - 8) / 7, ch = Math.Max(12, (mh - 24) / 6);

                for (int cell = 0; cell < 42; cell++)
                {
                    int day = cell - startDow + 1;
                    if (day < 1 || day > days) continue;
                    int cr = cell / 7, cc = cell % 7;
                    var date = new DateTime(year, month + 1, day);
                    bool isTod = date == today;
                    bool hasEv = byDay.TryGetValue(date, out var dayEvs);
                    var dp = new Panel
                    {
                        Bounds = new Rectangle(4 + cc * cw, 20 + cr * ch, cw - 1, ch - 1),
                        BackColor = isTod ? Color.FromArgb(232, 240, 254) : Color.Transparent,
                        Cursor = Cursors.Hand  // always hand – any day navigates to month
                    };
                    var dayLbl = new Label
                    {
                        Text = day.ToString(),
                        Font = new Font("Segoe UI", 6.5f, hasEv || isTod ? FontStyle.Bold : FontStyle.Regular),
                        ForeColor = isTod ? C_ACC2 : hasEv ? C_FG : C_MUT,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };
                    dp.Controls.Add(dayLbl);

                    var dateCopy = date;
                    if (hasEv && dayEvs!.Count > 0)
                    {
                        var cor = StatusInfo.GetCor(dayEvs[0].Lic.Status);
                        dp.Paint += (s2, e2) =>
                        {
                            using var b = new SolidBrush(cor);
                            e2.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e2.Graphics.FillEllipse(b, dp.Width - 5, 0, 4, 4);
                        };
                    }
                    // Any day click → navigate to that month in month view
                    EventHandler dayClick = (s2, e2) =>
                    {
                        _nav = new DateTime(dateCopy.Year, dateCopy.Month, 1);
                        SwitchView(ViewMode.Mes);
                    };
                    dp.Click += dayClick;
                    dayLbl.Click += dayClick;
                    mp.Controls.Add(dp);
                }
                pnlCal.Controls.Add(mp);
            }
        }

        // ── Grid (list view) ─────────────────────────────────────────────────
        DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = C_BG,
                GridColor = C_BRD,
                Font = new Font("Segoe UI", 9),
                ReadOnly = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                // Impede o scroll horizontal desnecessário garantindo que as colunas preencham
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = C_MID;
            g.ColumnHeadersDefaultCellStyle.ForeColor = C_MUT;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            g.DefaultCellStyle.BackColor = C_BG;
            g.DefaultCellStyle.ForeColor = C_FG;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 254);
            g.DefaultCellStyle.SelectionForeColor = C_FG;
            g.EnableHeadersVisualStyles = false;

            g.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Status",       Width = 120, Name = "colSt", ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Tipo",         Width = 42,  ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Número",       Width = 80,  ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Município/UF", Width = 140, ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Disputa",      Width = 80,  ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Portal",       Width = 90,  ReadOnly = true },
                new DataGridViewTextBoxColumn { HeaderText = "Órgão",        Width = 160, ReadOnly = true },
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Objeto / Produtos",
                    MinimumWidth = 160,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.False }
                }
            );
            return g;
        }

        void DgvFormat(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _res.Count) return;
            var l = _res[e.RowIndex];
            var row = dgv.Rows[e.RowIndex];
            var cor = StatusInfo.GetCor(l.Status);
            row.DefaultCellStyle.BackColor = Color.FromArgb(240,
                Math.Min(255, cor.R + 180),
                Math.Min(255, cor.G + 180),
                Math.Min(255, cor.B + 180));
            if (dgv.Columns[e.ColumnIndex].Name is "colSt")
            {
                e.CellStyle.BackColor = cor;
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // DATA OPS
        // ═════════════════════════════════════════════════════════════════════
        void RefreshAll()
        {
            try
            {
                if (dgv == null || lblEnc == null) return;

                _res = DataService.Filtrar(_filtro);

                bool hasFilter = !string.IsNullOrEmpty(_filtro.Busca) ||
                                !string.IsNullOrEmpty(_filtro.Estado) ||
                                !string.IsNullOrEmpty(_filtro.Municipio) ||
                                !string.IsNullOrEmpty(_filtro.Ano) ||
                                !string.IsNullOrEmpty(_filtro.FiltroItem) ||
                                _filtro.Status.HasValue ||
                                _filtro.DataInicio.HasValue ||
                                _filtro.DataFim.HasValue;

                if (hasFilter)
                {
                    _view = ViewMode.Lista;
                    dgv.Visible = true;
                    pnlCal.Visible = false;
                    btnLst.BackColor = C_ACC;
                    btnLst.ForeColor = Color.White;
                    btnMes.BackColor = Color.FromArgb(241, 243, 244);
                    btnMes.ForeColor = C_FG;
                    btnSem.BackColor = Color.FromArgb(241, 243, 244);
                    btnSem.ForeColor = C_FG;
                    btnAno.BackColor = Color.FromArgb(241, 243, 244);
                    btnAno.ForeColor = C_FG;
                }
                else if (_view == ViewMode.Lista)
                {
                    _view = ViewMode.Mes;
                    dgv.Visible = false;
                    pnlCal.Visible = true;
                    btnMes.BackColor = C_ACC;
                    btnMes.ForeColor = Color.White;
                    btnLst.BackColor = Color.FromArgb(241, 243, 244);
                    btnLst.ForeColor = C_FG;
                }

                RefreshGrid();
                RefreshStats();
                RefreshCombos();

                UpdateNavLabel();
                lblEnc.Text = $"  {_res.Count} licitação(ões) encontrada(s)";

                if (_view != ViewMode.Lista)
                    RedrawCal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void RefreshGrid()
        {
            try
            {
                dgv.SuspendLayout();
                dgv.Rows.Clear();

                if (_res != null && _res.Count > 0)
                {
                    foreach (var l in _res)
                    {
                        string produtos = string.IsNullOrEmpty(l.Produtos) ? "" :
                            (l.Produtos.Length > 65 ? l.Produtos.Substring(0, 65) + "…" : l.Produtos);

                        string disputa = l.DataDisputa.HasValue ? l.DataDisputa.Value.ToString("dd/MM/yyyy") : "";

                        dgv.Rows.Add(
                            StatusInfo.GetNome(l.Status),
                            l.GetSigla(),
                            l.Numero,
                            $"{l.Municipio}/{l.Estado}",
                            disputa,
                            l.Portal,
                            l.Orgao ?? "",
                            produtos
                        );
                    }
                }

                dgv.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar grid: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void RefreshStats()
        {
            var(g,p,s,n,t)=DataService.EstatisticasMes(_res);
            lblG.Text=g.ToString();lblP.Text=p.ToString();
            lblS.Text=s.ToString();lblN.Text=n.ToString();lblTot.Text=t.ToString();
        }

        void RefreshCombos()
        {
            var muns=DataService.Licitacoes.Select(l=>l.Municipio).Distinct().OrderBy(x=>x).ToArray();
            var anos=DataService.Licitacoes.Select(l=>l.Ano).Distinct().OrderByDescending(x=>x).ToArray();
            var cm=cmbMun.Text;var ca=cmbAno.Text;
            cmbMun.Items.Clear();cmbMun.Items.AddRange(muns);cmbMun.Text=cm;
            cmbAno.Items.Clear();cmbAno.Items.AddRange(anos);cmbAno.Text=ca;
        }

        void OpenDetalhe(Licitacao l){var d=new FormDetalhe(l);d.ShowDialog(this);if(d.Modificado)RefreshAll();}

        void EditarLic(Licitacao l){
            var d=new FormLicitacao(l);
            if(d.ShowDialog(this)!=DialogResult.OK) return;
            var ed=d.Resultado;
            l.Ano=ed.Ano;l.Estado=ed.Estado;l.Municipio=ed.Municipio;l.Orgao=ed.Orgao;
            l.Tipo=ed.Tipo;l.Numero=ed.Numero;l.Portal=ed.Portal;l.Status=ed.Status;
            l.DataDisputa=ed.DataDisputa;l.HoraDisputa=ed.HoraDisputa;l.Produtos=ed.Produtos;
            l.ValorEstimado=ed.ValorEstimado;l.CodigoEffecti=ed.CodigoEffecti;l.UASG=ed.UASG;l.CodigoBB=ed.CodigoBB;
            l.AddHistorico("Licitação editada");DataService.Atualizar(l);RefreshAll();}

        void AbrirPasta(Licitacao l){
            var path=!string.IsNullOrEmpty(l.PastaCliente)?l.PastaCliente:l.PastaServidor;
            if(!string.IsNullOrEmpty(path)&&System.IO.Directory.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe",path);
            else MessageBox.Show("Pasta não encontrada:\n"+path,"Aviso",MessageBoxButtons.OK,MessageBoxIcon.Warning);}

        void NovaClick(object? s,EventArgs e){
            var d=new FormLicitacao(null,null);
            if(d.ShowDialog(this)==DialogResult.OK){DataService.Adicionar(d.Resultado);RefreshAll();}}

        void LimparFiltros(object? s,EventArgs e){
            _filtro=new FiltroState();
            _dataInicio=null;_dataFim=null;
            txtBusca.Clear();txtItem.Clear();
            cmbEst.SelectedIndex=0;cmbSt.SelectedIndex=0;
            cmbMun.Text="";cmbAno.Text="";
            if(txtDI!=null){txtDI.Text="";txtDI.ForeColor=C_MUT;txtDI.Font=new Font("Segoe UI",8.5f);}
            if(txtDF!=null){txtDF.Text="";txtDF.ForeColor=C_MUT;txtDF.Font=new Font("Segoe UI",8.5f);}
            _nav=new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            _view=ViewMode.Mes;
            RefreshAll();
            SwitchView(ViewMode.Mes);}

        protected override void OnResize(EventArgs e){base.OnResize(e);if(_view!=ViewMode.Lista)RedrawCal();}
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SwitchView(ViewMode.Mes);
            // Garante posicionamento inicial dos controles da toolbar
            if (btnPrev?.Parent != null)
            {
                var tb = btnPrev.Parent.Parent as Panel;
                tb?.PerformLayout();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)  { PrevClick(null, EventArgs.Empty); return true; }
            if (keyData == Keys.Right) { NextClick(null, EventArgs.Empty); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    // ── Dark context menu renderer ────────────────────────────────────────────
    class DarkMenuRenderer:ToolStripProfessionalRenderer
    {
        static Color Bg =Color.FromArgb(255,255,255);
        static Color Sel=Color.FromArgb(232,240,254);
        static Color Brd=Color.FromArgb(218,220,224);
        public DarkMenuRenderer():base(new DarkColorTable()){}
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e){
            var r=e.Item.ContentRectangle;
            r.Inflate(2,0);
            using var b=new SolidBrush(e.Item.Selected?Sel:Bg);
            e.Graphics.FillRectangle(b,r);}
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){
            e.TextColor=Color.FromArgb(60,64,67);base.OnRenderItemText(e);}
    }
    class DarkColorTable:ProfessionalColorTable{
        static Color Bg=Color.FromArgb(255,255,255);
        static Color Brd=Color.FromArgb(218,220,224);
        public override Color MenuBorder=>Brd;
        public override Color MenuItemBorder=>Brd;
        public override Color MenuItemSelected=>Color.FromArgb(232,240,254);
        public override Color MenuItemSelectedGradientBegin=>Color.FromArgb(232,240,254);
        public override Color MenuItemSelectedGradientEnd=>Color.FromArgb(232,240,254);
        public override Color ToolStripDropDownBackground=>Bg;
        public override Color ImageMarginGradientBegin=>Bg;
        public override Color ImageMarginGradientMiddle=>Bg;
        public override Color ImageMarginGradientEnd=>Bg;
    }

    // ── Mini calendar popup ───────────────────────────────────────────────────
    class MiniCalendario : Form
    {
        public DateTime DataSelecionada { get; private set; } = DateTime.Today;

        DateTime _nav;
        Panel pnlDias = null!;
        Label lblMes  = null!;

        static Color C_BG  = Color.FromArgb(255,255,255);
        static Color C_MID = Color.FromArgb(245,245,245);
        static Color C_BRD = Color.FromArgb(218,220,224);
        static Color C_FG  = Color.FromArgb(60,64,67);
        static Color C_MUT = Color.FromArgb(112,117,122);
        static Color C_ACC = Color.FromArgb(26,115,232);

        public MiniCalendario(DateTime initial)
        {
            _nav = new DateTime(initial.Year, initial.Month, 1);
            DataSelecionada = initial.Date;

            FormBorderStyle = FormBorderStyle.None;
            BackColor       = C_BG;
            Size            = new Size(264, 268);
            StartPosition   = FormStartPosition.Manual;
            Font            = new Font("Segoe UI", 8.5f);
            ShowInTaskbar   = false;

            // Drop shadow effect via paint border
            Paint += (s, e) =>
            {
                using var pen = new Pen(C_BRD, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };

            // ── Header: prev / month+year / next ─────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = C_ACC };

            var btnP = new Button { Text = "‹", Width = 30, Height = 30, Left = 4, Top = 3,
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnP.FlatAppearance.BorderSize = 0;
            btnP.Click += (s, e) => { _nav = _nav.AddMonths(-1); Redraw(); };

            var btnN = new Button { Text = "›", Width = 30, Height = 30, Left = 230, Top = 3,
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnN.FlatAppearance.BorderSize = 0;
            btnN.Click += (s, e) => { _nav = _nav.AddMonths(1); Redraw(); };

            lblMes = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = Color.Transparent };

            hdr.Controls.AddRange(new Control[] { lblMes, btnP, btnN });

            // ── Day-of-week headers ───────────────────────────────────────────
            var dowHdr = new Panel { Dock = DockStyle.Top, Height = 22, BackColor = C_MID };
            string[] days = { "D", "S", "T", "Q", "Q", "S", "S" };
            int dw = 264 / 7;
            for (int i = 0; i < 7; i++)
                dowHdr.Controls.Add(new Label { Text = days[i], Width = dw, Height = 22, Left = i * dw,
                    TextAlign = ContentAlignment.MiddleCenter, ForeColor = C_MUT,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), BackColor = Color.Transparent });

            // ── Days grid ────────────────────────────────────────────────────
            pnlDias = new Panel { Dock = DockStyle.Fill, BackColor = C_BG };

            // ── Footer: Limpar + Hoje ─────────────────────────────────────────
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = C_MID };
            foot.Paint += (s, e) => e.Graphics.DrawLine(new Pen(C_BRD), 0, 0, foot.Width, 0);

            var btnLimpar = new Button { Text = "Limpar", Width = 80, Height = 24, Left = 4, Top = 3,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(252,235,235),
                ForeColor = Color.FromArgb(200,50,50), Font = new Font("Segoe UI",8f,FontStyle.Bold),
                Cursor = Cursors.Hand };
            btnLimpar.FlatAppearance.BorderColor = Color.FromArgb(220,150,150);
            btnLimpar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var btnHoje = new Button { Text = "Hoje", Width = 70, Height = 24, Left = 88, Top = 3,
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(232,240,254),
                ForeColor = C_ACC, Font = new Font("Segoe UI",8f,FontStyle.Bold),
                Cursor = Cursors.Hand };
            btnHoje.FlatAppearance.BorderColor = Color.FromArgb(180,210,255);
            btnHoje.Click += (s, e) =>
            {
                _nav = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DataSelecionada = DateTime.Today;
                DialogResult = DialogResult.OK;
                Close();
            };

            foot.Controls.AddRange(new Control[] { btnLimpar, btnHoje });

            Controls.AddRange(new Control[] { pnlDias, foot, dowHdr, hdr });

            // Close when clicking outside
            Deactivate += (s, e) => Close();

            Redraw();
        }

        void Redraw()
        {
            var ci = new System.Globalization.CultureInfo("pt-BR");
            string mesNome = _nav.ToString("MMMM yyyy", ci);
            lblMes.Text = char.ToUpper(mesNome[0]) + mesNome[1..];

            pnlDias.Controls.Clear();

            var today = DateTime.Today;
            int days  = DateTime.DaysInMonth(_nav.Year, _nav.Month);
            int startDow = (int)new DateTime(_nav.Year, _nav.Month, 1).DayOfWeek;
            int dw = pnlDias.Width > 0 ? pnlDias.Width / 7 : 37;
            int dh = 34;

            for (int cell = 0; cell < 42; cell++)
            {
                int day = cell - startDow + 1;
                if (day < 1 || day > days) continue;
                int cr = cell / 7, cc = cell % 7;
                var date = new DateTime(_nav.Year, _nav.Month, day);
                bool isToday    = date == today;
                bool isSelected = date == DataSelecionada;

                var btn = new Button
                {
                    Text      = day.ToString(),
                    Bounds    = new Rectangle(cc * dw + 2, cr * dh + 2, dw - 4, dh - 4),
                    FlatStyle = FlatStyle.Flat,
                    Cursor    = Cursors.Hand,
                    Font      = new Font("Segoe UI", 8.5f, isSelected || isToday ? FontStyle.Bold : FontStyle.Regular),
                    BackColor = isSelected ? C_ACC
                              : isToday   ? Color.FromArgb(232,240,254)
                              : C_BG,
                    ForeColor = isSelected ? Color.White
                              : isToday   ? C_ACC
                              : C_FG,
                };
                btn.FlatAppearance.BorderSize = isSelected || isToday ? 0 : 0;
                btn.FlatAppearance.BorderColor = C_BRD;
                btn.FlatAppearance.MouseOverBackColor = isSelected ? C_ACC : Color.FromArgb(240,245,255);

                var dateCopy = date;
                btn.Click += (s, e) =>
                {
                    DataSelecionada = dateCopy;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                pnlDias.Controls.Add(btn);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Ensure we stay on screen
            var screen = Screen.FromPoint(Location).WorkingArea;
            if (Right  > screen.Right)  Left = screen.Right  - Width;
            if (Bottom > screen.Bottom) Top  = Bottom > screen.Bottom ? Top - Height - 36 : Top;
            if (Left   < screen.Left)   Left = screen.Left;
            if (Top    < screen.Top)    Top  = screen.Top;
        }
    }
}