using System;
using System.Collections.Generic;

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
        public struct ColorInfo
        {
            public string Nome { get; set; }
            public uint CorARGB { get; set; }
        }

        public static readonly Dictionary<StatusLicitacao, ColorInfo> Info = new()
        {
            [StatusLicitacao.Cadastrado]      = new ColorInfo { Nome = "Cadastrado", CorARGB = 0xFF3B82F6 },
            [StatusLicitacao.Ganho]           = new ColorInfo { Nome = "Ganho", CorARGB = 0xFF22C55E },
            [StatusLicitacao.Perdido]         = new ColorInfo { Nome = "Perdido", CorARGB = 0xFFEF4444 },
            [StatusLicitacao.Suspenso]        = new ColorInfo { Nome = "Suspenso", CorARGB = 0xFFF97316 },
            [StatusLicitacao.Cancelado]       = new ColorInfo { Nome = "Cancelado", CorARGB = 0xFF6B7280 },
            [StatusLicitacao.Desclassificado] = new ColorInfo { Nome = "Desclassificado", CorARGB = 0xFFDC2626 },
            [StatusLicitacao.Codificado]      = new ColorInfo { Nome = "Codificado", CorARGB = 0xFF8B5CF6 },
            [StatusLicitacao.Impugnado]       = new ColorInfo { Nome = "Impugnado", CorARGB = 0xFFD946EF },
            [StatusLicitacao.NaoCodificado]   = new ColorInfo { Nome = "Não Codificado", CorARGB = 0xFFFB7185 },
            [StatusLicitacao.EnviadoAmostras] = new ColorInfo { Nome = "Enviado Amostras", CorARGB = 0xFF06B6D4 },
            [StatusLicitacao.Ata]             = new ColorInfo { Nome = "Ata", CorARGB = 0xFFF97316 },
            [StatusLicitacao.Questionamento]  = new ColorInfo { Nome = "Questionamento", CorARGB = 0xFFEAB308 },
        };
        public static string GetNome(StatusLicitacao s)    => Info[s].Nome;
        public static uint GetCorARGB(StatusLicitacao s)   => Info[s].CorARGB;
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
        public string Numero="",Codigo="",Descricao="",Quantidade="",Unidade="",ValorTotal="";
        public decimal ValorUnitario = 0;
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
        public string          Orgao="",HoraDisputa="09:00",CodigoEffecti="",UASG="";
        public decimal         ValorEstimado = 0;
        public string          CodigoBB="";
        public string          PastaServidor="",PastaCliente="",Diario="";
        public TipoLicitacao   Tipo   = TipoLicitacao.PE;
        public StatusLicitacao Status = StatusLicitacao.Cadastrado;
        public DateTime        DataDisputa = DateTime.Now;
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

        public bool HasActiveFilter()
        {
            return !string.IsNullOrWhiteSpace(Busca) ||
                   !string.IsNullOrWhiteSpace(Estado) ||
                   !string.IsNullOrWhiteSpace(Municipio) ||
                   !string.IsNullOrWhiteSpace(Ano) ||
                   !string.IsNullOrWhiteSpace(FiltroItem) ||
                   Status != null ||
                   DataInicio != null ||
                   DataFim != null;
        }
    }
}
