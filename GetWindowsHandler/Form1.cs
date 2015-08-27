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
        
        

        #region Dll Import 
        [DllImport("user32.DLL")]
        private static extern IntPtr SendMessage(int hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        static extern IntPtr GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);

        public delegate bool EnumChildWindow(IntPtr WindowHandle, string name);

        [DllImport("User32.dll")]
        public static extern int EnumChildWindows(IntPtr WinHandle, EnumChildWindow ecw, string name);

        public delegate bool EnumChildWindowsProc(IntPtr hwnd, uint lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);



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
        
        getContent content = new getContent();
        public static int count = 0;
        private static string [] userInformationFromPanel = new string[19];

        public Form1()
        {
            InitializeComponent();
            content.RegisterControlforMessages();
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            //开始遍历
            SearchWindow();
            this.button1.Text = count.ToString();
        }

        private void SearchWindow()
        {
            string lpszParentClass = "TMain"; //整个窗口的类名 
            string lpszParentWindow = "Member's Information and Result."; //窗口标题 
            IntPtr hWndParent = new IntPtr(0);
            IntPtr EdithWnd = new IntPtr(0);

            //查到窗体，得到整个窗体 
            hWndParent = FindWindow(lpszParentClass, lpszParentWindow);

            //实例化
            EnumChildWindowsProc myEnumChild = new EnumChildWindowsProc(EumWinChiPro);
            try
            {
                //遍历子控件
                EnumChildWindows(hWndParent, myEnumChild, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.Source + "\r\n\r\n" + ex.StackTrace.ToString());
            }
        }

        //遍历每个子空间的具体方法
        public bool EumWinChiPro(IntPtr hWnd, uint lParam)
        {
            StringBuilder s = new StringBuilder(64);
            //根据子窗口句柄获取窗口类名
            GetClassName(hWnd, s, 64);
            //根据子窗口句柄获取其内容并弹窗显示
            MessageBox.Show(count.ToString() + '\n' + content.GetControlText(hWnd));
            //把获取到的信息放到指定字符串
            if (count > 5 && count < 10)
            {
                userInformationFromPanel[count] = content.GetControlText(hWnd);
            }
            count++;            
            return true;
        }

    }


    //根据窗体句柄获取其内容的类
    public class getContent
    {
        #region DLL import
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)] //
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam,int lparam);
        #endregion

        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        //注册消息
        public void RegisterControlforMessages()
        {
            RegisterWindowMessage("WM_GETTEXT");
        }

        //根据句柄获取其内容
        public string GetControlText(IntPtr hWnd)
        {

            StringBuilder title = new StringBuilder();

            // Get the size of the string required to hold the window title. 
            Int32 size = SendMessage((int)hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();

            // If the return is 0, there is no title. 
            if (size > 0)
            {
                title = new StringBuilder(size + 1);
                //通过消息来设置或得到属性（内容、事件等）
                SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);                
            }
            return title.ToString();
        }

    }



}
