using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AgendaLicitacoes
{
    public class FormLicitacao : Form
    {
        readonly Licitacao _lic;
        readonly bool _isEdit;

        TextBox   txtAno=null!,txtMunicipio=null!,txtNumero=null!,txtPortal=null!,
                  txtOrgao=null!,txtValor=null!,txtEffecti=null!,txtUASG=null!,txtHora=null!,txtCodigoBB=null!,
                  txtProdutos=null!;
        ComboBox  cmbEstado=null!,cmbTipo=null!,cmbStatus=null!;
        DateTimePicker dtpDisputa=null!;
        CheckBox  chkData=null!;
        Panel     pnlCodigoBB=null!;

        static readonly string[] ESTADOS={
            "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA",
            "MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN",
            "RS","RO","RR","SC","SP","SE","TO"};

        public Licitacao Resultado => _lic;

        public FormLicitacao(Licitacao? src=null, DateTime? initialDate=null)
        {
            _isEdit = src!=null;
            _lic = src!=null
                ? Newtonsoft.Json.JsonConvert.DeserializeObject<Licitacao>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(src))!
                : new Licitacao{Ano=DateTime.Now.Year.ToString()};
            if(!_isEdit && initialDate.HasValue){
                _lic.DataDisputa=initialDate.Value;
            }
            Build();
            LoadData();
        }

        // ── UI construction ──────────────────────────────────────────────────
        void Build()
        {
            Text = _isEdit ? "Editar Licitação" : "Nova Licitação";
            ClientSize    = new Size(860, 490);
            MinimumSize   = new Size(780, 440);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = Color.FromArgb(255,255,255);
            Font          = new Font("Segoe UI",9);
            FormBorderStyle = FormBorderStyle.Sizable;

            // ── Header ───────────────────────────────────────────────────────
            var hdr = new Panel{Dock=DockStyle.Top,Height=50,BackColor=Color.FromArgb(245,245,245)};
            hdr.Controls.Add(new Label{
                Text       = _isEdit ? "✏  Editar Licitação" : "＋  Nova Licitação",
                ForeColor  = Color.FromArgb(60,64,67),
                Font       = new Font("Segoe UI",12,FontStyle.Bold),
                Dock       = DockStyle.Fill,
                TextAlign  = ContentAlignment.MiddleLeft,
                Padding    = new Padding(16,0,0,0)});

            // ── Scroll area ──────────────────────────────────────────────────
            var scroll = new Panel{Dock=DockStyle.Fill,AutoScroll=true,
                BackColor=Color.White,Padding=new Padding(16,10,16,10)};

            var tbl = new TableLayoutPanel{
                ColumnCount=4,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,
                Dock=DockStyle.Top,BackColor=Color.Transparent,Padding=new Padding(0)};
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));

            int r=0;

            // Section header
            Sec(tbl,"Identificação",r++,4);

            // Row: Ano | Estado | Tipo | Número
            Lbl(tbl,"Ano *",r,0);         txtAno      =Txt(tbl,r,1);
            Lbl(tbl,"Estado (UF) *",r,2); cmbEstado   =Cmb(tbl,r,3,ESTADOS);
            r++;

            Lbl(tbl,"Tipo de Licitação *",r,0);
            cmbTipo=Cmb(tbl,r,1);
            foreach(TipoLicitacao t in Enum.GetValues<TipoLicitacao>()) cmbTipo.Items.Add(TipoInfo.GetDisplay(t));
            cmbTipo.DropDownStyle=ComboBoxStyle.DropDownList;
            Lbl(tbl,"Número *",r,2); txtNumero=Txt(tbl,r,3);
            r++;

            Lbl(tbl,"Município",r,0); txtMunicipio=Txt(tbl,r,1);
            Lbl(tbl,"Portal *",r,2);  txtPortal   =Txt(tbl,r,3);
            r++;

            // Portal change triggers CodigoBB visibility
            txtPortal.TextChanged += PortalTextChanged;

            Lbl(tbl,"Órgão *",r,0); txtOrgao=Txt(tbl,r,1,3);
            r++;

            // CodigoBB row – hidden by default, shown when portal = LICITACOES-E
            pnlCodigoBB = new Panel{Dock=DockStyle.Fill,Height=30,BackColor=Color.Transparent,Visible=false,Margin=new Padding(0)};
            var lblBB = new Label{Text="Código BB *",Font=new Font("Segoe UI",8,FontStyle.Bold),
                ForeColor=Color.FromArgb(112,117,122),AutoSize=false,Width=140,Dock=DockStyle.Left,
                TextAlign=ContentAlignment.MiddleLeft,BackColor=Color.Transparent,Margin=new Padding(2,6,2,0)};
            txtCodigoBB = new TextBox{BorderStyle=BorderStyle.FixedSingle,
                Font=new Font("Segoe UI",9),BackColor=Color.FromArgb(250,250,250),ForeColor=Color.FromArgb(60,64,67),
                Dock=DockStyle.Fill,Margin=new Padding(2)};
            pnlCodigoBB.Controls.AddRange(new Control[]{txtCodigoBB,lblBB});
            tbl.Controls.Add(pnlCodigoBB);
            tbl.SetRow(pnlCodigoBB,r);
            tbl.SetColumn(pnlCodigoBB,0);
            tbl.SetColumnSpan(pnlCodigoBB,4);
            r++;

            Sec(tbl,"Disputa & Status",r++,4);

            // Data disputa
            Lbl(tbl,"Data de Disputa",r,0);
            var pData=new Panel{Margin=new Padding(2),Height=28,BackColor=Color.Transparent};
            chkData=new CheckBox{Text="Definida",Left=0,Top=5,Width=75,
                ForeColor=Color.FromArgb(112,117,122),BackColor=Color.Transparent};
            dtpDisputa=new DateTimePicker{Format=DateTimePickerFormat.Short,Left=78,Top=2,Width=110,Enabled=false};
            StyleDtp(dtpDisputa);
            chkData.CheckedChanged+=(s,e)=>dtpDisputa.Enabled=chkData.Checked;
            pData.Controls.AddRange(new Control[]{chkData,dtpDisputa});
            tbl.Controls.Add(pData); tbl.SetRow(pData,r); tbl.SetColumn(pData,1);
            Lbl(tbl,"Hora (HH:MM)",r,2); txtHora=Txt(tbl,r,3);
            r++;

            Lbl(tbl,"Status",r,0);
            cmbStatus=new ComboBox{
                Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,
                DrawMode=DrawMode.OwnerDrawFixed,ItemHeight=22,
                FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(250,250,250),
                ForeColor=Color.FromArgb(60,64,67),
                Font=new Font("Segoe UI",9),
                Margin=new Padding(2)};
            cmbStatus.DrawItem   += StatusDrawItem;
            cmbStatus.MeasureItem+= (s,e)=>{e.ItemHeight=22;};
            foreach(StatusLicitacao s in Enum.GetValues<StatusLicitacao>())
                cmbStatus.Items.Add(StatusInfo.GetNome(s));
            tbl.Controls.Add(cmbStatus); tbl.SetRow(cmbStatus,r); tbl.SetColumn(cmbStatus,1);

            Lbl(tbl,"Valor Estimado",r,2); txtValor=Txt(tbl,r,3);
            r++;

            Sec(tbl,"Códigos",r++,4);
            Lbl(tbl,"Código Effecti",r,0); txtEffecti=Txt(tbl,r,1);
            Lbl(tbl,"UASG",r,2);           txtUASG   =Txt(tbl,r,3);
            r++;

            Sec(tbl,"Objeto / Produtos",r++,4);
            Lbl(tbl,"Objeto / Produtos *",r,0);
            txtProdutos=new TextBox{
                Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                Font=new Font("Segoe UI",9),BackColor=BG,ForeColor=FG,
                Multiline=true,Height=60,ScrollBars=ScrollBars.Vertical,
                Margin=new Padding(2)};
            tbl.Controls.Add(txtProdutos);
            tbl.SetRow(txtProdutos,r);
            tbl.SetColumn(txtProdutos,1);
            tbl.SetColumnSpan(txtProdutos,3);
            r++;

            scroll.Controls.Add(tbl);

            // ── Footer ───────────────────────────────────────────────────────
            var footer=new Panel{Dock=DockStyle.Bottom,Height=52,
                BackColor=Color.FromArgb(245,245,245),Padding=new Padding(12,8,12,8)};
            var sep=new Panel{Dock=DockStyle.Top,Height=1,BackColor=Color.FromArgb(218,220,224)};

            var btnOk=new Button{
                Text="💾  Salvar",Width=130,Height=34,Dock=DockStyle.Right,
                FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(59,130,246),ForeColor=Color.White,
                Font=new Font("Segoe UI",9,FontStyle.Bold)};
            btnOk.FlatAppearance.BorderSize=0;
            btnOk.Click+=Salvar;

            var btnNo=new Button{
                Text="Cancelar",Width=100,Height=34,Dock=DockStyle.Right,
                FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(241,243,244),ForeColor=Color.FromArgb(112,117,122)};
            btnNo.FlatAppearance.BorderColor=Color.FromArgb(218,220,224);
            btnNo.Click+=(s,e)=>{DialogResult=DialogResult.Cancel;Close();};

            footer.Controls.AddRange(new Control[]{btnOk,btnNo,sep});
            Controls.AddRange(new Control[]{hdr,scroll,footer});
            AcceptButton=btnOk; CancelButton=btnNo;
        }

        void PortalTextChanged(object? s, EventArgs e)
        {
            bool isLicitacoesE = txtPortal.Text.Trim().Equals("LICITACOES-E", StringComparison.OrdinalIgnoreCase)
                              || txtPortal.Text.Trim().Equals("LICITAÇÕES-E", StringComparison.OrdinalIgnoreCase)
                              || txtPortal.Text.Trim().ToUpper().Contains("LICITACOES-E")
                              || txtPortal.Text.Trim().ToUpper().Contains("LICITAÇÕES-E");
            pnlCodigoBB.Visible = isLicitacoesE;
        }

        // ── Status dropdown drawn like image 3: dark bg + colored dot + text ─
        void StatusDrawItem(object? sender,DrawItemEventArgs e)
        {
            if (e.Index<0) return;
            var st=(StatusLicitacao)e.Index;
            var cor=StatusInfo.GetCor(st);
            var nome=StatusInfo.GetNome(st);
            bool selected=(e.State&DrawItemState.Selected)!=0;

            using var bgBrush=new SolidBrush(selected?Color.FromArgb(232,240,254):Color.White);
            e.Graphics.FillRectangle(bgBrush,e.Bounds);

            int dotSize=10,dotX=e.Bounds.Left+6,dotY=e.Bounds.Top+(e.Bounds.Height-dotSize)/2;
            using var dotBrush=new SolidBrush(cor);
            e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(dotBrush,dotX,dotY,dotSize,dotSize);

            using var textBrush=new SolidBrush(Color.FromArgb(60,64,67));
            e.Graphics.DrawString(nome,e.Font??Font,textBrush,e.Bounds.Left+22,e.Bounds.Top+3);

            if ((e.State&DrawItemState.Focus)!=0) e.DrawFocusRectangle();
        }

        void LoadData()
        {
            txtAno.Text=_lic.Ano;
            var ei=Array.IndexOf(ESTADOS,_lic.Estado);
            cmbEstado.SelectedIndex=ei>=0?ei:0;
            cmbTipo.SelectedIndex=(int)_lic.Tipo;
            txtNumero.Text=_lic.Numero;
            txtMunicipio.Text=_lic.Municipio;
            txtPortal.Text=_lic.Portal;
            txtOrgao.Text=_lic.Orgao;
            cmbStatus.SelectedIndex=(int)_lic.Status;
            txtValor.Text=_lic.ValorEstimado;
            txtEffecti.Text=_lic.CodigoEffecti;
            txtUASG.Text=_lic.UASG;
            txtHora.Text=_lic.HoraDisputa;
            txtCodigoBB.Text=_lic.CodigoBB;
            txtProdutos.Text=_lic.Produtos;
            if (_lic.DataDisputa.HasValue){chkData.Checked=true;dtpDisputa.Value=_lic.DataDisputa.Value;dtpDisputa.Enabled=true;}
            // Trigger visibility check after loading
            PortalTextChanged(null, EventArgs.Empty);
        }

        void Salvar(object? s,EventArgs e)
        {
            var errs=new List<string>();
            if (string.IsNullOrWhiteSpace(txtAno.Text))    errs.Add("Ano");
            if (cmbEstado.SelectedIndex<0)                 errs.Add("Estado");
            if (string.IsNullOrWhiteSpace(txtNumero.Text)) errs.Add("Número");
            if (string.IsNullOrWhiteSpace(txtPortal.Text)) errs.Add("Portal");
            if (string.IsNullOrWhiteSpace(txtOrgao.Text))  errs.Add("Órgão");
            if (string.IsNullOrWhiteSpace(txtProdutos.Text)) errs.Add("Objeto / Produtos");
            if (pnlCodigoBB.Visible && string.IsNullOrWhiteSpace(txtCodigoBB.Text)) errs.Add("Código BB");
            if (errs.Count>0){
                MessageBox.Show("Campos obrigatórios:\n• "+string.Join("\n• ",errs),"Atenção",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            _lic.Ano=txtAno.Text.Trim();
            _lic.Estado=ESTADOS[cmbEstado.SelectedIndex];
            _lic.Tipo=(TipoLicitacao)cmbTipo.SelectedIndex;
            _lic.Numero=txtNumero.Text.Trim();
            _lic.Municipio=txtMunicipio.Text.Trim();
            _lic.Portal=txtPortal.Text.Trim();
            _lic.Orgao=txtOrgao.Text.Trim();
            _lic.Produtos=txtProdutos.Text.Trim();
            _lic.Status=(StatusLicitacao)cmbStatus.SelectedIndex;
            _lic.ValorEstimado=txtValor.Text.Trim();
            _lic.CodigoEffecti=txtEffecti.Text.Trim();
            _lic.UASG=txtUASG.Text.Trim();
            _lic.HoraDisputa=txtHora.Text.Trim();
            _lic.DataDisputa=chkData.Checked?dtpDisputa.Value.Date:(DateTime?)null;
            _lic.CodigoBB=pnlCodigoBB.Visible?txtCodigoBB.Text.Trim():"";
            DialogResult=DialogResult.OK;
            Close();
        }

        // ── Helper builders ──────────────────────────────────────────────────
        static Color BG  = Color.FromArgb(250,250,250);
        static Color FG  = Color.FromArgb(60,64,67);
        static Color LBL = Color.FromArgb(112,117,122);

        void Lbl(TableLayoutPanel t,string text,int row,int col)
        {
            var l=new Label{Text=text,AutoSize=false,Dock=DockStyle.Fill,
                TextAlign=ContentAlignment.MiddleLeft,
                Font=new Font("Segoe UI",8,FontStyle.Bold),
                ForeColor=LBL,BackColor=Color.Transparent,
                Margin=new Padding(2,6,2,0)};
            t.Controls.Add(l);t.SetRow(l,row);t.SetColumn(l,col);
        }

        TextBox Txt(TableLayoutPanel t,int row,int col,int span=1)
        {
            var tb=new TextBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                Font=new Font("Segoe UI",9),BackColor=BG,ForeColor=FG,
                Margin=new Padding(2)};
            t.Controls.Add(tb);t.SetRow(tb,row);t.SetColumn(tb,col);
            if(span>1) t.SetColumnSpan(tb,span);
            return tb;
        }

        ComboBox Cmb(TableLayoutPanel t,int row,int col,string[]? items=null)
        {
            var cb=new ComboBox{Dock=DockStyle.Fill,FlatStyle=FlatStyle.Flat,
                Font=new Font("Segoe UI",9),BackColor=BG,ForeColor=FG,
                Margin=new Padding(2)};
            if(items!=null) cb.Items.AddRange(items);
            t.Controls.Add(cb);t.SetRow(cb,row);t.SetColumn(cb,col);
            return cb;
        }

        void Sec(TableLayoutPanel t,string text,int row,int span)
        {
            var p=new Panel{Dock=DockStyle.Fill,Height=28,BackColor=Color.Transparent,
                Margin=new Padding(0,10,0,4)};
            p.Controls.Add(new Label{Text=text,Font=new Font("Segoe UI",9,FontStyle.Bold),
                ForeColor=Color.FromArgb(26,115,232),Dock=DockStyle.Fill,
                TextAlign=ContentAlignment.MiddleLeft,BackColor=Color.Transparent});
            p.Controls.Add(new Panel{Dock=DockStyle.Bottom,Height=1,BackColor=Color.FromArgb(218,220,224)});
            t.Controls.Add(p);t.SetRow(p,row);t.SetColumn(p,0);
            if(span>1) t.SetColumnSpan(p,span);
        }

        static void StyleDtp(DateTimePicker d)
        {
            d.CalendarForeColor=Color.White;
            d.CalendarMonthBackground=Color.White;
            d.CalendarTitleBackColor=Color.FromArgb(26,115,232);
            d.CalendarTitleForeColor=Color.White;
        }
    }
}
