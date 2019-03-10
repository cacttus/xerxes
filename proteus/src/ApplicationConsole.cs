using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proteus
{
    /// <summary>
    /// Allows WPF and WINFORMS appliations to have a console window to accept
    /// std input
    /// </summary>
    public class ApplicationConsole
    {
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern bool AllocConsole();
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool isConsoleSizeZero
        {
            get { return 0 == (Console.WindowHeight + Console.WindowWidth); }
        }
        public static bool IsOutputRedirected
        {
            get { return isConsoleSizeZero && !Console.KeyAvailable; }
        }
        public static bool IsInputRedirected
        {
            get { return isConsoleSizeZero && Console.KeyAvailable; }
        }

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void CreateConsole(bool blnHidden = true)
        {
            AllocConsole();
            if (blnHidden)
                HideSysConsole();
        }


        private static void ShowSysConsole()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }
        private static void HideSysConsole()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

    }
}
