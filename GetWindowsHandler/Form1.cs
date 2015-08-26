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
        int hwnd;   //窗口句柄
        int process;//进程句柄
        int pointer;
        private const uint LVM_FIRST = 0x1000;
        private const uint LVM_GETHEADER = LVM_FIRST + 31;
        private const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;//获取列表行数
        private const uint LVM_GETITEMTEXT = LVM_FIRST + 45;//获取列表内的内容
        private const uint LVM_GETITEMW = LVM_FIRST + 75;

        private const uint HDM_GETITEMCOUNT = 0x1200;//获取列表列数

        private const uint PROCESS_VM_OPERATION = 0x0008;//允许函数VirtualProtectEx使用此句柄修改进程的虚拟内存
        private const uint PROCESS_VM_READ = 0x0010;//允许函数访问权限
        private const uint PROCESS_VM_WRITE = 0x0020;//允许函数写入权限

        private const uint MEM_COMMIT = 0x1000;//为特定的页面区域分配内存中或磁盘的页面文件中的物理存储
        private const uint MEM_RELEASE = 0x8000;
        private const uint MEM_RESERVE = 0x2000;//保留进程的虚拟地址空间,而不分配任何物理存储

        private const uint PAGE_READWRITE = 4;        

        [DllImport("user32.dll")]//查找窗口
        private static extern int FindWindow(
                                            string strClassName,    //窗口类名
                                            string strWindowName    //窗口标题
        );

        [DllImport("user32.dll")]//在窗口列表中寻找与指定条件相符的第一个子窗口
        private static extern int FindWindowEx(
                                              int hwndParent, // handle to parent window
　　                                          int hwndChildAfter, // handle to child window
                                              string className, //窗口类名            
                                              string windowName // 窗口标题
        );
        [DllImport("user32.DLL")]
        private static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll")]//找出某个窗口的创建者(线程或进程),返回创建者的标志符
        private static extern int GetWindowThreadProcessId(int hwnd, out int processId);
        [DllImport("kernel32.dll")]//打开一个已存在的进程对象,并返回进程的句柄
        private static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll")]//为指定的进程分配内存地址:成功则返回分配内存的首地址
        private static extern int VirtualAllocEx(int hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll")]//从指定内存中读取字节集数据
        private static extern bool ReadProcessMemory(
                                            int hProcess, //被读取者的进程句柄
                                            int lpBaseAddress,//开始读取的内存地址
                                            IntPtr lpBuffer, //数据存储变量
                                            int nSize, //要写入多少字节
                                            ref uint vNumberOfBytesRead//读取长度
        );
        [DllImport("kernel32.dll")]//将数据写入内存中
        private static extern bool WriteProcessMemory(
                                            int hProcess,//由OpenProcess返回的进程句柄
                                            int lpBaseAddress, //要写的内存首地址,再写入之前,此函数将先检查目标地址是否可用,并能容纳待写入的数据
                                            IntPtr lpBuffer, //指向要写的数据的指针
                                            int nSize, //要写入的字节数
                                            ref uint vNumberOfBytesRead
        );
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(int handle);
        [DllImport("kernel32.dll")]//在其它进程中释放申请的虚拟内存空间
        private static extern bool VirtualFreeEx(
                                    int hProcess,//目标进程的句柄,该句柄必须拥有PROCESS_VM_OPERATION的权限
                                    int lpAddress,//指向要释放的虚拟内存空间首地址的指针
                                    uint dwSize,
                                    uint dwFreeType//释放类型
        );
        [DllImport("User32.dll")]
        static extern int GetWindowText(int handle, StringBuilder text, int MaxLen);

        #endregion 

        getContent content = new getContent();

        public Form1()
        {
            InitializeComponent();
            content.RegisterControlforMessages();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int x;

            //指定窗口的名称
            x = FindWindow(null, "Member's Information and Result.");
            if (x != 0)
            {
                MessageBox.Show("找到主窗口");
            }

            //借助spy++工具，可以查找到一些类名对应的窗口
            hwnd = FindWindowEx(x, 0, "TPanel",null);//进程界面窗口的句柄,通过SPY获取
            if (hwnd != 0)
            {
                MessageBox.Show("找到TPanel\n" + Convert.ToString(hwnd));
            }

            hwnd = FindWindowEx(x, 0, "TComboBox", null);//进程界面窗口的句柄,通过SPY获取
            if (hwnd != 0)
            {
                MessageBox.Show("找到TComboBox\n" + Convert.ToString(hwnd));
            }
            //通过句柄来获取控件内容
            MessageBox.Show(content.GetControlText(hwnd));

            hwnd = FindWindowEx(hwnd, 0, "Edit", null);//进程界面窗口的句柄,通过SPY获取
            if (hwnd != 0)
            {
                MessageBox.Show("找到Edit\n" + Convert.ToString(hwnd));
            }

            MessageBox.Show(content.GetControlText(hwnd));
        }
    }


    public class getContent
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)] //
        public static extern bool SendMessage(int hWnd, uint Msg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam,int lparam);

        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;

        public void RegisterControlforMessages()
        {
            RegisterWindowMessage("WM_GETTEXT");
        }
        public string GetControlText(int hWnd)
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
