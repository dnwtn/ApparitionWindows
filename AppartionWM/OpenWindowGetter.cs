using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using HWND = System.IntPtr;

namespace WindowManager
{
    /// <summary>Contains functionality to get all the open windows.</summary>
    internal class OpenWindowGetter
    {
        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<string, HWND> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<string, HWND> windows = new Dictionary<string, HWND>();

            EnumWindows(delegate(HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                if(builder.ToString() != "Start")
                    windows[builder.ToString()] = hWnd;

                return true;

            }, 0);

            return windows;
        }
    }
}
