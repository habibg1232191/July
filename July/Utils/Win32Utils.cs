using System;
using System.Runtime.InteropServices;

namespace July.Utils;

public class Win32Utils
{
    
    public static IntPtr FindDtwWindow()
    {
        IntPtr hWnd = FindWindow("Progman", "Program Manager");

        SendMessageTimeout(hWnd, 0x052C, (IntPtr)0, null, 0x0000, 1000, (IntPtr)null);
        IntPtr hWndWorkW = (IntPtr)null;
        do
        {
            hWndWorkW = FindWindowEx((IntPtr)null, hWndWorkW, "WorkerW", null);
            if ((IntPtr)null == hWndWorkW)
            {
                continue;
            }

            IntPtr hView = FindWindowEx(hWndWorkW, (IntPtr)null, "SHELLDLL_DefView", null);
            if ((IntPtr)null == hView)
            {
                continue;
            }

            IntPtr h = FindWindowEx((IntPtr)null, hWndWorkW, "WorkerW", null);
            while ((IntPtr)null != h)
            {
                SendMessage(h, 0x0010, (IntPtr)0, (IntPtr)0);
                h = FindWindowEx((IntPtr)null, hWndWorkW, "WorkerW", null);
            }

            break;
        } while (true);

        return hWnd;
    }

    [DllImport("User32.dll")]
    public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("User32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass,
        string lpszWindow);

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, string lParam, int fuFlags,
        int uTimeout, IntPtr lPdwResult);

    [DllImport("User32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    
    [DllImport("User32.dll", EntryPoint = "SetWindowLongW")]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    
    [DllImport("User32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, UIntPtr wParam, IntPtr lParam);

    [DllImport("User32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uiAction, int uiParam, ref RECT pvParam, int fWinIni);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [System.Runtime.InteropServices.DllImport("User32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam);

    public static Delegate SetWindowProc(IntPtr hWnd, WndProcDelegate newWndProc)
    {
        IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(newWndProc);
        IntPtr oldWndProcPtr = IntPtr.Size == 4 ? SetWindowLong32(hWnd, -4, newWndProcPtr) : SetWindowLong64(hWnd, -4, newWndProcPtr);
        return Marshal.GetDelegateForFunctionPointer(oldWndProcPtr, typeof(WndProcDelegate));
    }
}