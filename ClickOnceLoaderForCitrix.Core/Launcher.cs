using SHDocVw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ClickOnceLoaderForCitrix.Core
{
    enum Dialogtype
    {
        /// <summary>
        /// Dialog is op type install
        /// </summary>
        Install,
        /// <summary>
        /// Dialog is of type Error
        /// </summary>
        Error,
    }

    public static class Launcher
    {
        #region DLL Imports
        //Load methods in native DLL (User32.dll) - not a .net DLL

        /// <summary>
        /// Get window by handle
        /// </summary>
        [DllImport("User32.dll")]
        static extern Int32 FindWindow(String lpClassName, String lpWindowName);

        /// <summary>
        /// Send a key to application window
        /// </summary>
        [DllImport("user32.dll")]
        static extern Int32 PostMessage(int hWnd, int Msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Activate an application window.
        /// </summary>
        [DllImport("USER32.DLL")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Send message to window
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(int hWnd, int msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Used to minimize IExplore window
        /// </summary>
        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

        /// <summary>
        /// Get PID by process handle
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(int hWnd, out int processId);
        #endregion

        #region constants

        // keys https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx

        /// <summary>
        /// Close window
        /// </summary>
        private const int WM_CLOSE = 0x0010;

        private const int WM_QUIT = 0x0012;
        /// <summary>
        /// Minimize window
        /// </summary>
        private const int SW_MINIMIZE = 6;

        /// <summary>
        /// Key pressed
        /// </summary>
        private const int KEY_DOWN = 0x0100;

        /// <summary>
        /// S key
        /// </summary>
        private const int KEY_S = 0x53;
        /// <summary>
        /// I key
        /// </summary>
        private const int KEY_I = 0x49;
        /// <summary>
        /// J key
        /// </summary>
        private const int KEY_J = 0x4A;
        /// <summary>
        /// O key
        /// </summary>
        private const int KEY_O = 0x4F;
        #endregion

        #region fields
        //all values need to be true before program ends
        public static bool InstallReached = false;
        public static bool ErrorReached = false;
        public static int IECloseAttempt = 0;
        #endregion

        #region methods
        //not used at this time
        /// <summary>
        /// Checks for running instances of the iexplore process
        /// </summary>
        /// <returns></returns>
        public static bool CheckForInternetExplorer()
        {
            List<Process> p = new List<Process>();
            p = new List<Process>(Process.GetProcessesByName("iexplore"));

            if (p.Count > 0)
            {
                Debug.Write("iexplore active.");
                return true;
            }
            else
            {
                Debug.WriteLine("No iexplore active.");
                return false;
            }
        }

        /// <summary>
        /// Starts the Knowliah TCStarter Clickonce application.
        /// Install dialog, error and knowliah corner client launcher get automatic answers in the form of a keypress.
        /// </summary>
        /// <param name="path">URL of the clickonce location</param>
        /// <param name="installDialogHandle">Handle mentioned in the clickonce install dialog</param>
        /// <param name="errorDialogHandle">Handle mentioned in the error dialog</param>
        /// <param name="emptyInternetExplorerTitle">Handle mentioned for an empty Internet Explorer page. This is the result after navigation to clickonce location</param>
        /// <param name="timeout">Timeout for all dialogs, time starts at same moment for all dialogs</param>
        /// <returns></returns>
        public static async Task<bool> RunKnowliahCornerTCStarter(
            string path,
            string installDialogHandle,
            string errorDialogHandle,
            string emptyInternetExplorerTitle,
            int timeout)
        {
            //start a new instance of internet explorer
            InternetExplorer ie = new InternetExplorer();
            IWebBrowserApp wb = (IWebBrowserApp)ie;
            //navigate...
            wb.Navigate(path);
            //and minimize
            ShowWindow(wb.HWND, SW_MINIMIZE);
            //detect all subsequent dialogs
            DetectDialog(installDialogHandle, KEY_I, timeout, Dialogtype.Install);
            DetectDialog(errorDialogHandle, KEY_O, timeout, Dialogtype.Error);

            //wait for all dialogs to complete or timeout
            if (await AllDialogsReached())
            {
                await Task.Delay(1000);
                //try to close internet explorer window
                DetectIexploreWindow(emptyInternetExplorerTitle);
            }
            return true;
        }

        /// <summary>
        /// Check if all dialog tasks have been completed. Always await this method.
        /// </summary>
        /// <returns>true upon completion</returns>
        private static async Task<bool> AllDialogsReached()
        {
            while (ErrorReached == false || InstallReached == false)
            {
                Debug.WriteLine("Not all dialogs reached or timedout yet...");
                await Task.Delay(500);
            }
            return true;
        }

        /// <summary>
        /// Detect Internet explorer for the handle and close it
        /// </summary>
        /// <param name="titlebartext">Handle text for an empty IExplore page</param>
        public static void DetectIexploreWindow(string titlebartext)
        {
            int hwnd = 0;
            IntPtr hwndChild = IntPtr.Zero;

            //Get handle for the ClickOnce Window - name in titlebar
            hwnd = FindWindow(null, titlebartext);

            //Close window
            if (hwnd != 0)
            {
                IECloseAttempt++;
                SendMessage(hwnd, WM_CLOSE, 0, IntPtr.Zero);
                try
                {
                    //Max 10 retries to close Internet Explorer
                    if (IECloseAttempt < 10)
                    {
                        DetectIexploreWindow(titlebartext);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Detect the dialog and send corresponding key for dialog to continue
        /// </summary>
        /// <param name="dialogTitle">Handle of the dialog window</param>
        /// <param name="keysToSend">Key to be sent to dialog</param>
        /// <param name="timeout">MAximum time which may be performed for this method</param>
        /// <param name="dialog">Type of dialog this method is used for</param>
        private static async void DetectDialog(string dialogTitle, int keysToSend, int timeout, Dialogtype dialog)
        {
            try
            {
                DateTime endTime = DateTime.Now + TimeSpan.FromMilliseconds(timeout);
                bool dialogFound = false;
                //loop until timeout reached
                while (DateTime.Now < endTime)
                {
                    int hwnd = 0;
                    IntPtr hwndChild = IntPtr.Zero;

                    //Get handle for the ClickOnce Window - name in titlebar
                    hwnd = FindWindow(null, dialogTitle);

                    if (hwnd == 0)
                    {
                        Debug.WriteLine(dialogTitle + " not found.");
                        //stop if dialog was open previously
                        if (dialogFound)
                        {
                            break;
                        }
                    }
                    else
                    {
                        Debug.WriteLine(dialogTitle + " found.");
                        PostMessage(hwnd, KEY_DOWN, keysToSend, IntPtr.Zero);
                        dialogFound = true;
                    }
                    await Task.Delay(500);
                }

            }
            finally
            {
                //set bools to true depending on the dialog type this Method is valid for
                switch (dialog)
                {
                    case Dialogtype.Install:
                        InstallReached = true;
                        break;
                    case Dialogtype.Error:
                        ErrorReached = true;
                        break;
                    default:
                        break;
                }
            }

        }
        #endregion
    }
}
