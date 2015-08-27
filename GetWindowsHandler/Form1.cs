using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace GetWindowsHandler
{
    public partial class Form1 : Form
    {

        #region 变量声明及方法定义
        IntPtr hwnd;   //窗口句柄

        [DllImport("user32.DLL")]
        private static extern IntPtr SendMessage(int hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        static extern IntPtr GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);

        public delegate bool EnumChildWindow(IntPtr WindowHandle, string name);

        [DllImport("User32.dll")]
        public static extern int EnumChildWindows(IntPtr WinHandle, EnumChildWindow ecw, string name);

        #endregion 

        public delegate bool EnumChildWindowsProc(IntPtr hwnd, uint lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #region Dll Import 

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")] //找子窗体
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")] //用于发送信息给窗体
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "EnumChildWindows")]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildWindowsProc lpEnumFunc, int lParam);

        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);
        #endregion
        private void SearchWindow()
        {
            #region
            string lpszParentClass = "TMain"; //整个窗口的类名 
            string lpszParentWindow = "Member's Information and Result."; //窗口标题 
            IntPtr hWndParent = new IntPtr(0);
            IntPtr EdithWnd = new IntPtr(0);

            //查到窗体，得到整个窗体 
            hWndParent = FindWindow(lpszParentClass, lpszParentWindow);

            EnumChildWindowsProc myEnumChild = new EnumChildWindowsProc(EumWinChiPro);
            try
            {
                EnumChildWindows(hWndParent, myEnumChild, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.Source + "\r\n\r\n" + ex.StackTrace.ToString());
            }
        }
        #endregion

        getContent content = new getContent();

        public static int count;
        public bool EumWinChiPro(IntPtr hWnd, uint lParam)
        {
            StringBuilder s = new StringBuilder(50);
            GetClassName(hWnd, s, 50);
            MessageBox.Show(s.ToString() + '\n' + content.GetControlText(hWnd));
            count++;
            return true;
        }

        public static string all = null;
        public static int num = 0;

        public Form1()
        {
            InitializeComponent();
            content.RegisterControlforMessages();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            content.RegisterControlforMessages();
            SearchWindow();
            this.button1.Text = count.ToString();
        }

    }


    public class getContent
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)] //
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam,int lparam);

        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        public void RegisterControlforMessages()
        {
            RegisterWindowMessage("WM_GETTEXT");
        }
        public string GetControlText(IntPtr hWnd)
        {

            StringBuilder title = new StringBuilder();

            // Get the size of the string required to hold the window title. 
            Int32 size = SendMessage((int)hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();

            // If the return is 0, there is no title. 
            if (size > 0)
            {
                title = new StringBuilder(size + 1);

                SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);                
            }
            return title.ToString();
        }

    }



}
