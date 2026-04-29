using System;
using System.Collections.Generic;
using System.Drawing;

namespace AgendaLicitacoes
{
    public enum StatusLicitacao
    {
        Cadastrado = 0,
        Ganho,
        Perdido,
        Suspenso,
        Cancelado,
        Desclassificado,
        Codificado,
        Impugnado,
        NaoCodificado,
        EnviadoAmostras,
        Ata,
        Questionamento
    }

    public enum TipoLicitacao { PE=0, DL, CH, PR, CC, TP, CV }

    public static class StatusInfo
    {
        public static readonly Dictionary<StatusLicitacao,(string Nome,Color Cor,Color CorTexto)> Info = new()
        {
            [StatusLicitacao.Cadastrado]      = ("Cadastrado",       Color.FromArgb( 59,130,246), Color.White),
            [StatusLicitacao.Ganho]           = ("Ganho",            Color.FromArgb( 34,197, 94), Color.White),
            [StatusLicitacao.Perdido]         = ("Perdido",          Color.FromArgb(239, 68, 68), Color.White),
            [StatusLicitacao.Suspenso]        = ("Suspenso",         Color.FromArgb(249, 115, 22), Color.White),
            [StatusLicitacao.Cancelado]       = ("Cancelado",        Color.FromArgb(107,114,128), Color.White),
            [StatusLicitacao.Desclassificado]  = ("Desclassificado",    Color.FromArgb(220, 38, 38), Color.White),
            [StatusLicitacao.Codificado]      = ("Codificado",       Color.FromArgb(139, 92,246), Color.White),
            [StatusLicitacao.Impugnado]       = ("Impugnado",        Color.FromArgb(217, 70,239), Color.White),
            [StatusLicitacao.NaoCodificado]   = ("Não Codificado",   Color.FromArgb(251,113,133), Color.White),
            [StatusLicitacao.EnviadoAmostras] = ("Enviado Amostras", Color.FromArgb(  6,182,212), Color.White),
            [StatusLicitacao.Ata]             = ("Ata",              Color.FromArgb(249,115, 22), Color.White),
            [StatusLicitacao.Questionamento]  = ("Questionamento",   Color.FromArgb(234,179,  8), Color.White),
        };
        public static string GetNome(StatusLicitacao s)    => Info[s].Nome;
        public static Color  GetCor(StatusLicitacao s)     => Info[s].Cor;
        public static Color  GetCorTexto(StatusLicitacao s)=> Info[s].CorTexto;
    }

    public static class TipoInfo
    {
        public static readonly Dictionary<TipoLicitacao,string> Nomes = new()
        {
            [TipoLicitacao.PE]="Pregão Eletrônico",[TipoLicitacao.DL]="Dispensa Eletrônica",
            [TipoLicitacao.CH]="Chamamento Público",[TipoLicitacao.PR]="Pregão Presencial",
            [TipoLicitacao.CC]="Concorrência",[TipoLicitacao.TP]="Tomada de Preços",[TipoLicitacao.CV]="Convite",
        };
        public static string GetSigla(TipoLicitacao t)  => t.ToString();
        public static string GetNome(TipoLicitacao t)   => Nomes[t];
        public static string GetDisplay(TipoLicitacao t)=> $"{t} – {Nomes[t]}";
    }

    public class Item
    {
        public string Numero="",Codigo="",Descricao="",Quantidade="",Unidade="",ValorUnitario="",ValorTotal="";
        // Status do item: null=não definido, true=ganho, false=perdido
        public bool? Ganho = null;
    }

    public class Anexo
    {
        public string   Nome="",Caminho="",Tipo="";
        public DateTime DataAdd=DateTime.Now;
    }

    public class HistoricoItem
    {
        public DateTime DataHora=DateTime.Now;
        public string   Descricao="",Usuario="Sistema";
    }

    public class Licitacao
    {
        public int    Id;
        // obrigatórios
        public string Ano=DateTime.Now.Year.ToString(),Estado="",Municipio="",Numero="",Portal="",Produtos="";
        // opcionais
        public string          Orgao="",HoraDisputa="09:00",CodigoEffecti="",UASG="",ValorEstimado="";
        public string          CodigoBB="";
        public string          PastaServidor="",PastaCliente="",Diario="";
        public TipoLicitacao   Tipo   = TipoLicitacao.PE;
        public StatusLicitacao Status = StatusLicitacao.Cadastrado;
        public DateTime?       DataDisputa;
        public DateTime?       DataInicioAta;
        public DateTime?       DataFimAta;
        public List<Item>          Itens    = new();
        public List<Anexo>         Anexos   = new();
        public List<HistoricoItem> Historico= new();
        public DateTime DataCriacao=DateTime.Now,DataAtualizacao=DateTime.Now;

        public string GetTitulo()=>$"{TipoInfo.GetSigla(Tipo)} Nº {Numero} – {(Produtos.Length>40?Produtos[..40]+"…":Produtos)} ({Portal})";
        public string GetSigla()=>TipoInfo.GetSigla(Tipo);
        public void AddHistorico(string d){Historico.Add(new HistoricoItem{Descricao=d});DataAtualizacao=DateTime.Now;}
    }

    public class Config
    {
        public string CaminhoServidor="",CaminhoCliente="";
        public bool AutoSalvar=true,CriarPastasAutomatico=true;
    }

    public class FiltroState
    {
        public string Busca="",Estado="",Municipio="",Ano="",FiltroItem="";
        public StatusLicitacao? Status=null;
        public DateTime? DataInicio=null,DataFim=null;
    }
}
