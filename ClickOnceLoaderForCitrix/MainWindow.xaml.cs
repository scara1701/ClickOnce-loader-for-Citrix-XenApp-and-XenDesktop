using ClickOnceLoaderForCitrix.Core;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ClickOnceLoaderForCitrix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Text in handle for Internet Explorer - empty page
        /// </summary>
        private string _iexploreTitleBar = "Lege pagina - Internet Explorer";
        /// <summary>
        /// <summary>
        /// Text in handle for error in ClickOnce dialog
        /// </summary>
        private string _errorClickonceDialog = "Kan de toepassing niet starten";
        /// <summary>
        /// Text in handle for install in ClickOnce dialog
        /// </summary>
        private string _toepassingInstallerenDialog = "Toepassing installeren - beveiligingswaarschuwing";

        /// <summary>
        /// Path of ClickOnce application (*.application), needs to be loaded in IEXplore
        /// </summary>
        private string _applicationPath;

        /// <summary>
        /// Timeout for dialog handling
        /// </summary>
        private int timeout = 10000;


        public MainWindow()
        {
            InitializeComponent();
            string[] Args = Environment.GetCommandLineArgs();
            if (Args.Length < 2) { Launcher.StopProgram(); }
            _applicationPath = Args[1];
            RunLauncherAsync(timeout);
        }

        private async Task RunLauncherAsync(int waittime)
        {
           Launcher.RunKnowliahCornerTCStarter(
                _applicationPath,
                _toepassingInstallerenDialog,
                _errorClickonceDialog,
                _iexploreTitleBar,
                waittime);
        }
    }
}
