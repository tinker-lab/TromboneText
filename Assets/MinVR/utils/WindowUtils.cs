using System;
using System.Runtime.InteropServices;
using System.Text;


/**
 * Sets the position and title of graphics windows.  Currently only implemented on Windows platforms.
 * 
 * References:
 * http://unitydevelopers.blogspot.com/2015/04/set-size-and-position-of-windows.html
   http://answers.unity3d.com/questions/148723/how-can-i-change-the-title-of-the-standalone-playe.html
   http://matt.benic.us/post/88468666204/using-win32-api-to-get-specific-window-instance-in
   http://answers.unity3d.com/questions/936814/choose-screen-with-command-line-arguments.html
   https://gist.github.com/mattbenic/908483ad0bedbc62ab17
 */
public class WindowUtils {



#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private const string UnityWindowClassName = "UnityWndClass";

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int wFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    static extern bool SetWindowText(IntPtr hwnd, System.String lpString);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    enum WindowLongFlags : int {
        GWL_EXSTYLE = -20,
        GWLP_HINSTANCE = -6,
        GWLP_HWNDPARENT = -8,
        GWL_ID = -12,
        GWL_STYLE = -16,
        GWL_USERDATA = -21,
        GWL_WNDPROC = -4,
        DWLP_USER = 0x8,
        DWLP_MSGRESULT = 0x0,
        DWLP_DLGPROC = 0x4
    }

    [Flags]
    enum WindowStyles : uint {
        WS_OVERLAPPED = 0x00000000,
        WS_POPUP = 0x80000000,
        WS_CHILD = 0x40000000,
        WS_MINIMIZE = 0x20000000,
        WS_VISIBLE = 0x10000000,
        WS_DISABLED = 0x08000000,
        WS_CLIPSIBLINGS = 0x04000000,
        WS_CLIPCHILDREN = 0x02000000,
        WS_MAXIMIZE = 0x01000000,
        WS_BORDER = 0x00800000,
        WS_DLGFRAME = 0x00400000,
        WS_VSCROLL = 0x00200000,
        WS_HSCROLL = 0x00100000,
        WS_SYSMENU = 0x00080000,
        WS_THICKFRAME = 0x00040000,
        WS_GROUP = 0x00020000,
        WS_TABSTOP = 0x00010000,

        WS_MINIMIZEBOX = 0x00020000,
        WS_MAXIMIZEBOX = 0x00010000,

        WS_CAPTION = WS_BORDER | WS_DLGFRAME,
        WS_TILED = WS_OVERLAPPED,
        WS_ICONIC = WS_MINIMIZE,
        WS_SIZEBOX = WS_THICKFRAME,
        WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_CHILDWINDOW = WS_CHILD,

        //Extended Window Styles

        WS_EX_DLGMODALFRAME = 0x00000001,
        WS_EX_NOPARENTNOTIFY = 0x00000004,
        WS_EX_TOPMOST = 0x00000008,
        WS_EX_ACCEPTFILES = 0x00000010,
        WS_EX_TRANSPARENT = 0x00000020,

        //#if(WINVER >= 0x0400)

        WS_EX_MDICHILD = 0x00000040,
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_WINDOWEDGE = 0x00000100,
        WS_EX_CLIENTEDGE = 0x00000200,
        WS_EX_CONTEXTHELP = 0x00000400,

        WS_EX_RIGHT = 0x00001000,
        WS_EX_LEFT = 0x00000000,
        WS_EX_RTLREADING = 0x00002000,
        WS_EX_LTRREADING = 0x00000000,
        WS_EX_LEFTSCROLLBAR = 0x00004000,
        WS_EX_RIGHTSCROLLBAR = 0x00000000,

        WS_EX_CONTROLPARENT = 0x00010000,
        WS_EX_STATICEDGE = 0x00020000,
        WS_EX_APPWINDOW = 0x00040000,

        WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
        WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
        //#endif /* WINVER >= 0x0400 */

        //#if(WIN32WINNT >= 0x0500)

        WS_EX_LAYERED = 0x00080000,
        //#endif /* WIN32WINNT >= 0x0500 */

        //#if(WINVER >= 0x0500)

        WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
        WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
                                      //#endif /* WINVER >= 0x0500 */

        //#if(WIN32WINNT >= 0x0500)

        WS_EX_COMPOSITED = 0x02000000,
        WS_EX_NOACTIVATE = 0x08000000
        //#endif /* WIN32WINNT >= 0x0500 */

    }


    /// <summary>
    /// Gets the current window handle.
    /// </summary>
    /// <returns>The window handle.</returns>
    private static IntPtr GetWindowHandle() {
        IntPtr windowHandle = IntPtr.Zero;

        // enumerates all nonchild windows associated with the current thread.
        uint threadId = GetCurrentThreadId();
        EnumThreadWindows(threadId, (hWnd, lParam) => {
            // retrieves the name of the class to which the specified window belongs.
            var classText = new StringBuilder(UnityWindowClassName.Length + 1);
            GetClassName(hWnd, classText, classText.Capacity);
            // compare to see if this is what we are looking for
            if (classText.ToString() == UnityWindowClassName) {
                windowHandle = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);

        //
        return windowHandle;
    }
#endif


    public static void RemoveBorder() {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // 1) get the current window handle.
        IntPtr windowHandle = WindowUtils.GetWindowHandle();

        // 2) clear out the window style flags by setting style to visible only
        if (windowHandle != IntPtr.Zero) {
            SetWindowLong(windowHandle, (int)WindowLongFlags.GWL_STYLE, (uint)WindowStyles.WS_VISIBLE);
        }
#endif
    }


    public static void SetPositionAndSize(int x, int y, int resX, int resY) {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // 1) get the current window handle.
        IntPtr windowHandle = WindowUtils.GetWindowHandle();

        // 2) offset and position the window, if we got something.
        if (windowHandle != IntPtr.Zero) {
            SetWindowPos(windowHandle, 0 /*HWND_TOP*/, x, y, resX, resY, 0 /*No extra flags*/);
        }
#endif
    }



    public static void SetWindowTitle(string title) {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // 1) get the current window handle.
        IntPtr windowHandle = WindowUtils.GetWindowHandle();
        // 2) set the window title, if we got something.
        if (windowHandle != IntPtr.Zero) {
            SetWindowText(windowHandle, title);
        }
#endif
    }
}
