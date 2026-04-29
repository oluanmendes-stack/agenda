using Microsoft.UI.Xaml;

namespace AgendaLicitacoes
{
    internal static class Program
    {
        [System.STAThread]
        static void Main(string[] args)
        {
            WinUIApplication.Current?.Close();
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
