using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AgendaLicitacoes
{
    public class FormDetalhe : Form
    {
        readonly Licitacao _lic;
        public bool Modificado{get;private set;}=false;

        TabControl tabMain=null!;
        RichTextBox rtbDiario=null!;
        DataGridView dgvItens=null!,dgvAnexos=null!,dgvHistorico=null!;
        Panel pnlHeader=null!;
        Label lblStatusHeader=null!;
        TabPage tItens=null!, tAnexos=null!;

        static Color DarkBg =Color.FromArgb(255,255,255);
        static Color DarkMid=Color.FromArgb(245,245,245);
        static Color DarkBrd=Color.FromArgb(218,220,224);
        static Color TextFg =Color.FromArgb(60,64,67);
        static Color TextMut=Color.FromArgb(112,117,122);

        public FormDetalhe(Licitacao lic){_lic=lic;Build();Fill();}

        void Build()
        {
            var cor=StatusInfo.GetCor(_lic.Status);
            Text=_lic.GetTitulo();
            Size=new Size(1040,720);MinimumSize=new Size(800,500);
            StartPosition=FormStartPosition.CenterParent;
            BackColor=DarkBg;Font=new Font("Segoe UI",9);

            // ── Header ───────────────────────────────────────────────────────
            pnlHeader=new Panel{Dock=DockStyle.Top,Height=82,BackColor=cor};

            // REGRA DO WINFORMS: controles Dock=Right devem ser adicionados ANTES
            // dos controles Dock=Fill para que o Fill ocupe apenas o espaço restante.
            var pBtns=new FlowLayoutPanel{
                Dock=DockStyle.Right,
                AutoSize=true,
                AutoSizeMode=AutoSizeMode.GrowAndShrink,
                FlowDirection=FlowDirection.LeftToRight,
                WrapContents=false,
                BackColor=Color.Transparent,
                Padding=new Padding(0,24,12,0)
            };
            var btnAta=HBtn("🔗  Linkar Ata", Color.White);
            var btnEdt=HBtn("✏  Editar",      Color.White);
            var btnPst=HBtn("📁  Pasta",       Color.White);
            var btnDel=HBtn("🗑  Excluir",     Color.FromArgb(220,230,240));
            btnAta.Margin=new Padding(0,0,4,0);
            btnEdt.Margin=new Padding(0,0,4,0);
            btnPst.Margin=new Padding(0,0,4,0);
            btnDel.Margin=new Padding(0);
            btnAta.Click+=LinkarAtaClick;
            btnEdt.Click+=EditarClick;
            btnPst.Click+=PastaClick;
            btnDel.Click+=ExcluirClick;
            pBtns.Controls.AddRange(new Control[]{btnAta,btnEdt,btnPst,btnDel});
            pnlHeader.Controls.Add(pBtns); // <-- Right: adicionar PRIMEIRO

            // Fill ocupa o restante à esquerda dos botões
            var pTexto=new Panel{Dock=DockStyle.Fill,BackColor=Color.Transparent,Padding=new Padding(14,6,4,0)};
            var lblT=new Label{
                Text=_lic.GetTitulo(),ForeColor=Color.White,
                Font=new Font("Segoe UI",12,FontStyle.Bold),
                AutoSize=false,Dock=DockStyle.Top,Height=28};
            var lblO=new Label{
                Text=$"🏛  {(_lic.Orgao.Length>0?_lic.Orgao:"Órgão não informado")}",
                ForeColor=Color.FromArgb(220,240,255),Font=new Font("Segoe UI",8.5f),
                AutoSize=false,Dock=DockStyle.Top,Height=20};
            lblStatusHeader=new Label{
                Text=$"● {StatusInfo.GetNome(_lic.Status)}",
                ForeColor=Color.White,Font=new Font("Segoe UI",8,FontStyle.Bold),
                AutoSize=false,Dock=DockStyle.Top,Height=18};
            // Dock.Top empilha de baixo pra cima: adicionar na ordem inversa da exibição
            pTexto.Controls.Add(lblStatusHeader);
            pTexto.Controls.Add(lblO);
            pTexto.Controls.Add(lblT);
            pnlHeader.Controls.Add(pTexto); // <-- Fill: adicionar DEPOIS

            // ── Info strip ───────────────────────────────────────────────────
            var strip=new Panel{Dock=DockStyle.Top,Height=30,BackColor=DarkMid,Padding=new Padding(10,4,10,0)};
            var stripLbl=new Label{AutoSize=false,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,
                Font=new Font("Segoe UI",8),ForeColor=TextMut};
            stripLbl.Text=
                $"Disputa: {(_lic.DataDisputa.HasValue?_lic.DataDisputa.Value.ToString("dd/MM/yyyy")+" "+_lic.HoraDisputa:"—")}   |   " +
                $"{_lic.Municipio}/{_lic.Estado}   |   {_lic.Ano}   |   " +
                $"Effecti: {(_lic.CodigoEffecti.Length>0?_lic.CodigoEffecti:"—")}   |   UASG: {(_lic.UASG.Length>0?_lic.UASG:"—")}";
            strip.Controls.Add(stripLbl);

            // ── Tabs ─────────────────────────────────────────────────────────
            tabMain=new TabControl{Dock=DockStyle.Fill,Font=new Font("Segoe UI",9),Padding=new Point(12,4)};

            var tDiario  =new TabPage("📝  Diário");
            tItens       =new TabPage($"📦  Itens ({_lic.Itens.Count})");
            tAnexos      =new TabPage($"📎  Anexos ({_lic.Anexos.Count})");
            var tHist    =new TabPage("🕐  Histórico");
            foreach(var tp in new[]{tDiario,tItens,tAnexos,tHist}){
                tp.BackColor=DarkBg; tp.ForeColor=TextFg;}

            BuildDiario(tDiario);
            BuildItens(tItens);
            BuildAnexos(tAnexos);
            BuildHistorico(tHist);
            tabMain.TabPages.AddRange(new[]{tDiario,tItens,tAnexos,tHist});

            // ── Footer ───────────────────────────────────────────────────────
            var foot=new Panel{Dock=DockStyle.Bottom,Height=22,BackColor=DarkMid};
            var fLbl=new Label{AutoSize=false,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,
                Font=new Font("Segoe UI",7.5f),ForeColor=TextMut,
                Text=$"Criado: {_lic.DataCriacao:dd/MM/yyyy HH:mm}   Atualizado: {_lic.DataAtualizacao:dd/MM/yyyy HH:mm}",
                Padding=new Padding(8,0,0,0)};
            foot.Controls.Add(fLbl);

            Controls.AddRange(new Control[]{tabMain,strip,pnlHeader,foot});
        }

        Button HBtn(string t,Color bg){
            var b=new Button{Text=t,Width=108,Height=30,
                FlatStyle=FlatStyle.Flat,BackColor=bg,ForeColor=Color.FromArgb(60,64,67),
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold),Cursor=Cursors.Hand};
            b.FlatAppearance.BorderColor=Color.FromArgb(218,220,224);
            b.FlatAppearance.BorderSize=1;
            return b;
        }

        void TabDraw(object? s,DrawItemEventArgs e){
            var tp=tabMain.TabPages[e.Index];
            bool sel=e.Index==tabMain.SelectedIndex;
            using var bg=new SolidBrush(sel?Color.FromArgb(232,240,254):Color.White);
            e.Graphics.FillRectangle(bg,e.Bounds);
            using var fg=new SolidBrush(sel?Color.FromArgb(26,115,232):Color.FromArgb(112,117,122));
            var sf=new System.Drawing.StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};
            e.Graphics.DrawString(tp.Text,new Font("Segoe UI",8.5f,sel?FontStyle.Bold:FontStyle.Regular),fg,e.Bounds,sf);
        }

        void BuildDiario(TabPage tp){
            var bar=new ToolStrip{GripStyle=ToolStripGripStyle.Hidden,BackColor=Color.FromArgb(245,245,245),
                RenderMode=ToolStripRenderMode.System};
            var bAdd=new ToolStripButton("➕ Adicionar Entrada"){ForeColor=TextFg};
            var bSav=new ToolStripButton("💾 Salvar no Arquivo"){ForeColor=TextFg};
            bAdd.Click+=(s,e)=>{
                var d=new FormAddNota();
                if(d.ShowDialog(this)==DialogResult.OK){
                    var entry=$"\n\n[{DateTime.Now:dd/MM/yyyy HH:mm}] {d.Texto}";
                    rtbDiario.AppendText(entry);
                    _lic.Diario=rtbDiario.Text;
                    DataService.SalvarDiarioArquivo(_lic);
                    _lic.AddHistorico("Entrada adicionada ao diário");
                    DataService.Atualizar(_lic);Modificado=true;}};
            bSav.Click+=(s,e)=>{
                _lic.Diario=rtbDiario.Text;
                DataService.SalvarDiarioArquivo(_lic);DataService.Atualizar(_lic);
                MessageBox.Show("Diário salvo!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);};
            bar.Items.AddRange(new ToolStripItem[]{bAdd,new ToolStripSeparator(),bSav});
            rtbDiario=new RichTextBox{Dock=DockStyle.Fill,Font=new Font("Consolas",9),
                BorderStyle=BorderStyle.None,BackColor=Color.White,
                ForeColor=Color.FromArgb(60,64,67),ScrollBars=RichTextBoxScrollBars.Vertical};
            tp.Controls.AddRange(new Control[]{rtbDiario,bar});
        }

        void BuildItens(TabPage tp){
            var bar=new ToolStrip{GripStyle=ToolStripGripStyle.Hidden,BackColor=Color.FromArgb(245,245,245)};
            var bAdd   = new ToolStripButton("➕ Adicionar Item"){ForeColor=TextFg};
            var bEdit  = new ToolStripButton("✏ Editar Item"){ForeColor=TextFg};
            var bDel   = new ToolStripButton("🗑 Remover"){ForeColor=TextFg};
            var bSep1  = new ToolStripSeparator();
            var bGanho = new ToolStripButton("✅ Todos Ganho"){ForeColor=Color.FromArgb(22,163,74),Font=new Font("Segoe UI",8.5f,FontStyle.Bold)};
            var bPerd  = new ToolStripButton("❌ Todos Perdido"){ForeColor=Color.FromArgb(220,38,38),Font=new Font("Segoe UI",8.5f,FontStyle.Bold)};

            bAdd.Click+=(s,e)=>{
                var d=new FormAddItem();
                if(d.ShowDialog(this)==DialogResult.OK){
                    _lic.Itens.Add(d.Item);_lic.AddHistorico("Item adicionado: "+d.Item.Descricao);
                    DataService.Atualizar(_lic);RefreshItens();
                    tp.Text=$"📦  Itens ({_lic.Itens.Count})";Modificado=true;}};

            bEdit.Click+=(s,e)=>EditarItemSelecionado(tp);

            bDel.Click+=(s,e)=>{
                if(dgvItens.CurrentRow?.Index>=0&&dgvItens.CurrentRow.Index<_lic.Itens.Count){
                    var i=dgvItens.CurrentRow.Index;
                    if(MessageBox.Show($"Remover \"{_lic.Itens[i].Descricao}\"?","Confirmar",MessageBoxButtons.YesNo)==DialogResult.Yes){
                        _lic.Itens.RemoveAt(i);DataService.Atualizar(_lic);RefreshItens();
                        tp.Text=$"📦  Itens ({_lic.Itens.Count})";Modificado=true;}}};

            bGanho.Click+=(s,e)=>{
                if(_lic.Itens.Count==0) return;
                foreach(var it in _lic.Itens) it.Ganho=true;
                AtualizarStatusPorItens();
                DataService.Atualizar(_lic);RefreshItens();Modificado=true;
                AtualizarHeader();};

            bPerd.Click+=(s,e)=>{
                if(_lic.Itens.Count==0) return;
                foreach(var it in _lic.Itens) it.Ganho=false;
                AtualizarStatusPorItens();
                DataService.Atualizar(_lic);RefreshItens();Modificado=true;
                AtualizarHeader();};

            bar.Items.AddRange(new ToolStripItem[]{bAdd,new ToolStripSeparator(),bEdit,new ToolStripSeparator(),bDel,bSep1,bGanho,bPerd});

            dgvItens=MkGrid();
            dgvItens.Columns.AddRange(
                new DataGridViewTextBoxColumn{HeaderText="#",Width=36},
                new DataGridViewTextBoxColumn{HeaderText="Item",Width=50},
                new DataGridViewTextBoxColumn{HeaderText="Código",Width=80},
                new DataGridViewTextBoxColumn{HeaderText="Descrição",AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill},
                new DataGridViewTextBoxColumn{HeaderText="Qtd",Width=55},
                new DataGridViewTextBoxColumn{HeaderText="Vl.Unit.",Width=90},
                new DataGridViewTextBoxColumn{HeaderText="Vl.Total",Width=90},
                new DataGridViewTextBoxColumn{HeaderText="Resultado",Width=90,Name="colResultado"});

            dgvItens.CellFormatting+=ItensCellFormatting;
            dgvItens.CellDoubleClick+=(s,e)=>{if(e.RowIndex>=0) EditarItemSelecionado(tp);};

            tp.Controls.AddRange(new Control[]{dgvItens,bar});
        }

        void EditarItemSelecionado(TabPage tp)
        {
            int idx = dgvItens.CurrentRow?.Index ?? -1;
            if(idx<0||idx>=_lic.Itens.Count) return;
            var dlg = new FormEditItem(_lic.Itens[idx]);
            if(dlg.ShowDialog(this)!=DialogResult.OK) return;
            // Item already mutated in place by FormEditItem
            AtualizarStatusPorItens();
            DataService.Atualizar(_lic);
            RefreshItens();
            tp.Text=$"📦  Itens ({_lic.Itens.Count})";
            Modificado=true;
            AtualizarHeader();
        }

        // Atualiza o status da licitação com base nos itens
        void AtualizarStatusPorItens()
        {
            if(_lic.Itens.Count==0) return;
            bool algumGanho    = _lic.Itens.Any(i=>i.Ganho==true);
            bool todosDefinidos= _lic.Itens.All(i=>i.Ganho.HasValue);
            bool todosPerdidos  = todosDefinidos && _lic.Itens.All(i=>i.Ganho==false);
            bool todosGanhos    = todosDefinidos && _lic.Itens.All(i=>i.Ganho==true);

            if(algumGanho)
                _lic.Status=StatusLicitacao.Ganho;
            else if(todosPerdidos)
                _lic.Status=StatusLicitacao.Perdido;

            _lic.AddHistorico($"Status atualizado para {StatusInfo.GetNome(_lic.Status)} via itens");
        }

        void AtualizarHeader()
        {
            var cor=StatusInfo.GetCor(_lic.Status);
            pnlHeader.BackColor=cor;
            lblStatusHeader.Text=$"● {StatusInfo.GetNome(_lic.Status)}";
        }

        void ItensCellFormatting(object? s,DataGridViewCellFormattingEventArgs e)
        {
            if(e.RowIndex<0||e.RowIndex>=_lic.Itens.Count) return;
            var it=_lic.Itens[e.RowIndex];
            var col=dgvItens.Columns[e.ColumnIndex];
            if(col.Name=="colResultado"){
                if(it.Ganho==true){
                    e.CellStyle.BackColor=Color.FromArgb(220,252,231);
                    e.CellStyle.ForeColor=Color.FromArgb(22,163,74);
                    e.CellStyle.Font=new Font("Segoe UI",8.5f,FontStyle.Bold);
                } else if(it.Ganho==false){
                    e.CellStyle.BackColor=Color.FromArgb(254,226,226);
                    e.CellStyle.ForeColor=Color.FromArgb(220,38,38);
                    e.CellStyle.Font=new Font("Segoe UI",8.5f,FontStyle.Bold);
                } else {
                    e.CellStyle.BackColor=Color.FromArgb(245,245,245);
                    e.CellStyle.ForeColor=Color.FromArgb(112,117,122);
                }
            }
        }

        void BuildAnexos(TabPage tp){
            var bar=new ToolStrip{GripStyle=ToolStripGripStyle.Hidden,BackColor=Color.FromArgb(245,245,245)};
            var bAdd=new ToolStripButton("➕ Adicionar"){ForeColor=TextFg};
            var bOpn=new ToolStripButton("📂 Abrir"){ForeColor=TextFg};
            var bDel=new ToolStripButton("🗑 Remover"){ForeColor=TextFg};
            bAdd.Click+=(s,e)=>{
                var tipos=new string[]{"Proposta Inicial","Proposta Final","Empenhos","Edital","Termo de Referência","Resultado","ATA","Outros"};
                var dlgTipo=new Form{Text="Tipo do Anexo",Size=new Size(340,180),StartPosition=FormStartPosition.CenterParent,
                    BackColor=Color.FromArgb(245,245,245),Font=new Font("Segoe UI",9),FormBorderStyle=FormBorderStyle.FixedDialog,
                    MaximizeBox=false,MinimizeBox=false};
                dlgTipo.Controls.Add(new Label{Text="Selecione o tipo do anexo:",Left=16,Top=14,AutoSize=true,ForeColor=Color.FromArgb(60,64,67)});
                var cmbTipo=new ComboBox{Left=16,Top=38,Width=290,DropDownStyle=ComboBoxStyle.DropDownList,
                    BackColor=Color.White,ForeColor=Color.FromArgb(60,64,67)};
                cmbTipo.Items.AddRange(tipos);cmbTipo.SelectedIndex=0;
                dlgTipo.Controls.Add(cmbTipo);
                var bOkT=new Button{Text="OK",DialogResult=DialogResult.OK,Left=120,Top=80,Width=90,Height=30,
                    FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(26,115,232),ForeColor=Color.White};
                bOkT.FlatAppearance.BorderSize=0;
                var bNoT=new Button{Text="Cancelar",DialogResult=DialogResult.Cancel,Left=216,Top=80,Width=90,Height=30,
                    FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(241,243,244),ForeColor=Color.FromArgb(112,117,122)};
                dlgTipo.Controls.AddRange(new Control[]{bOkT,bNoT});
                dlgTipo.AcceptButton=bOkT;dlgTipo.CancelButton=bNoT;
                if(dlgTipo.ShowDialog(this)!=DialogResult.OK) return;
                var tipoSel=(string)cmbTipo.SelectedItem!;
                var ofd=new OpenFileDialog{Filter="Todos os Arquivos (*.*)|*.*",Title=$"Selecionar arquivo - {tipoSel}"};
                if(ofd.ShowDialog()==DialogResult.OK){
                    var subFolder=tipoSel;
                    // Caminho final do anexo: usa a cópia na pasta da licitação quando possível
                    string caminhoAnexo = ofd.FileName;
                    if(!string.IsNullOrEmpty(_lic.PastaServidor)){
                        var destDir=Path.Combine(_lic.PastaServidor,subFolder);
                        try{
                            Directory.CreateDirectory(destDir);
                            var destFile=Path.Combine(destDir,Path.GetFileName(ofd.FileName));
                            if(!File.Exists(destFile)) File.Copy(ofd.FileName,destFile);
                            caminhoAnexo = destFile; // aponta para a cópia
                        }catch{}
                    }
                    _lic.Anexos.Add(new Anexo{Nome=Path.GetFileName(ofd.FileName),Caminho=caminhoAnexo,
                        Tipo=tipoSel,DataAdd=DateTime.Now});
                    _lic.AddHistorico($"Anexo ({tipoSel}): "+Path.GetFileName(ofd.FileName));
                    DataService.Atualizar(_lic);RefreshAnexos();
                    tp.Text=$"Anexos ({_lic.Anexos.Count})";Modificado=true;}};
            bOpn.Click+=(s,e)=>{
                if(dgvAnexos.CurrentRow?.Index>=0&&dgvAnexos.CurrentRow.Index<_lic.Anexos.Count){
                    var p=_lic.Anexos[dgvAnexos.CurrentRow.Index].Caminho;
                    if(File.Exists(p)) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(p){UseShellExecute=true});
                    else MessageBox.Show("Não encontrado:\n"+p,"Aviso",MessageBoxButtons.OK,MessageBoxIcon.Warning);}};
            bDel.Click+=(s,e)=>{
                if(dgvAnexos.CurrentRow?.Index>=0&&dgvAnexos.CurrentRow.Index<_lic.Anexos.Count){
                    var i=dgvAnexos.CurrentRow.Index;
                    if(MessageBox.Show($"Remover \"{_lic.Anexos[i].Nome}\"?","Confirmar",MessageBoxButtons.YesNo)==DialogResult.Yes){
                        _lic.Anexos.RemoveAt(i);DataService.Atualizar(_lic);RefreshAnexos();
                        tp.Text=$"📎  Anexos ({_lic.Anexos.Count})";Modificado=true;}}};
            bar.Items.AddRange(new ToolStripItem[]{bAdd,new ToolStripSeparator(),bOpn,new ToolStripSeparator(),bDel});
            dgvAnexos=MkGrid();
            dgvAnexos.CellDoubleClick+=(s,e)=>bOpn.PerformClick();
            dgvAnexos.Columns.AddRange(
                new DataGridViewTextBoxColumn{HeaderText="#",Width=36},
                new DataGridViewTextBoxColumn{HeaderText="Nome",AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill},
                new DataGridViewTextBoxColumn{HeaderText="Tipo",Width=60},
                new DataGridViewTextBoxColumn{HeaderText="Data",Width=100},
                new DataGridViewTextBoxColumn{HeaderText="Caminho",Width=260});
            tp.Controls.AddRange(new Control[]{dgvAnexos,bar});
        }

        void BuildHistorico(TabPage tp){
            dgvHistorico=MkGrid();dgvHistorico.Dock=DockStyle.Fill;
            dgvHistorico.Columns.AddRange(
                new DataGridViewTextBoxColumn{HeaderText="Data/Hora",Width=130},
                new DataGridViewTextBoxColumn{HeaderText="Descrição",AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill},
                new DataGridViewTextBoxColumn{HeaderText="Usuário",Width=100});
            tp.Controls.Add(dgvHistorico);
        }

        void Fill(){
            rtbDiario.Text=_lic.Diario;
            RefreshItens();RefreshAnexos();RefreshHistorico();
        }
        void RefreshItens(){
            dgvItens.Rows.Clear();
            for(int i=0;i<_lic.Itens.Count;i++){
                var it=_lic.Itens[i];
                string resultado = it.Ganho==true?"✅ Ganho":it.Ganho==false?"❌ Perdido":"—";
                dgvItens.Rows.Add(i+1,it.Numero,it.Codigo,it.Descricao,it.Quantidade,it.ValorUnitario,it.ValorTotal,resultado);}
        }
        void RefreshAnexos(){
            dgvAnexos.Rows.Clear();
            for(int i=0;i<_lic.Anexos.Count;i++){
                var ax=_lic.Anexos[i];
                dgvAnexos.Rows.Add(i+1,ax.Nome,ax.Tipo,ax.DataAdd.ToString("dd/MM/yyyy"),ax.Caminho);}
        }
        void RefreshHistorico(){
            dgvHistorico.Rows.Clear();
            for(int i=_lic.Historico.Count-1;i>=0;i--){
                var h=_lic.Historico[i];
                dgvHistorico.Rows.Add(h.DataHora.ToString("dd/MM/yyyy HH:mm"),h.Descricao,h.Usuario);}
        }

        void EditarClick(object? s,EventArgs e){
            var dlg=new FormLicitacao(_lic);
            if(dlg.ShowDialog(this)!=DialogResult.OK) return;
            var ed=dlg.Resultado;
            _lic.Ano=ed.Ano;_lic.Estado=ed.Estado;_lic.Municipio=ed.Municipio;
            _lic.Orgao=ed.Orgao;_lic.Tipo=ed.Tipo;_lic.Numero=ed.Numero;
            _lic.Portal=ed.Portal;_lic.Status=ed.Status;_lic.DataDisputa=ed.DataDisputa;
            _lic.HoraDisputa=ed.HoraDisputa;_lic.Produtos=ed.Produtos;
            _lic.ValorEstimado=ed.ValorEstimado;_lic.CodigoEffecti=ed.CodigoEffecti;
            _lic.UASG=ed.UASG;_lic.CodigoBB=ed.CodigoBB;
            _lic.AddHistorico("Licitação editada");DataService.Atualizar(_lic);
            Modificado=true;
            AtualizarHeader();
            Text=_lic.GetTitulo();
            rtbDiario.Text=_lic.Diario;
        }
        void PastaClick(object? s,EventArgs e){
            var path=!string.IsNullOrEmpty(_lic.PastaCliente)?_lic.PastaCliente:_lic.PastaServidor;
            if(!string.IsNullOrEmpty(path)&&System.IO.Directory.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe",path);
            else MessageBox.Show("Pasta não encontrada:\n"+path,"Aviso",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
        void ExcluirClick(object? s,EventArgs e){
            if(MessageBox.Show($"Excluir:\n{_lic.GetTitulo()}?","Confirmar exclusão",
               MessageBoxButtons.YesNo,MessageBoxIcon.Warning)==DialogResult.Yes){
                DataService.Remover(_lic.Id);Modificado=true;Tag="deleted";Close();}
        }

        void LinkarAtaClick(object? s, EventArgs e) {
            var ofd=new OpenFileDialog{Filter="Todos os Arquivos (*.*)|*.*",Title="Selecionar arquivo da ATA"};
            if(ofd.ShowDialog()!=DialogResult.OK) return;
            
            string caminhoAnexo = ofd.FileName;
            if(!string.IsNullOrEmpty(_lic.PastaServidor)){
                var destDir=Path.Combine(_lic.PastaServidor,"ATA");
                try{
                    Directory.CreateDirectory(destDir);
                    var destFile=Path.Combine(destDir,Path.GetFileName(ofd.FileName));
                    if(!File.Exists(destFile)) File.Copy(ofd.FileName,destFile);
                    caminhoAnexo = destFile;
                }catch{}
            }
            
            var dlg = new FormAtaDates(_lic.DataInicioAta, _lic.DataFimAta);
            if(dlg.ShowDialog(this) == DialogResult.OK) {
                _lic.DataInicioAta = dlg.DataInicio;
                _lic.DataFimAta = dlg.DataFim;
                _lic.Status = StatusLicitacao.Ata;
                _lic.Anexos.Add(new Anexo{Nome=Path.GetFileName(ofd.FileName),Caminho=caminhoAnexo, Tipo="ATA",DataAdd=DateTime.Now});
                _lic.AddHistorico($"ATA vinculada: {Path.GetFileName(ofd.FileName)} (Validade: {dlg.DataInicio:dd/MM/yyyy} a {dlg.DataFim:dd/MM/yyyy})");
                
                DataService.Atualizar(_lic);
                RefreshAnexos();
                AtualizarHeader();
                Modificado=true;
                if (tAnexos != null) tAnexos.Text=$"📎  Anexos ({_lic.Anexos.Count})";
                MessageBox.Show("ATA vinculada com sucesso!","Sucesso",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        DataGridView MkGrid(){
            var g=new DataGridView{Dock=DockStyle.Fill,BorderStyle=BorderStyle.None,
                RowHeadersVisible=false,AllowUserToAddRows=false,AllowUserToDeleteRows=false,
                AllowUserToResizeRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor=Color.White,GridColor=Color.FromArgb(218,220,224),
                Font=new Font("Segoe UI",8.5f),ReadOnly=true,
                ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells};
            g.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(245,245,245);
            g.ColumnHeadersDefaultCellStyle.ForeColor=Color.FromArgb(112,117,122);
            g.ColumnHeadersDefaultCellStyle.Font=new Font("Segoe UI",8.5f,FontStyle.Bold);
            g.DefaultCellStyle.BackColor=Color.White;
            g.DefaultCellStyle.ForeColor=Color.FromArgb(60,64,67);
            g.DefaultCellStyle.SelectionBackColor=Color.FromArgb(232,240,254);
            g.DefaultCellStyle.SelectionForeColor=Color.FromArgb(60,64,67);
            g.EnableHeadersVisualStyles=false;
            return g;
        }
    }

    // ── Mini dialogs ─────────────────────────────────────────────────────────
    public class FormAddNota:Form{
        RichTextBox rtb=null!;
        public string Texto=>rtb.Text;
        public FormAddNota(){
            Text="Adicionar ao Diário";Size=new Size(500,220);
            StartPosition=FormStartPosition.CenterParent;
            Font=new Font("Segoe UI",9);BackColor=Color.FromArgb(245,245,245);
            var lbl=new Label{Text=$"[{DateTime.Now:dd/MM/yyyy HH:mm}]  Texto:",
                Dock=DockStyle.Top,Height=24,ForeColor=Color.FromArgb(148,163,184),
                TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(8,0,0,0),BackColor=Color.Transparent};
            rtb=new RichTextBox{Dock=DockStyle.Fill,Font=new Font("Segoe UI",9),
                BackColor=Color.White,ForeColor=Color.FromArgb(60,64,67),BorderStyle=BorderStyle.None};
            var pBtn=new Panel{Dock=DockStyle.Bottom,Height=40,BackColor=Color.FromArgb(245,245,245)};
            var bOk=new Button{Text="Confirmar",DialogResult=DialogResult.OK,Width=100,Height=30,
                Left=110,Top=5,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(26,115,232),ForeColor=Color.White};
            bOk.FlatAppearance.BorderSize=0;
            var bNo=new Button{Text="Cancelar",DialogResult=DialogResult.Cancel,Width=100,Height=30,
                Left=5,Top=5,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(241,243,244),ForeColor=Color.FromArgb(112,117,122)};
            pBtn.Controls.AddRange(new Control[]{bOk,bNo});
            Controls.AddRange(new Control[]{rtb,lbl,pBtn});
            AcceptButton=bOk;CancelButton=bNo;
        }
    }

    public class FormAddItem:Form{
        public Item Item{get;}=new();
        TextBox txtN=null!,txtC=null!,txtD=null!,txtQ=null!,txtVU=null!,txtVT=null!;
        public FormAddItem(){
            Text="Adicionar Item";Size=new Size(500,280);
            StartPosition=FormStartPosition.CenterParent;
            Font=new Font("Segoe UI",9);BackColor=Color.FromArgb(245,245,245);
            var tbl=new TableLayoutPanel{ColumnCount=2,Dock=DockStyle.Fill,Padding=new Padding(12),BackColor=Color.Transparent};
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,130));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            void R(string l,ref TextBox tb){
                tbl.Controls.Add(new Label{Text=l,TextAlign=ContentAlignment.MiddleLeft,
                    Dock=DockStyle.Fill,ForeColor=Color.FromArgb(112,117,122),BackColor=Color.Transparent});
                tb=new TextBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                    Margin=new Padding(2),BackColor=Color.White,ForeColor=Color.FromArgb(60,64,67)};
                tbl.Controls.Add(tb);}
            R("Número Item *",ref txtN!);
            R("Código Item *",ref txtC!);
            R("Descrição",ref txtD!);
            R("Quantidade",ref txtQ!);
            R("Valor Unitário",ref txtVU!);
            // Valor Total: readonly, calculado automaticamente
            tbl.Controls.Add(new Label{Text="Valor Total",TextAlign=ContentAlignment.MiddleLeft,
                Dock=DockStyle.Fill,ForeColor=Color.FromArgb(112,117,122),BackColor=Color.Transparent});
            txtVT=new TextBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                Margin=new Padding(2),BackColor=Color.FromArgb(240,242,245),ForeColor=Color.FromArgb(60,64,67),ReadOnly=true};
            tbl.Controls.Add(txtVT);
            // Calcular total automaticamente
            void CalcTotal(){
                var vu=txtVU.Text.Replace(",",".");
                var qq=txtQ.Text.Replace(",",".");
                if(double.TryParse(vu,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out double vud)
                && double.TryParse(qq,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out double qqd))
                    txtVT.Text=(vud*qqd).ToString("N2");
                else txtVT.Text="";}
            txtVU.TextChanged+=(s,e)=>CalcTotal();
            txtQ.TextChanged+=(s,e)=>CalcTotal();
            var pBtn=new Panel{Dock=DockStyle.Bottom,Height=40,BackColor=Color.FromArgb(245,245,245)};
            var bOk=new Button{Text="Salvar",Width=100,Height=30,Left=110,Top=5,
                DialogResult=DialogResult.OK,FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(26,115,232),ForeColor=Color.White};
            bOk.FlatAppearance.BorderSize=0;
            bOk.Click+=(s,e)=>{
                if(string.IsNullOrWhiteSpace(txtN.Text)){MessageBox.Show("Número do Item obrigatório!");DialogResult=DialogResult.None;return;}
                if(string.IsNullOrWhiteSpace(txtC.Text)){MessageBox.Show("Código obrigatório!");DialogResult=DialogResult.None;return;}
                Item.Numero=txtN.Text;
                Item.Codigo=txtC.Text;Item.Descricao=txtD.Text;Item.Quantidade=txtQ.Text;
                Item.ValorUnitario=txtVU.Text;Item.ValorTotal=txtVT.Text;};
            var bNo=new Button{Text="Cancelar",Width=100,Height=30,Left=5,Top=5,
                DialogResult=DialogResult.Cancel,FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(241,243,244),ForeColor=Color.FromArgb(112,117,122)};
            pBtn.Controls.AddRange(new Control[]{bOk,bNo});
            Controls.AddRange(new Control[]{tbl,pBtn});
            AcceptButton=bOk;CancelButton=bNo;
        }
    }

    // ── FormEditItem: editar item existente com campo de resultado ────────────
    public class FormEditItem : Form
    {
        readonly Item _item;
        TextBox txtN=null!,txtC=null!,txtD=null!,txtQ=null!,txtVU=null!,txtVT=null!;
        ComboBox cmbResultado=null!;

        public FormEditItem(Item item)
        {
            _item=item;
            Text="Editar Item";Size=new Size(520,340);
            StartPosition=FormStartPosition.CenterParent;
            Font=new Font("Segoe UI",9);BackColor=Color.FromArgb(245,245,245);
            FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;

            var tbl=new TableLayoutPanel{ColumnCount=2,Dock=DockStyle.Fill,Padding=new Padding(12),BackColor=Color.Transparent};
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,140));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));

            void R(string l,ref TextBox tb){
                tbl.Controls.Add(new Label{Text=l,TextAlign=ContentAlignment.MiddleLeft,
                    Dock=DockStyle.Fill,ForeColor=Color.FromArgb(112,117,122),BackColor=Color.Transparent});
                tb=new TextBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                    Margin=new Padding(2),BackColor=Color.White,ForeColor=Color.FromArgb(60,64,67)};
                tbl.Controls.Add(tb);}

            R("Número Item *",ref txtN!);
            R("Código Item *",ref txtC!);
            R("Descrição",ref txtD!);
            R("Quantidade",ref txtQ!);
            R("Valor Unitário",ref txtVU!);
            // Valor Total: readonly, calculado automaticamente
            tbl.Controls.Add(new Label{Text="Valor Total",TextAlign=ContentAlignment.MiddleLeft,
                Dock=DockStyle.Fill,ForeColor=Color.FromArgb(112,117,122),BackColor=Color.Transparent});
            txtVT=new TextBox{Dock=DockStyle.Fill,BorderStyle=BorderStyle.FixedSingle,
                Margin=new Padding(2),BackColor=Color.FromArgb(240,242,245),ForeColor=Color.FromArgb(60,64,67),ReadOnly=true};
            tbl.Controls.Add(txtVT);

            // Calcular total automaticamente
            void CalcTotal(){
                var vu=txtVU.Text.Replace(",",".");
                var qq=txtQ.Text.Replace(",",".");
                if(double.TryParse(vu,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out double vud)
                && double.TryParse(qq,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out double qqd))
                    txtVT.Text=(vud*qqd).ToString("N2");
                else txtVT.Text=_item.ValorTotal;}
            txtVU.TextChanged+=(s,e)=>CalcTotal();
            txtQ.TextChanged+=(s,e)=>CalcTotal();

            // Resultado do item
            tbl.Controls.Add(new Label{Text="Resultado",TextAlign=ContentAlignment.MiddleLeft,
                Dock=DockStyle.Fill,ForeColor=Color.FromArgb(60,64,67),Font=new Font("Segoe UI",9,FontStyle.Bold),BackColor=Color.Transparent});
            cmbResultado=new ComboBox{Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,
                BackColor=Color.White,ForeColor=Color.FromArgb(60,64,67),Margin=new Padding(2)};
            cmbResultado.Items.AddRange(new object[]{"— Não definido","✅ Ganho","❌ Perdido"});
            tbl.Controls.Add(cmbResultado);

            // Load values
            txtN.Text=_item.Numero;txtC.Text=_item.Codigo;txtD.Text=_item.Descricao;
            txtQ.Text=_item.Quantidade;
            txtVU.Text=_item.ValorUnitario;txtVT.Text=_item.ValorTotal;
            cmbResultado.SelectedIndex=_item.Ganho==null?0:_item.Ganho==true?1:2;

            var pBtn=new Panel{Dock=DockStyle.Bottom,Height=46,BackColor=Color.FromArgb(245,245,245),Padding=new Padding(8,8,8,8)};
            var bOk=new Button{Text="💾 Salvar",Width=110,Height=30,Dock=DockStyle.Right,
                FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(26,115,232),ForeColor=Color.White,
                Font=new Font("Segoe UI",9,FontStyle.Bold)};
            bOk.FlatAppearance.BorderSize=0;
            bOk.Click+=Salvar;
            var bNo=new Button{Text="Cancelar",Width=100,Height=30,Dock=DockStyle.Right,
                DialogResult=DialogResult.Cancel,FlatStyle=FlatStyle.Flat,
                BackColor=Color.FromArgb(241,243,244),ForeColor=Color.FromArgb(112,117,122)};
            pBtn.Controls.AddRange(new Control[]{bOk,bNo});

            Controls.AddRange(new Control[]{tbl,pBtn});
            AcceptButton=bOk;CancelButton=bNo;
        }

        void Salvar(object? s,EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtN.Text)){MessageBox.Show("Número do Item obrigatório!");return;}
            if(string.IsNullOrWhiteSpace(txtC.Text)){MessageBox.Show("Código obrigatório!");return;}
            _item.Numero=txtN.Text;_item.Codigo=txtC.Text;_item.Descricao=txtD.Text;
            _item.Quantidade=txtQ.Text;
            _item.ValorUnitario=txtVU.Text;_item.ValorTotal=txtVT.Text;
            _item.Ganho=cmbResultado.SelectedIndex==0?(bool?)null:cmbResultado.SelectedIndex==1;
            DialogResult=DialogResult.OK;Close();
        }
    }

    public class FormAtaDates : Form {
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public FormAtaDates(DateTime? ini, DateTime? fim) {
            Text="Datas da ATA"; Size=new Size(320, 200);
            StartPosition=FormStartPosition.CenterParent;
            Font=new Font("Segoe UI",9); BackColor=Color.FromArgb(245,245,245);
            FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; MinimizeBox=false;
            
            var dtIni = new DateTimePicker{Format=DateTimePickerFormat.Short, Left=20, Top=40, Width=120};
            var dtFim = new DateTimePicker{Format=DateTimePickerFormat.Short, Left=160, Top=40, Width=120};
            
            if(ini.HasValue) dtIni.Value=ini.Value; else dtIni.Value=DateTime.Today;
            if(fim.HasValue) dtFim.Value=fim.Value; else dtFim.Value=DateTime.Today.AddMonths(12);

            Controls.Add(new Label{Text="Início:", Left=20, Top=20, AutoSize=true, ForeColor=Color.FromArgb(60,64,67)});
            Controls.Add(new Label{Text="Fim:", Left=160, Top=20, AutoSize=true, ForeColor=Color.FromArgb(60,64,67)});
            Controls.Add(dtIni); Controls.Add(dtFim);
            
            var bOk = new Button{Text="Salvar", DialogResult=DialogResult.OK, Left=50, Top=100, Width=90, Height=30, BackColor=Color.FromArgb(26,115,232), ForeColor=Color.White, FlatStyle=FlatStyle.Flat};
            bOk.FlatAppearance.BorderSize=0;
            var bNo = new Button{Text="Cancelar", DialogResult=DialogResult.Cancel, Left=150, Top=100, Width=90, Height=30, BackColor=Color.FromArgb(241,243,244), FlatStyle=FlatStyle.Flat};
            Controls.Add(bOk); Controls.Add(bNo);
            AcceptButton=bOk; CancelButton=bNo;
            
            bOk.Click+=(s,e)=> { DataInicio=dtIni.Value; DataFim=dtFim.Value; };
        }
    }
}
