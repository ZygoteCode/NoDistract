using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Reflection;

namespace NoDistract
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private static char[] _allowedCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        private static string[] _processNames = new string[]
        {
            //"Discord"
        };

        private static string[] _windowTitles = new string[]
        {
            "Lettore multimediale VLC",
            @"C:\Windows\System32\drivers\etc",
            "YouTube",
            "TikTok",
            "Facebook",
            "Instagram",
            //"Discord"
        };

        private static string[] readOnlyFiles = new string[]
        {
            @"C:\Windows\System32\drivers\etc\hosts"
        };

        private static string[] hiddenFiles = new string[]
        {
            @"C:\Windows\System32\drivers\etc\hosts"
        };

        public static void Main()
        {
            try
            {
                RegistryKey startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                startupKey.SetValue("NoDistract", Assembly.GetExecutingAssembly().Location);
            }
            catch
            {

            }

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0x00);

            while (true)
            {
                Thread.Sleep(200);

                foreach (string file in readOnlyFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.ReadOnly);
                        }
                    }
                    catch
                    {

                    }
                }

                foreach (string file in hiddenFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.SetAttributes(file, File.GetAttributes(file) | FileAttributes.Hidden);
                        }
                    }
                    catch
                    {

                    }
                }

                try
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced");

                    if (key != null)
                    {
                        key.SetValue("Hidden", 2);
                    }
                }
                catch
                {

                }

                try
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer");

                    if (key != null)
                    {
                        key.SetValue("Hidden", 2);
                    }
                }
                catch
                {

                }

                try
                {
                    Guid CLSID_ShellApplication = new Guid("13709620-C279-11CE-A49E-444553540000");
                    Type shellApplicationType = Type.GetTypeFromCLSID(CLSID_ShellApplication, true);
                    object shellApplication = Activator.CreateInstance(shellApplicationType);
                    object windows = shellApplicationType.InvokeMember("Windows", System.Reflection.BindingFlags.InvokeMethod, null, shellApplication, new object[] { });
                    Type windowsType = windows.GetType();
                    object count = windowsType.InvokeMember("Count", System.Reflection.BindingFlags.GetProperty, null, windows, null);

                    for (int i = 0; i < (int)count; i++)
                    {
                        try
                        {
                            object item = windowsType.InvokeMember("Item", System.Reflection.BindingFlags.InvokeMethod, null, windows, new object[] { i });
                            Type itemType = item.GetType();

                            string itemName = (string)itemType.InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, item, null);

                            if (itemName == "Windows Explorer")
                            {
                                itemType.InvokeMember("Refresh", System.Reflection.BindingFlags.InvokeMethod, null, item, null);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch
                {

                }

                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string processName = FilterString(process.ProcessName);

                        if (!IsStringEmpty(processName))
                        {
                            foreach (string otherProcessName in _processNames)
                            {
                                if (!IsStringEmpty(otherProcessName))
                                {
                                    string newOtherProcessName = FilterString(otherProcessName);

                                    if (!IsStringEmpty(newOtherProcessName))
                                    {
                                        if (processName.Contains(newOtherProcessName) || newOtherProcessName.Contains(processName))
                                        {
                                            process.Kill();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }

                    try
                    {
                        string windowTitle = FilterString(process.MainWindowTitle);

                        if (!IsStringEmpty(windowTitle))
                        {
                            foreach (string otherWindowTitle in _windowTitles)
                            {
                                if (!IsStringEmpty(otherWindowTitle))
                                {
                                    string newOtherWindowTitle = FilterString(otherWindowTitle);

                                    if (!IsStringEmpty(newOtherWindowTitle))
                                    {
                                        if (windowTitle.Contains(newOtherWindowTitle) || newOtherWindowTitle.Contains(windowTitle))
                                        {
                                            process.Kill();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }

                    try
                    {
                        foreach (ProcessThread thread in process.Threads)
                        {
                            try
                            {
                                EnumThreadWindows(thread.Id,
(hWnd, lParam) =>
{
    try
    {
        var intLength = GetWindowTextLength(hWnd) + 1;
        var stringBuilder = new StringBuilder(intLength);

        if (GetWindowText(hWnd, stringBuilder, intLength) > 0)
        {
            string windowTitle = stringBuilder.ToString();

            if (!IsStringEmpty(windowTitle))
            {
                string newWindowTitle = FilterString(windowTitle);

                if (!IsStringEmpty(newWindowTitle))
                {
                    foreach (string otherWindowTitle in _windowTitles)
                    {
                        if (!IsStringEmpty(otherWindowTitle))
                        {
                            string newOtherWindowTitle = FilterString(otherWindowTitle);

                            if (!IsStringEmpty(newOtherWindowTitle))
                            {
                                if (newWindowTitle.Contains(newOtherWindowTitle))
                                {
                                    process.Kill();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    catch
    {

    }

    return true;
},
IntPtr.Zero);
                            }
                            catch
                            {

                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        public static string FilterString(string input)
        {
            input = input.ToLower();
            string result = "";

            foreach (char c in input)
            {
                foreach (char a in _allowedCharacters)
                {
                    if (c == a)
                    {
                        result += c;
                        break;
                    }
                }
            }

            return result;
        }

        public static bool IsStringEmpty(string input)
        {
            if (input == null || input.Length == 0 || input[0] == '\0' || input == "" || String.IsNullOrEmpty(input))
            {
                return true;
            }

            return false;
        }
    }
}