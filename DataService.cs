using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AgendaLicitacoes
{
    public static class DataService
    {
        static readonly string DataFile   = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"licitacoes_data.json");
        static readonly string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"licitacoes_config.json");

        public static Config          Config      { get; private set; } = new();
        public static List<Licitacao> Licitacoes  { get; private set; } = new();
        static int _next = 1;

        // ── Pasta raiz detectada automaticamente ─────────────────────────────
        // Procura "01 - EDITAIS E PROPOSTAS" a partir do diretório do exe,
        // subindo até 4 níveis caso o executável esteja em subpasta (ex: bin\Release).
        // Se não encontrar, aponta para uma pasta nova ao lado do exe.
        public static string PastaRaizDetectada { get; private set; } = "";

        static void DetectarPastaRaiz()
        {
            if (!string.IsNullOrEmpty(PastaRaizDetectada)) return; // já detectado

            const string TARGET = "01 - EDITAIS E PROPOSTAS";

            // Âncora: diretório do .exe em si (não do working dir)
            var exeDir = Path.GetDirectoryName(
                System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                ?? AppDomain.CurrentDomain.BaseDirectory)
                ?? AppDomain.CurrentDomain.BaseDirectory;

            var dir = exeDir.TrimEnd(Path.DirectorySeparatorChar);
            for (int i = 0; i <= 4; i++)
            {
                if (string.IsNullOrEmpty(dir)) break;
                var candidate = Path.Combine(dir, TARGET);
                if (Directory.Exists(candidate))
                {
                    PastaRaizDetectada = candidate;
                    return;
                }
                dir = Path.GetDirectoryName(dir) ?? "";
            }
            // Não encontrou — criará ao lado do próprio .exe
            PastaRaizDetectada = Path.Combine(exeDir, TARGET);
        }

        // ── Config ──────────────────────────────────────────────────────────
        public static void CarregarConfig()
        {
            DetectarPastaRaiz();
            if (!File.Exists(ConfigFile)) return;
            try { Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile)) ?? new(); }
            catch { Config = new(); }
        }
        public static void SalvarConfig()
            => File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Config, Formatting.Indented));

        // ── Dados ────────────────────────────────────────────────────────────
        public static void CarregarDados()
        {
            if (!File.Exists(DataFile)) return;
            try
            {
                var w = JsonConvert.DeserializeObject<Wrapper>(File.ReadAllText(DataFile));
                if (w != null){ Licitacoes=w.Licitacoes??new(); _next=w.ProximoId; }
            }
            catch { Licitacoes = new(); }
        }
        public static void SalvarDados()
            => File.WriteAllText(DataFile,
               JsonConvert.SerializeObject(new Wrapper{Licitacoes=Licitacoes,ProximoId=_next},Formatting.Indented));

        public static Licitacao Adicionar(Licitacao l)
        {
            l.Id=_next++; l.DataCriacao=l.DataAtualizacao=DateTime.Now;
            l.AddHistorico("Licitação criada");
            if (Config.CriarPastasAutomatico) CriarPastas(l);
            Licitacoes.Add(l);
            if (Config.AutoSalvar) SalvarDados();
            return l;
        }
        public static void Atualizar(Licitacao l)
        {
            l.DataAtualizacao=DateTime.Now;
            if (Config.CriarPastasAutomatico) CriarPastas(l);
            if (Config.AutoSalvar) SalvarDados();
        }
        public static void Remover(int id)
        {
            Licitacoes.RemoveAll(x=>x.Id==id);
            if (Config.AutoSalvar) SalvarDados();
        }

        // ── Filtro ───────────────────────────────────────────────────────────
        public static List<Licitacao> Filtrar(FiltroState f)
        {
            return Licitacoes.Where(l=>{
                if (!string.IsNullOrEmpty(f.Busca)){
                    var b=f.Busca.ToLower();
                    bool hit=l.Numero.ToLower().Contains(b)||l.Produtos.ToLower().Contains(b)
                        ||l.Municipio.ToLower().Contains(b)||l.Orgao.ToLower().Contains(b)
                        ||l.Portal.ToLower().Contains(b)||l.Estado.ToLower().Contains(b)
                        ||l.GetSigla().ToLower().Contains(b)
                        ||l.CodigoEffecti.ToLower().Contains(b)
                        ||l.UASG.ToLower().Contains(b)
                        ||l.CodigoBB.ToLower().Contains(b)
                        ||l.Itens.Exists(i=>i.Descricao.ToLower().Contains(b)||i.Codigo.ToLower().Contains(b));
                    if (!hit) return false;
                }
                if (!string.IsNullOrEmpty(f.Estado)   && !l.Estado.Equals(f.Estado,StringComparison.OrdinalIgnoreCase)) return false;
                if (!string.IsNullOrEmpty(f.Municipio) && !l.Municipio.ToLower().Contains(f.Municipio.ToLower())) return false;
                if (!string.IsNullOrEmpty(f.Ano)       && !l.Ano.Contains(f.Ano)) return false;
                if (f.Status.HasValue                  && l.Status!=f.Status.Value) return false;
                if (f.DataInicio.HasValue && l.DataDisputa.HasValue && l.DataDisputa.Value.Date<f.DataInicio.Value.Date) return false;
                if (f.DataFim.HasValue    && l.DataDisputa.HasValue && l.DataDisputa.Value.Date>f.DataFim.Value.Date)   return false;
                if (!string.IsNullOrEmpty(f.FiltroItem)){
                    var fi=f.FiltroItem.ToLower();
                    if (!l.Itens.Exists(i=>i.Descricao.ToLower().Contains(fi)||i.Codigo.ToLower().Contains(fi))) return false;
                }
                return true;
            }).OrderBy(l=>l.DataDisputa).ToList();
        }

        public static (int G,int P,int S,int N,int T) EstatisticasMes(List<Licitacao> lista)
        {
            var now=DateTime.Now;
            var m=lista.Where(l=>l.DataDisputa.HasValue&&l.DataDisputa.Value.Month==now.Month&&l.DataDisputa.Value.Year==now.Year).ToList();
            return (m.Count(l=>l.Status==StatusLicitacao.Ganho),
                    m.Count(l=>l.Status==StatusLicitacao.Perdido),
                    m.Count(l=>l.Status==StatusLicitacao.Suspenso),
                    m.Count(l=>l.Status==StatusLicitacao.NaoCodificado),
                    m.Count);
        }

        // ── Pastas ───────────────────────────────────────────────────────────
        public static void CriarPastas(Licitacao l)
        {
            // Garante que a detecção já ocorreu
            if (string.IsNullOrEmpty(PastaRaizDetectada)) DetectarPastaRaiz();

            var mun = Sanitize(l.Municipio);
            var rel = Path.Combine(l.Ano, l.Estado, mun, $"{l.GetSigla()} {l.Numero}");
            var sv  = Path.Combine(PastaRaizDetectada, rel);
            l.PastaServidor = sv;
            l.PastaCliente  = sv;   // mesmo caminho — sem distinção servidor/cliente
            try
            {
                Directory.CreateDirectory(Path.Combine(sv, "Docs"));
                Directory.CreateDirectory(Path.Combine(sv, "Proposta Inicial"));
                Directory.CreateDirectory(Path.Combine(sv, "Proposta Final"));
                Directory.CreateDirectory(Path.Combine(sv, "Empenhos"));
                Directory.CreateDirectory(Path.Combine(sv, "Edital"));
                Directory.CreateDirectory(Path.Combine(sv, "Termo de Referência"));
                Directory.CreateDirectory(Path.Combine(sv, "Resultado"));
                Directory.CreateDirectory(Path.Combine(sv, "ATA"));
                Directory.CreateDirectory(Path.Combine(sv, "Outros"));
                var dp = Path.Combine(sv, "Docs", "diario_do_processo.txt");
                if (!File.Exists(dp))
                    File.WriteAllText(dp,
                        $"DIÁRIO DO PROCESSO\nLicitação: {l.GetSigla()} {l.Numero} – {l.Produtos}\n" +
                        $"Órgão: {l.Orgao}\nMunicípio: {l.Municipio}/{l.Estado}\nPortal: {l.Portal}\n" +
                        $"Data Disputa: {l.DataDisputa:dd/MM/yyyy} {l.HoraDisputa}\n\n==============================\n\n");
            }
            catch { }
        }
        public static void SalvarDiarioArquivo(Licitacao l)
        {
            if (string.IsNullOrEmpty(l.PastaServidor)) return;
            try { File.WriteAllText(Path.Combine(l.PastaServidor,"Docs","diario_do_processo.txt"),l.Diario); }
            catch { }
        }
        static string Sanitize(string s){ foreach(var c in Path.GetInvalidFileNameChars()) s=s.Replace(c,'_'); return s.Trim(); }

        public static string ExportarJson()
            => JsonConvert.SerializeObject(new Wrapper{Licitacoes=Licitacoes,ProximoId=_next},Formatting.Indented);
    }

    public class Wrapper { public int ProximoId=1; public List<Licitacao> Licitacoes=new(); }
}
