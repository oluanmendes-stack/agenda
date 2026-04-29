using Microsoft.UI.Xaml;

namespace AgendaLicitacoes
{
    internal static class Program
    {
        [System.STAThread]
        static void Main(string[] args)
        {
            // Configurar variável de ambiente necessária para Windows App SDK com PublishSingleFile
            Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
            
            WinUIApplication.Current?.Close();
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
