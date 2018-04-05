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
        private string _applicationPath = "http://ingenium-20/KnowliahCorner/TCstarter.application?url=ingenium-20&TB=kennisDB&port=443";

        /// <summary>
        /// Timeout for dialog handling
        /// </summary>
        private int timeout = 5000;


        public MainWindow()
        {
            InitializeComponent();
            string[] Args = Environment.GetCommandLineArgs();
            if (Args.Length<2) { StopProgram(); }
            string _applicationPath = Args[1];
            RunLauncherAsync(timeout);
        }

        private void StopProgram()
        {
            System.Environment.Exit(0);
        }

        private async Task RunLauncherAsync(int waittime)
        {
            await Task.Delay(1000);

            await Launcher.RunKnowliahCornerTCStarter(
                _applicationPath,
                _toepassingInstallerenDialog,
                _errorClickonceDialog,
                _iexploreTitleBar,
                waittime);

            //Launcher.DetectIexploreWindow(_toepassingInstallerenDialog);

            await Task.Delay(2000);

            StopProgram();
        }
    }
}
