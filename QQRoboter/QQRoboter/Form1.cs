using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.IO;
using PubOp;
using System.Collections.Specialized;
using System.Diagnostics;
namespace QQRoboter
{
    public partial class Form1 : Form
    {
        string Md = "";
        int p = 0;
        int hour = 0;
        int shijian = 120;
        string value = "";
        bool iscai = false;//是否在猜拳中
        bool iscaimi = false;
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow")]

        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //把窗口放到最前面
        const int WM_SETTEXT = 0x000C;//写字符串
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("USER32.DLL")]

        public static extern bool SetForegroundWindow(IntPtr hWnd); //模拟键盘事件

        [DllImport("User32.dll")]

        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        //释放按键的常量

        private const int KEYEVENTF_KEYUP = 2;
        const int WM_PASTE = 0x302;
        const int WM_CUT = 0x300;
        const int WM_COPY = 0x301;
        //发送消息

        [DllImport("user32.dll")]

        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]//获取窗口大小

        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]//获取窗口坐标

        public struct RECT
        {

            public int Left; //最左坐标

            public int Top; //最上坐标

            public int Right; //最右坐标

            public int Bottom; //最下坐标

        }


        [DllImport("user32.dll")]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #region SendMessage 参数

        private const int WM_KEYDOWN = 0X100;

        private const int WM_KEYUP = 0X101;

        private const int WM_SYSCHAR = 0X106;

        private const int WM_SYSKEYUP = 0X105;

        private const int WM_SYSKEYDOWN = 0X104;

        private const int WM_CHAR = 0X102;

        #endregion

        //鼠标事件
        long period = 0;

        [DllImport("user32")]

        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        public void InputStr(IntPtr k, string Input)
        {

            //不能发送汉字，只能发送键盘上有的内容 也可以模拟shift+！等 
            byte[] ch = (System.Text.Encoding.GetEncoding("gb2312").GetBytes(Input));
            for (int i = 0; i < ch.Length; i++)
            {

                SendMessage(k, WM_CHAR, ch[i], 0);

            }

        }
        public abstract partial class Win32Native
        {
            private static volatile List<int> hWndCollect;

            private static bool ScanSessionWindow(int hwnd, int lParam)
            {
                StringBuilder buf = new StringBuilder(MAXBYTE);
                if (GetClassName(hwnd, buf, MAXBYTE) && buf.ToString() == "TXGuiFoundation")
                    if (GetWindowTextLength(hwnd) > 0 && GetWindowText(hwnd, buf, MAXBYTE))
                    {
                        string str = buf.ToString();
                        if (str != "TXMenuWindow" && str != "QQ" && str != "增加时长")
                        {
                            Console.WriteLine("\t" + (hWndCollect.Count + 1) + ": " + str);
                            hWndCollect.Add(hwnd);
                        }
                    }
                return true;
            }

            public static int[] GetSessionWindow()
            {
                hWndCollect = new List<int>();
                EnumWindows(ScanSessionWindow, NULL);
                return hWndCollect.ToArray();
            }
        }
        public abstract partial class Win32Native
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool EnumWindows(LPENUMWINDOWSPROC lpEnumFunc, int lParam);

            [DllImport("user32.dll", CharSet = CharSet.Ansi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetClassName(int hWnd, StringBuilder buf, int nMaxCount);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern int GetWindowTextLength(int hWnd);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWindow(IntPtr hWnd);

            private delegate bool LPENUMWINDOWSPROC(int hwnd, int lParam);

            public const int NULL = 0;
            private const int MAXBYTE = 255;
        }
        private Bitmap TextToBitmap(string text, Font font, Rectangle rect, Brush fontcolor, Color backColor)
        {
            Graphics g;
            Bitmap bmp;
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            if (rect == Rectangle.Empty)
            {
                bmp = new Bitmap(1, 1);
                g = Graphics.FromImage(bmp);
                //计算绘制文字所需的区域大小（根据宽度计算长度），重新创建矩形区域绘图
                SizeF sizef = g.MeasureString(text, font, PointF.Empty, format);

                int width = (int)(sizef.Width + 5);
                int height = (int)(sizef.Height + 5);
                rect = new Rectangle(0, 0, width, height);
                bmp.Dispose();

                bmp = new Bitmap(width, height);
            }
            else
            {
                bmp = new Bitmap(rect.Width, rect.Height);
            }

            g = Graphics.FromImage(bmp);

            //使用ClearType字体功能
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(backColor), rect);
            g.DrawString(text, font, fontcolor, rect, format);
            return bmp;
        }

        /// <summary>
        /// 发送文字
        /// </summary>
        private void send()
        {

            IntPtr k = FindWindow(null, this.comboBox1.Text);
            SetForegroundWindow(k);//把找到的的对话框在最前面显示如果使用了这个方法
            Clipboard.Clear();
            string text = this.textBox3.Text;

            //得到Bitmap(传入Rectangle.Empty自动计算宽高)
            //Bitmap bmp = TextToBitmap(text, this.textBox1.Font, Rectangle.Empty, Brushes.Red, Color.White);

            //用PictureBox显示

            //Clipboard.SetImage(bmp);
            Clipboard.SetText(this.textBox3.Text);
            ShowWindow(k, 1);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            Thread threadz2 = new Thread(new ParameterizedThreadStart(GetMessage));//获取数据的进程
            threadz2.SetApartmentState(ApartmentState.STA);
            threadz2.IsBackground = true;
            threadz2.Start();
            comboBox1.Text = comboBox1.Items[0].ToString();
        }
        private Image SCREEN()
        {
            Image baseImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(baseImage);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);
            g.Dispose();
            return baseImage;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr k = FindWindow(null, comboBox1.Text);
            // SetForegroundWindow(k);//把找到的的对话框在最前面显示如果使用了这个方法
            Clipboard.Clear();
            string text = this.textBox1.Text;

            //得到Bitmap(传入Rectangle.Empty自动计算宽高)
            Bitmap bmp = TextToBitmap(text, this.textBox1.Font, Rectangle.Empty, Brushes.Red, this.textBox1.BackColor);


            {
                Clipboard.SetImage(bmp);
            }
            ShowWindow(k, 1);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = checkBox2.Checked;
        }
        public void sends()
        {

            IntPtr k = FindWindow(null, comboBox1.Text);
            // SetForegroundWindow(k);//把找到的的对话框在最前面显示如果使用了这个方法
            Clipboard.Clear();

            string text = this.textBox3.Text;
            //用PictureBox显示
            Bitmap bmp = TextToBitmap(text, this.textBox1.Font, Rectangle.Empty, Brushes.Red, this.textBox3.BackColor);
            Clipboard.SetImage(bmp);


            ShowWindow(k, 1);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            hour++;
            label5.Text = hour.ToString();
            if (hour >= 60 * shijian)
            {
                hour = 0;
                this.textBox3.Text = "打卡";
                sends();

            }
        }
        public void FindUserMessage(IntPtr hwnd)
        {
            if (!Win32Native.IsWindow(hwnd))
                label3.Text = "状态：Error";
            else
            {
                AutomationElement element = AutomationElement.FromHandle(hwnd);
                element = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "消息"));
                if (element != null && element.Current.IsEnabled)
                {
                    ValuePattern vpTextEdit = element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                    if (vpTextEdit != null)
                    {
                        value = vpTextEdit.Current.Value;
                    }
                }
            }
        }
        private void GetMessage(object str)
        {
            try
            {

                while (1 == 1)
                {
                    Thread.Sleep(200);
                    int[] hWndOfSession = Win32Native.GetSessionWindow();
                    if (hWndOfSession.Length <= 0)
                        label3.Text = "状态：异常";
                    else
                    {
                        label3.Text = "状态：正常";
                        foreach (int hWnd in hWndOfSession)
                            FindUserMessage((IntPtr)hWnd);
                        string[] content = value.Split('\r');
                        List<string> tempList = new List<string>();
                        foreach (var item in content)
                        {
                            if (!string.IsNullOrEmpty(item))
                                tempList.Add(item);
                        }

                        //ADD(content[content.Count() - 5] + ":" + content[content.Count() - 2], content[content.Count() - 5]);
                        ADD(tempList[tempList.Count() - 2] + ":" + tempList[tempList.Count() - 1], tempList[tempList.Count() - 2]);

                    }
                }
            }
            catch
            {
                Thread threadz2 = new Thread(new ParameterizedThreadStart(GetMessage));//获取数据的进程
                threadz2.SetApartmentState(ApartmentState.STA);
                threadz2.IsBackground = true;
                threadz2.Start();

            }

        }
        private string Get(string url)
        {
            string strHTML = "";
            WebClient myWebClient = new WebClient();
            Stream myStream = myWebClient.OpenRead(url);
            StreamReader sr = new StreamReader(myStream, System.Text.Encoding.GetEncoding("utf-8"));
            strHTML = sr.ReadToEnd();
            myStream.Close();
            return strHTML;
        }
        public void ADD(string item, string s)
        {

            Regex regex = new Regex(@"(\d+):(\d+):(\d+)");
            s = regex.Replace(s, "").Trim();
            Regex regex1 = new Regex(@"(?<=【).*?(?=】)");
            s = regex1.Replace(s, "").Trim().Replace("【", "").Replace("】", "");
            if (!this.listBox1.Items.Contains(item))
            {
                this.listBox1.Items.Add(item);

            }
            if (item.IndexOf("加入本群。") != -1)//猜拳
            {
                this.textBox3.Text = "欢迎 " + item.Replace("加入本群。", "") + " 加入阴阳师辅助群，购买辅助请联系群主或管理员，辅助价格10元每月，百分百不会被封！\n本辅助完全模拟人工，当前版本支持桌面版、一切模拟器，具体参考群中使用文件。";
                send();
                iscai = true;
            }
            if (item.IndexOf("\\猜拳") != -1 && iscai == false)//猜拳
            {
                this.textBox3.Text = "/xyx 请发送\\+剪刀或石头或布";
                send();
                iscai = true;
            }
            if (item.IndexOf("\\\\") != -1)
            {
                this.textBox3.Text = Get("http://i.itpk.cn/api.php?question=" + item.Replace('\\', ' ')).Replace("[name]", s).Replace("[cqname]", "@木头羊@");
                send();
            }
            if (item.IndexOf("\\命令") != -1)
            {

                this.textBox3.Text = "\\\\关键字：表示聊天\n\\猜拳：表示进行猜拳游戏\n\\继续：表示继续进行猜拳游戏，只需发送你要出的即可\n\\一言：表示获得一句鸡汤\n\\开启：表示开启打卡通知，仅支持管理员\n\\关闭：表示关闭打卡通知，仅支持管理员\n\\定时\\整数：表示每个整数分钟通知打卡，仅支持管理员，默认为60分钟\\n\\查询：表示查询金币余额";
                sends();
            }

            if (item.IndexOf("\\签到") != -1 && s != "")
            {

                Random a1 = new Random();
                int c = a1.Next(1000) + 1;
                if ((InIHelper.ReadConfig<string>("user", s + DateTime.Now.ToString("YYYY-MM-dd"))) != "1")//没有签到
                {
                    InIHelper.WriteConfig("user", s, "true");
                    InIHelper.WriteConfig("user", s + DateTime.Now.ToString("YYYY-MM-dd"), "1");
                    // OperateIniFile.WriteIniData("user",s,"true","config//user.ini");
                    this.textBox3.Text = s + "签到成功，赠送" + c + "/cp" + ",持有" + (InIHelper.ReadConfig<int>("user", s + "jb") + c) + "/cp";
                    InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") + c).ToString());
                    send();
                }
                else
                {
                    this.textBox3.Text = s + "：今天已经签到了，请明天再来";
                    send();
                }

            }
            if (item.IndexOf("\\继续") != -1)
            {
                iscai = true;

                this.textBox3.Text = "/ts请出！";
                send();
            }
            if (item.IndexOf("\\猜谜") != -1)
            {
                iscaimi = true;

                string[] mi = miyu();

                Md = mi[1];
                this.textBox3.Text = mi[0];
                send();
                p = 0;
            }
            if (iscaimi == true)
            {
                if (item.IndexOf("=" + Md) != -1)
                {
                    this.textBox3.Text = "恭喜，" + s + "猜对了，奖励1000/cp！";
                    InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") + 1000).ToString());
                    send();
                    iscaimi = false;
                    p = 0;
                }
                else
                {
                    if (item.IndexOf("=") != -1 && item.IndexOf("=" + Md) == -1)
                    {


                        if (p == 3)
                        {
                            this.textBox3.Text = "很遗憾，" + s + "猜错了,答案是：" + Md;
                            send();
                            p = 0;
                        }
                        else
                        {

                            this.textBox3.Text = "很遗憾，" + s + "猜错了,还剩余" + (3 - p) + "次机会，扣除2/cp";
                            InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") - 2).ToString());
                            send();
                            p++;
                        }
                    }
                }

            }
            if (item.IndexOf("\\查询") != -1)
            {
                if ((InIHelper.ReadConfig<string>("user", s)) != "true")//没有签到
                {
                    this.textBox3.Text = s + "，持有:0/cp";
                    send();
                }
                else
                {
                    this.textBox3.Text = s + "持有:" + InIHelper.ReadConfig<string>("user", s + "jb") + "/cp";
                    send();
                }
            }
            if (iscai == true)
            {
                Random a = new Random();
                int b = a.Next(1, 4);

                if (item.IndexOf("\\石头") != -1)
                {
                    if (b % 3 == 0)
                    {
                        sendwt("机器人出布，你出的石头，你输了，扣除10/cp。/cy", imageList1.Images[2], imageList1.Images[1]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") - 10).ToString());

                    }
                    if (b % 3 == 1)
                    {
                        sendwt("机器人出石头，你出的石头，平局，不扣除/cp。/fd", imageList1.Images[1], imageList1.Images[1]);
                    }
                    if (b % 3 == 2)
                    {
                        sendwt("机器人出剪刀，你出的石头，你赢了，奖励20/cp。/gz", imageList1.Images[0], imageList1.Images[1]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") + 20).ToString());
                    }
                    iscai = false;
                }
                if (item.IndexOf("\\剪刀") != -1)
                {
                    if (b % 3 == 0)
                    {
                        sendwt("机器人出石头，你出的剪刀，你输了，扣除10/cp。/cy", imageList1.Images[1], imageList1.Images[0]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") - 10).ToString());
                    }
                    if (b % 3 == 1)
                    {
                        sendwt("机器人出剪刀，你出的剪刀，平局，不扣除/cp。/fd", imageList1.Images[0], imageList1.Images[0]);
                    }
                    if (b % 3 == 2)
                    {
                        sendwt("机器人出布，你出的剪刀，你赢了，奖励20/cp。/gz", imageList1.Images[2], imageList1.Images[0]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") + 20).ToString());
                    }
                    iscai = false;
                }
                if (item.IndexOf("\\布") != -1)
                {
                    if (b % 3 == 0)
                    {
                        sendwt("机器人出剪刀，你出的布，你输了，扣除10/cp。/cy", imageList1.Images[0], imageList1.Images[2]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") - 10).ToString());
                    }
                    if (b % 3 == 1)
                    {
                        sendwt("机器人出布，你出的布，平局，不扣除/cp。/fd", imageList1.Images[2], imageList1.Images[2]);
                    }
                    if (b % 3 == 2)
                    {
                        sendwt("机器人出石头，你出的布，你赢了，奖励20/cp。/gz", imageList1.Images[1], imageList1.Images[2]);
                        InIHelper.WriteConfig("user", s + "jb", (InIHelper.ReadConfig<int>("user", s + "jb") + 20).ToString());
                    }
                    iscai = false;
                }

            }

            if (isin(s) == true && item.IndexOf("\\开启") != -1)
            {
                this.textBox3.Text = "通知开启成功！";
                send();
                checkBox2.Checked = true;
            }
            if (isin(s) == true && item.IndexOf("\\关闭") != -1)
            {
                this.textBox3.Text = "通知关闭成功！";
                send();
                checkBox2.Checked = false;
            }
            if (isin(s) == true && item.IndexOf("\\定时") != -1)
            {
                string[] a = item.Split('\\');
                if (a.Count() > 1 && int.Parse(a[2]) > 0)
                {
                    shijian = int.Parse(a[2]);
                    this.textBox3.Text = "定时设置成功，间隔" + a[2].Trim() + "分钟！";
                    checkBox2.Checked = true;
                    timer1.Enabled = true;
                }
                else
                {
                    this.textBox3.Text = "[消息]：定时设置格式错误，请以#定时数字格式输入！";
                }
                send();

            }
            if (item.IndexOf("\\一言") != -1)
            {
                this.textBox3.Text = Get("https://api.lwl12.com/hitokoto/main/get");
                send();

            }

        }
        public bool isin(string s)
        {
            int k = 0;
            for (int i = 0; i <= listBox2.Items.Count - 1; i++)
            {
                // MessageBox.Show(listBox2.Items[i].ToString ());
                if (s.Trim().IndexOf(listBox2.Items[i].ToString()) != -1)
                {

                    k = 1;

                }
            }
            if (k == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int[] hWndOfSession = Win32Native.GetSessionWindow();
                if (hWndOfSession.Length <= 0)
                    label3.Text = "状态：No";
                else
                {
                    label3.Text = "状态：Yes";
                    foreach (int hWnd in hWndOfSession)
                        FindUserMessage((IntPtr)hWnd);
                    string[] content = value.Split(' ');

                    ADD(content[content.Count() - 5] + ":" + content[content.Count() - 2], content[content.Count() - 3]);

                }
            }
            catch
            {


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox2.Items.Add(textBox4.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                listBox2.Items.RemoveAt(listBox2.SelectedIndex);
            }
            catch
            { }
        }
        public void sendt(string text, Image img)
        {

            IntPtr k = FindWindow(null, comboBox1.Text);
            // SetForegroundWindow(k);//把找到的的对话框在最前面显示如果使用了这个方法
            Clipboard.Clear();


            Clipboard.SetImage(img);
            //Clipboard.SetText(text);

            ShowWindow(k, 1);
            SendMessage(k, WM_PASTE, 0, 0);


            ShowWindow(k, 1);
            Clipboard.Clear();
            Clipboard.SetText(text);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }
        public void sendwt(string text, Image img, Image img2)
        {

            IntPtr k = FindWindow(null, comboBox1.Text);
            // SetForegroundWindow(k);//把找到的的对话框在最前面显示如果使用了这个方法
            Clipboard.Clear();


            // Clipboard.SetImage(img);
            //Clipboard.SetText(text);

            // ShowWindow(k, 1);
            // SendMessage(k, WM_PASTE, 0, 0);
            //ShowWindow(k, 1);
            // Clipboard.Clear();
            // Clipboard.SetText("  VS  ");
            //  SendMessage(k, WM_PASTE, 0, 0);
            // ShowWindow(k, 1);
            // Clipboard.Clear();
            // Clipboard.SetImage(img2);
            // SendMessage(k, WM_PASTE, 0, 0);
            ShowWindow(k, 1);
            Clipboard.Clear();
            Clipboard.SetText(text);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }
        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            IntPtr k = FindWindow(null, comboBox1.Text);
            Clipboard.Clear();
            System.Collections.Specialized.StringCollection strcoll = new System.Collections.Specialized.StringCollection();
            // strcoll.Add(@"C:\Users\admin\Documents\Visual Studio 2010\Projects\QQRoboter\QQRoboter\bin\Debug");
            strcoll.Add(textBox2.Text);
            Clipboard.SetFileDropList(strcoll);
            ShowWindow(k, 1);
            SendMessage(k, WM_PASTE, 0, 0);
            SendMessage(k, WM_KEYDOWN, 0X0D, 0);//发

            SendMessage(k, WM_KEYUP, 0X0D, 0); //送

            SendMessage(k, WM_CHAR, 0X0D, 0); //回车
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Process[] process = Process.GetProcesses();

            foreach (Process p in process)
            {
                if (p.MainWindowTitle.ToString() != "" && p.ProcessName.ToLower() == "qq" && comboBox1.Items.Contains(p.MainWindowTitle.ToString()) == false)
                    comboBox1.Items.Add(p.MainWindowTitle.ToString());
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {

            send();

        }
        private string[] miyu()
        {
            StreamReader sr = new StreamReader("谜语.txt", System.Text.Encoding.GetEncoding("gb2312"));//你的txt文件路径
            int i = 0;
            string rowstring = "";
            Random a = new Random();
            int b = a.Next(1, 3863);
            while (sr.Peek() > 0)
            {
                rowstring = sr.ReadLine();// 获取当前行字符串
                i++;//用于标记现在读到第几行
                if (i == b)
                {
                    break;
                }
            }
            int c = rowstring.IndexOf("答案是：");
            int d = rowstring.IndexOf("、");
            string midi = rowstring.Substring(c + 4, 1);
            string mimian = rowstring.Substring(d + 1, c - 5);
            sr.Close();
            string[] m = { mimian, midi };
            return m;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader("谜语.txt", System.Text.Encoding.GetEncoding("gb2312"));//你的txt文件路径
            int i = 0;
            string rowstring = "";
            Random a = new Random();
            int b = a.Next(1, 3863);
            while (sr.Peek() > 0)
            {
                rowstring = sr.ReadLine();// 获取当前行字符串
                i++;//用于标记现在读到第几行
                if (i == b)
                {
                    break;
                }
            }
            int c = rowstring.IndexOf("答案是：");
            int d = rowstring.IndexOf("、");
            string midi = rowstring.Substring(c + 4, 1);
            string mimian = rowstring.Substring(d + 1, c - 5);
            sr.Close();
            MessageBox.Show(mimian + "\n" + midi);
        }

        private void button8_Click(object sender, EventArgs e)
        {


        }
    }
}
