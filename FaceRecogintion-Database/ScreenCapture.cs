using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FaceRecognition_Database
{
    public class ScreenCapture
    {
	    public static Process[] ProcessList = Process.GetProcesses();
        protected static IntPtr m_HBitmap;
        #region Public Class Functions

        public static List<string> GetWindowsTitles()
        {
	        List<string> result = new List<string>();
	        foreach (Process proc in ProcessList)
	        {
		        if(proc.MainWindowTitle != "" && proc.MainWindowHandle.ToInt32() != 0)
                    result.Add(proc.MainWindowTitle);
	        }
	        return result;
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr FindWindow(string caption)
        {
	        return FindWindow(default(string), caption);
        }

        public static Bitmap GetWindowCaptureAsBitmap(string windowTitle)
        {
	        SIZE size;
            IntPtr hWnd = IntPtr.Zero;
            PlatformInvokeUSER32.RECT rc = new PlatformInvokeUSER32.RECT();
	        Bitmap bitmap = new Bitmap(1,1);
            try
            {
		        hWnd = FindWindow(windowTitle);
		        PlatformInvokeUSER32.ShowWindow(hWnd, 1);
                PlatformInvokeUSER32.SetForegroundWindow(hWnd);
		        Thread.Sleep(100);
                PlatformInvokeUSER32.GetWindowRect(hWnd, out rc);
		        size.cx = rc.Right - rc.Left;
		        size.cy = rc.Bottom - rc.Top;
                if (hWnd.ToInt32() == 0 || size.cx ==0 || size.cy == 0) 
	                throw new Exception("Process has no window!");
                // create a bitmap from the visible clipping bounds of 
                //the graphics object from the window
                bitmap = new Bitmap(size.cx, size.cy);

                // create a graphics object from the bitmap
                Graphics gfxBitmap = Graphics.FromImage(bitmap);

                // get a device context for the bitmap
                IntPtr hdcBitmap = gfxBitmap.GetHdc();

                // get a device context for the window
                IntPtr hdcWindow = PlatformInvokeUSER32.GetWindowDC(hWnd.ToInt32());

                // bitblt the window to the bitmap
                PlatformInvokeGDI32.BitBlt(hdcBitmap, 0, 0, size.cx, size.cy,
	                hdcWindow, 0, 0, (int)PlatformInvokeGDI32.SRCCOPY);

                // release the bitmap's device context
                gfxBitmap.ReleaseHdc(hdcBitmap);


                PlatformInvokeUSER32.ReleaseDC(hWnd, hdcWindow);

                // dispose of the bitmap's graphics object
                gfxBitmap.Dispose();
                PlatformInvokeUSER32.SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            }
            catch (Exception ex)
            {
	            MessageBox.Show(ex.Message);
            }
            // return the bitmap of the window
	        return bitmap;
        }
        public static Bitmap GetDesktopImage()
        {
            SIZE size;

            IntPtr hBitmap;

            IntPtr hDC = PlatformInvokeUSER32.GetDC
                          (PlatformInvokeUSER32.GetDesktopWindow());

            IntPtr hMemDC = PlatformInvokeGDI32.CreateCompatibleDC(hDC);

            size.cx = PlatformInvokeUSER32.GetSystemMetrics
                      (PlatformInvokeUSER32.SM_CXSCREEN);

            size.cy = PlatformInvokeUSER32.GetSystemMetrics
                      (PlatformInvokeUSER32.SM_CYSCREEN);

            hBitmap = PlatformInvokeGDI32.CreateCompatibleBitmap
                        (hDC, size.cx, size.cy);

            if (hBitmap != IntPtr.Zero)
            {
                //Here we select the compatible bitmap in the memeory device
                //context and keep the refrence to the old bitmap.
                IntPtr hOld = (IntPtr)PlatformInvokeGDI32.SelectObject
                                       (hMemDC, hBitmap);
                //We copy the Bitmap to the memory device context.
                PlatformInvokeGDI32.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC,
                                           0, 0, PlatformInvokeGDI32.SRCCOPY);
                //We select the old bitmap back to the memory device context.
                PlatformInvokeGDI32.SelectObject(hMemDC, hOld);
                //We delete the memory device context.
                PlatformInvokeGDI32.DeleteDC(hMemDC);
                //We release the screen device context.
                PlatformInvokeUSER32.ReleaseDC(PlatformInvokeUSER32.
                                               GetDesktopWindow(), hDC);
                //Image is created by Image bitmap handle and stored in
                //local variable.
                Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                //Release the memory to avoid memory leaks.
                PlatformInvokeGDI32.DeleteObject(hBitmap);
                
                //This statement runs the garbage collector manually.
                GC.Collect();
                //Return the bitmap 
                return bmp;
            }
            //If hBitmap is null, retun null.
            return null;
        }

        public static Rect GetWindowRectangle()
        {
            //Process proc = Process.GetCurrentProcess();
            //proc.WaitForInputIdle();
            //IntPtr ptr = proc.MainWindowHandle;
            Rect rect = new Rect(new System.Windows.Point(Application.Current.MainWindow.Left, Application.Current.MainWindow.Top)
                , new System.Windows.Size(Application.Current.MainWindow.ActualWidth, Application.Current.MainWindow.ActualHeight));
            //PlatformInvokeUSER32.GetWindowRect(ptr, ref rect);
            //return new Rectangle (new System.Drawing.Point((int)rect.Location.X, (int)rect.Location.Y), 
            // new System.Drawing.Size((int)rect.Size.Height, (int)rect.Size.Height));
            return rect;
        }
        #endregion
    }

    //This structure shall be used to keep the size of the screen.
    public struct SIZE
    {
        public int cx;
        public int cy;
    }
    public class PlatformInvokeGDI32
    {
        #region Class Variables
        public const int SRCCOPY = 13369376;
        #endregion
        #region Class Functions<br>
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest,
            int yDest, int wDest, int hDest, IntPtr hdcSource,
            int xSrc, int ySrc, int RasterOp);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,
            int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        #endregion
    }

    /// <summary>
    /// This class shall keep the User32 APIs used in our program.
    /// </summary>
    public class PlatformInvokeUSER32
    {
	    [StructLayout(LayoutKind.Sequential)]
	    public struct RECT
	    {
		    public int Left;        // x position of upper-left corner
		    public int Top;         // y position of upper-left corner
		    public int Right;       // x position of lower-right corner
		    public int Bottom;      // y position of lower-right corner
	    }
        #region Class Variables
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        #endregion

        #region Class Functions
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hwnd);

        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion
    }
}
