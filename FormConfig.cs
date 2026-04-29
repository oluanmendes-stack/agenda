using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AgendaLicitacoes
{
    public class FormConfig:Form
    {
        CheckBox chkAS=null!,chkCP=null!;

        static Color Bg =Color.FromArgb(255,255,255);
        static Color Mid=Color.FromArgb(245,245,245);
        static Color Brd=Color.FromArgb(218,220,224);
        static Color Fg =Color.FromArgb(60,64,67);
        static Color Mut=Color.FromArgb(112,117,122);
        static Color Acc=Color.FromArgb(26,115,232);

        public FormConfig()
        {
            Text="Configurações";Size=new Size(660,400);
            MinimumSize=new Size(580,340);StartPosition=FormStartPosition.CenterParent;
            BackColor=Bg;Font=new Font("Segoe UI",9);

            var hdr=new Panel{Dock=DockStyle.Top,Height=50,BackColor=Mid};
            hdr.Controls.Add(new Label{Text="⚙  Configurações",ForeColor=Fg,
                Font=new Font("Segoe UI",12,FontStyle.Bold),Dock=DockStyle.Fill,
                TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(16,0,0,0)});

            var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=Bg,Padding=new Padding(16,10,16,10)};
            var tbl=new TableLayoutPanel{ColumnCount=1,AutoSize=true,Dock=DockStyle.Top,BackColor=Color.Transparent};
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));

            void Sec(string t){
                var p=new Panel{Height=28,Dock=DockStyle.Top,BackColor=Color.Transparent,Margin=new Padding(0,10,0,4)};
                p.Controls.Add(new Label{Text=t,Font=new Font("Segoe UI",9,FontStyle.Bold),
                    ForeColor=Acc,Dock=DockStyle.Fill,
                    TextAlign=ContentAlignment.MiddleLeft,BackColor=Color.Transparent});
                p.Controls.Add(new Panel{Dock=DockStyle.Bottom,Height=1,BackColor=Brd});
                tbl.Controls.Add(p);}
            void Desc(string t) => tbl.Controls.Add(new Label{Text=t,Font=new Font("Segoe UI",8),
                ForeColor=Mut,AutoSize=true,Margin=new Padding(0,0,0,6),BackColor=Color.Transparent});

            // ── Pasta raiz ───────────────────────────────────────────────────
            Sec("📁  Pasta Raiz (detectada automaticamente)");
            Desc("O programa localiza a pasta \"01 - EDITAIS E PROPOSTAS\" no mesmo diretório do executável\n" +
                 "(ou sobe até 4 níveis caso o .exe esteja em subpasta como bin\\Release).\n" +
                 "Se não existir, a pasta será criada automaticamente ao salvar a primeira licitação.");

            // Caixa de leitura mostrando o caminho detectado
            var pPath = new Panel{Height=32,Dock=DockStyle.Top,BackColor=Color.Transparent,Margin=new Padding(0,4,0,4)};
            var txtPath = new TextBox{
                Left=0,Top=3,Width=560,Height=26,
                Anchor=AnchorStyles.Left|AnchorStyles.Top|AnchorStyles.Right,
                Font=new Font("Consolas",8.5f),
                BackColor=Color.FromArgb(240,242,245),ForeColor=Fg,
                BorderStyle=BorderStyle.FixedSingle,
                ReadOnly=true,
                Text=DataService.PastaRaizDetectada
            };
            var btnAbrir=new Button{
                Text="📂",Width=34,Height=26,
                Left=txtp_right(txtPath),Top=3,
                FlatStyle=FlatStyle.Flat,
                BackColor=Mid,ForeColor=Fg,
                Cursor=Cursors.Hand
            };
            btnAbrir.FlatAppearance.BorderColor=Brd;
            btnAbrir.Click+=(s,e)=>{
                var path=DataService.PastaRaizDetectada;
                if(!Directory.Exists(path)){
                    try{Directory.CreateDirectory(path);}catch{}
                }
                if(Directory.Exists(path))
                    System.Diagnostics.Process.Start("explorer.exe",path);
                else
                    MessageBox.Show("Pasta não encontrada:\n"+path,"Aviso",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            };
            pPath.Controls.AddRange(new Control[]{txtPath,btnAbrir});
            tbl.Controls.Add(pPath);

            // Resize: keep txtPath filling width
            pPath.Resize+=(s,e)=>{
                txtPath.Width=pPath.Width-btnAbrir.Width-6;
                btnAbrir.Left=txtPath.Width+2;
            };

            // ── Salvamento ───────────────────────────────────────────────────
            Sec("💾  Salvamento");
            chkAS=new CheckBox{Text="Ativar auto-salvar",AutoSize=true,Margin=new Padding(0,4,0,4),
                ForeColor=Fg,BackColor=Color.Transparent};
            chkCP=new CheckBox{Text="Criar pastas automaticamente ao salvar",AutoSize=true,Margin=new Padding(0,4,0,4),
                ForeColor=Fg,BackColor=Color.Transparent};
            tbl.Controls.Add(chkAS);tbl.Controls.Add(chkCP);

            // ── Dados ────────────────────────────────────────────────────────
            Sec("🗄  Dados");
            Desc("Dados armazenados em licitacoes_data.json (mesma pasta do .exe).");
            var bExp=new Button{Text="📤  Exportar JSON",AutoSize=true,FlatStyle=FlatStyle.Flat,
                BackColor=Mid,ForeColor=Fg,Margin=new Padding(0,4,0,4),Height=30};
            bExp.FlatAppearance.BorderColor=Brd;
            bExp.Click+=ExportarClick;
            tbl.Controls.Add(bExp);

            scroll.Controls.Add(tbl);

            var foot=new Panel{Dock=DockStyle.Bottom,Height=52,BackColor=Mid,Padding=new Padding(12,8,12,8)};
            foot.Controls.Add(new Panel{Dock=DockStyle.Top,Height=1,BackColor=Brd});
            var bSav=new Button{Text="💾  Salvar",Width=140,Height=34,Dock=DockStyle.Right,
                FlatStyle=FlatStyle.Flat,BackColor=Acc,ForeColor=Color.White,
                Font=new Font("Segoe UI",9,FontStyle.Bold)};
            bSav.FlatAppearance.BorderSize=0;bSav.Click+=SalvarClick;
            var bNo=new Button{Text="Cancelar",Width=100,Height=34,Dock=DockStyle.Right,
                FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(241,243,244),ForeColor=Mut};
            bNo.FlatAppearance.BorderColor=Brd;bNo.Click+=(s,e)=>Close();
            foot.Controls.AddRange(new Control[]{bSav,bNo});

            Controls.AddRange(new Control[]{hdr,scroll,foot});

            chkAS.Checked=DataService.Config.AutoSalvar;
            chkCP.Checked=DataService.Config.CriarPastasAutomatico;
        }

        // helper — retorna o right de um controle (Left+Width)
        static int txtp_right(Control c)=>c.Left+c.Width+2;

        void SalvarClick(object? s,EventArgs e){
            DataService.Config.AutoSalvar=chkAS.Checked;
            DataService.Config.CriarPastasAutomatico=chkCP.Checked;
            DataService.SalvarConfig();
            MessageBox.Show("Configurações salvas!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            Close();}

        void ExportarClick(object? s,EventArgs e){
            var sfd=new SaveFileDialog{Filter="JSON|*.json",FileName=$"licitacoes_{DateTime.Now:yyyyMMdd}.json"};
            if(sfd.ShowDialog()==DialogResult.OK){
                File.WriteAllText(sfd.FileName,DataService.ExportarJson());
                MessageBox.Show("Exportado!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);}}
    }
}
