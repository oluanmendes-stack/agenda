using Microsoft.UI.Xaml;
using Microsoft.UI;
using Windows.UI;

namespace AgendaLicitacoes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.ExtendsContentIntoTitleBar = true;
            m_window.SetTitleBar(null);
            m_window.Activate();
        }

        private Window m_window;
    }
}
