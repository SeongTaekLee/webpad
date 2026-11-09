// 웹패드 실행 파일 (설치 없는 포터블, 파일 하나)
// - 편집기(index.html, 글꼴)와 WebView2 라이브러리를 exe 안에 넣어 두고, 처음 실행할 때 exe 옆 WebPad_data 에 푼다.
//   (exe 옆에 쓸 수 없으면 %LOCALAPPDATA%\WebPad)
// - 윈도우에 내장된 WebView2로 자체 창을 띄우므로 Ctrl+N / Ctrl+W / Ctrl+Tab 같은 키를 편집기가 직접 받는다.
// - 파일을 exe로 열면(연결 프로그램, 끌어다 놓기) 탭으로 열리고, 이미 켜져 있으면 그 창으로 넘긴다.
// - WebView2 런타임이 없는 PC에서는 엣지/크롬 앱 창으로 대신 연다.
// 빌드: launcher\build.cmd (윈도우 기본 .NET Framework 4 csc)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

[assembly: AssemblyTitle("WebPad")]
[assembly: AssemblyProduct("WebPad")]
[assembly: AssemblyDescription("웹패드 - 가벼운 코드 편집기")]
[assembly: AssemblyVersion("1.2.3.0")]
[assembly: AssemblyFileVersion("1.2.3.0")]

static class Program
{
    public static string Data, AppDir, Key;

    [STAThread]
    static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbedded;
        try
        {
            Data = DataDir();
            AppDir = Path.Combine(Data, "app");
            Key = "WebPad_" + ((uint)Data.ToLowerInvariant().GetHashCode()).ToString("x8");
            string file = args.Length > 0 ? Path.GetFullPath(args[0]) : null;

            bool first;
            Mutex mutex = new Mutex(true, Key, out first);
            if (!first) { Forward(file); return 0; }

            Extract();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!HasWebView2()) { LaunchBrowser(); return 0; }
            Run(file);
            GC.KeepAlive(mutex);
        }
        catch (Exception ex)
        {
            MessageBox.Show("웹패드를 시작하지 못했어요.\n\n" + ex.Message, "웹패드", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return 0;
    }

    // WebView2 형식을 쓰는 코드는 따로 두어, 내장 DLL을 찾는 처리기가 먼저 등록되게 한다
    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Run(string file) { Application.Run(new MainForm(file)); }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static bool HasWebView2()
    {
        try
        {
            CoreWebView2Environment.SetLoaderDllFolderPath(Path.Combine(Data, "bin"));
            return !string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString());
        }
        catch (Exception) { return false; }
    }

    static Assembly ResolveEmbedded(object sender, ResolveEventArgs e)
    {
        string name = new AssemblyName(e.Name).Name;
        using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("lib/" + name + ".dll"))
        {
            if (s == null) return null;
            return Assembly.Load(ReadAll(s));
        }
    }

    static string DataDir()
    {
        string beside = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebPad_data");
        try
        {
            Directory.CreateDirectory(beside);
            string probe = Path.Combine(beside, ".write-test");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return beside;
        }
        catch (Exception)
        {
            string local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebPad");
            Directory.CreateDirectory(local);
            return local;
        }
    }

    static string Arch()
    {
        string a = (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") ?? Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "").ToUpperInvariant();
        if (a == "ARM64") return "arm64";
        return Environment.Is64BitProcess ? "x64" : "x86";
    }

    // 내용이 바뀐 파일만 다시 쓴다 (새 버전 exe를 실행하면 자동으로 갱신)
    static void Extract()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        string loader = "bin/" + Arch() + "/WebView2Loader.dll";
        foreach (string name in asm.GetManifestResourceNames())
        {
            string target;
            if (name.StartsWith("app/")) target = Path.Combine(Data, name.Replace('/', Path.DirectorySeparatorChar));
            else if (name == loader) target = Path.Combine(Data, "bin", "WebView2Loader.dll");
            else continue;
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            byte[] data;
            using (Stream s = asm.GetManifestResourceStream(name)) data = ReadAll(s);
            if (File.Exists(target) && SameBytes(File.ReadAllBytes(target), data)) continue;
            try { File.WriteAllBytes(target, data); } catch (IOException) { }
        }
    }

    static byte[] ReadAll(Stream s)
    {
        byte[] data = new byte[s.Length];
        int read = 0;
        while (read < data.Length) read += s.Read(data, read, data.Length - read);
        return data;
    }

    static bool SameBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    public static string Inbox { get { return Path.Combine(Data, "inbox"); } }

    // 이미 켜진 웹패드에 파일을 넘기고 그 창을 앞으로 가져오게 한다
    static void Forward(string file)
    {
        if (file != null)
        {
            Directory.CreateDirectory(Inbox);
            File.WriteAllText(Path.Combine(Inbox, Guid.NewGuid().ToString("N") + ".txt"), file);
        }
        try { EventWaitHandle.OpenExisting(Key + "_evt").Set(); } catch (Exception) { }
    }

    public static void LaunchBrowser()
    {
        string url = new Uri(Path.Combine(AppDir, "index.html")).AbsoluteUri;
        string browser = FindBrowser();
        if (browser == null) { Process.Start(url); return; }
        ProcessStartInfo psi = new ProcessStartInfo(browser,
            "--app=\"" + url + "\" --user-data-dir=\"" + Path.Combine(Data, "profile-browser") + "\" --no-first-run --no-default-browser-check --disable-sync");
        psi.UseShellExecute = false;
        Process.Start(psi);
    }

    static string FindBrowser()
    {
        string[] exes = { "msedge.exe", "chrome.exe" };
        foreach (string exe in exes)
        {
            string p = AppPath(Registry.CurrentUser, exe) ?? AppPath(Registry.LocalMachine, exe);
            if (p != null && File.Exists(p)) return p;
        }
        string[] guesses = {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft\Edge\Application\msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft\Edge\Application\msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Google\Chrome\Application\chrome.exe")
        };
        foreach (string g in guesses) if (File.Exists(g)) return g;
        return null;
    }

    static string AppPath(RegistryKey hive, string exe)
    {
        using (RegistryKey k = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + exe))
            return k == null ? null : k.GetValue(null) as string;
    }
}

// 윈도우 기본 "폴더 선택" 창 (탐색기와 같은 모양). .NET 4의 FolderBrowserDialog는 옛날 트리 모양이라 직접 부른다
static class FolderPicker
{
    [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
    class FileOpenDialogCom { }

    [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileOpenDialog
    {
        [PreserveSig] int Show(IntPtr parent);
        void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
        void SetFileTypeIndex(uint iFileType);
        void GetFileTypeIndex(out uint piFileType);
        void Advise(IntPtr pfde, out uint pdwCookie);
        void Unadvise(uint dwCookie);
        void SetOptions(uint fos);
        void GetOptions(out uint pfos);
        void SetDefaultFolder(IShellItem psi);
        void SetFolder(IShellItem psi);
        void GetFolder(out IShellItem ppsi);
        void GetCurrentSelection(out IShellItem ppsi);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        void GetResult(out IShellItem ppsi);
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItem
    {
        void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
        void GetParent(out IShellItem ppsi);
        void GetDisplayName(uint sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    static extern void SHCreateItemFromParsingName(string pszPath, IntPtr pbc, ref Guid riid, out IShellItem ppv);

    public static string Pick(IntPtr owner, string start)
    {
        IFileOpenDialog dlg = (IFileOpenDialog)new FileOpenDialogCom();
        try
        {
            dlg.SetOptions(0x20 | 0x40 | 0x800); // 폴더 고르기 | 실제 파일 시스템만 | 경로가 있어야 함
            dlg.SetTitle("폴더 열기");
            if (!string.IsNullOrEmpty(start) && Directory.Exists(start))
            {
                Guid iid = typeof(IShellItem).GUID;
                IShellItem si;
                try { SHCreateItemFromParsingName(start, IntPtr.Zero, ref iid, out si); dlg.SetFolder(si); } catch (Exception) { }
            }
            if (dlg.Show(owner) != 0) return null;
            IShellItem result;
            dlg.GetResult(out result);
            string path;
            result.GetDisplayName(0x80058000, out path); // SIGDN_FILESYSPATH
            return path;
        }
        finally { Marshal.ReleaseComObject(dlg); }
    }
}

class MainForm : Form
{
    WebView2 web;
    string pendingFile;
    bool ready, allowClose, closing;
    JavaScriptSerializer json = new JavaScriptSerializer();
    EventWaitHandle wake;

    public MainForm(string file)
    {
        pendingFile = file;
        json.MaxJsonLength = int.MaxValue;
        Text = "웹패드";
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch (Exception) { }
        StartPosition = FormStartPosition.Manual;
        LoadBounds();
        web = new WebView2();
        web.Dock = DockStyle.Fill;
        Controls.Add(web);
        Load += async (s, e) => await Init();
        FormClosing += OnClosingForm;

        // 두 번째로 실행된 웹패드가 보낸 파일을 받는다
        wake = new EventWaitHandle(false, EventResetMode.AutoReset, Program.Key + "_evt");
        Thread t = new Thread(() => { while (true) { wake.WaitOne(); try { BeginInvoke(new Action(ProcessInbox)); } catch (Exception) { return; } } });
        t.IsBackground = true;
        t.Start();
    }

    async Task Init()
    {
        bool debug = Environment.GetEnvironmentVariable("WEBPAD_DEBUG") != null;
        try
        {
            CoreWebView2EnvironmentOptions opts = new CoreWebView2EnvironmentOptions(debug ? "--remote-debugging-port=9333" : null);
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Program.Data, "profile"), opts);
            await web.EnsureCoreWebView2Async(env);
        }
        catch (Exception)
        {
            Program.LaunchBrowser();
            allowClose = true;
            Close();
            return;
        }
        CoreWebView2 core = web.CoreWebView2;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false; // 찾기·인쇄·새로고침 같은 브라우저 단축키 대신 편집기 단축키
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
        core.Settings.AreDevToolsEnabled = debug;
        core.Settings.IsReputationCheckingRequired = false; // 주소 평판 확인(SmartScreen)으로 외부에 알리지 않는다
        core.SetVirtualHostNameToFolderMapping("webpad.local", Program.AppDir, CoreWebView2HostResourceAccessKind.Allow);
        core.DocumentTitleChanged += (s, e) => { Text = core.DocumentTitle; };
        core.NewWindowRequested += (s, e) => { e.Handled = true; try { Process.Start(e.Uri); } catch (Exception) { } };
        core.WebMessageReceived += OnMessage;
        core.Navigate("https://webpad.local/index.html");
    }

    void OnMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        Dictionary<string, object> m;
        try { m = json.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson); } catch (Exception) { return; }
        string type = m.ContainsKey("type") ? m["type"] as string : null;
        if (type == "ready")
        {
            ready = true;
            if (pendingFile != null) { SendFile(pendingFile); pendingFile = null; }
            ProcessInbox();
        }
        else if (type == "flushed")
        {
            allowClose = true;
            Close();
        }
        else if (m.ContainsKey("id"))
        {
            // 파일 다루기는 브라우저 대신 윈도우가 직접 한다 (브라우저는 C:\ 나 사용자 폴더 같은 곳을 막는다)
            Dictionary<string, object> reply = new Dictionary<string, object>();
            reply["type"] = "reply";
            reply["id"] = m["id"];
            try
            {
                if (type == "dropPaths") DropPaths(e, reply); else HostCall(type, m, reply);
                reply["ok"] = true;
            }
            catch (Exception ex) { reply["ok"] = false; reply["error"] = ex.Message; }
            web.CoreWebView2.PostWebMessageAsJson(json.Serialize(reply));
        }
    }

    // 끌어다 놓은 파일: 브라우저는 이름만 알려주므로 함께 넘어온 파일 객체에서 전체 경로를 꺼낸다
    static void DropPaths(CoreWebView2WebMessageReceivedEventArgs e, Dictionary<string, object> reply)
    {
        List<string> files = new List<string>(), dirs = new List<string>();
        var objs = e.AdditionalObjects;
        if (objs != null)
            foreach (object o in objs)
            {
                CoreWebView2File f = o as CoreWebView2File;
                if (f == null || string.IsNullOrEmpty(f.Path)) continue;
                if (Directory.Exists(f.Path)) dirs.Add(f.Path); else files.Add(f.Path);
            }
        reply["files"] = files;
        reply["dirs"] = dirs;
    }

    static readonly HashSet<string> SkipDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", "node_modules", "__pycache__", ".svn", "$RECYCLE.BIN", "System Volume Information" };

    // 기본은 "숨김이면서 시스템"인 항목($Recycle.Bin 등)을 감춘다. 설정에서 켜면 모두 보여준다
    static bool IsHiddenSystem(FileSystemInfo fi)
    {
        return (fi.Attributes & FileAttributes.System) != 0 && (fi.Attributes & FileAttributes.Hidden) != 0;
    }

    void HostCall(string type, Dictionary<string, object> m, Dictionary<string, object> reply)
    {
        string path = m.ContainsKey("path") ? m["path"] as string : null;
        bool showHidden = m.ContainsKey("hidden") && m["hidden"] is bool && (bool)m["hidden"];
        if (type == "write")
        {
            File.WriteAllBytes(path, Convert.FromBase64String((string)m["b64"]));
        }
        else if (type == "read")
        {
            reply["b64"] = Convert.ToBase64String(File.ReadAllBytes(path));
            reply["name"] = Path.GetFileName(path);
        }
        else if (type == "list")
        {
            List<object> list = new List<object>();
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileSystemInfo fi in di.EnumerateFileSystemInfos())
            {
                if (!showHidden && IsHiddenSystem(fi)) continue;
                Dictionary<string, object> e = new Dictionary<string, object>();
                e["name"] = fi.Name;
                e["dir"] = (fi.Attributes & FileAttributes.Directory) != 0;
                list.Add(e);
            }
            reply["entries"] = list;
        }
        else if (type == "search")
        {
            string q = ((string)m["q"]).ToLowerInvariant();
            int max = Convert.ToInt32(m["max"]), scanned = 0;
            List<object> found = new List<object>();
            Stack<string> stack = new Stack<string>();
            stack.Push(path);
            while (stack.Count > 0 && found.Count < max && scanned < 200000)
            {
                string dir = stack.Pop();
                IEnumerable<FileSystemInfo> items;
                try { items = new DirectoryInfo(dir).EnumerateFileSystemInfos(); } catch (Exception) { continue; }
                try
                {
                    foreach (FileSystemInfo fi in items)
                    {
                        scanned++;
                        if (!showHidden && IsHiddenSystem(fi)) continue;
                        if ((fi.Attributes & FileAttributes.Directory) != 0) { if (!SkipDirs.Contains(fi.Name)) stack.Push(fi.FullName); continue; }
                        if (fi.Name.ToLowerInvariant().Contains(q)) { found.Add(fi.FullName); if (found.Count >= max) break; }
                    }
                }
                catch (Exception) { }
            }
            reply["paths"] = found;
        }
        else if (type == "pickFolder")
        {
            reply["path"] = FolderPicker.Pick(Handle, m.ContainsKey("start") ? m["start"] as string : null);
        }
        else if (type == "pickFiles")
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                dlg.Title = "파일 열기";
                dlg.Filter = "모든 파일 (*.*)|*.*";
                reply["paths"] = dlg.ShowDialog(this) == DialogResult.OK ? new List<string>(dlg.FileNames) : new List<string>();
            }
        }
        else if (type == "pickSave")
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "다른 이름으로 저장";
                dlg.Filter = "모든 파일 (*.*)|*.*";
                dlg.FileName = m.ContainsKey("name") ? m["name"] as string : "";
                string dir = m.ContainsKey("dir") ? m["dir"] as string : null;
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) dlg.InitialDirectory = dir;
                reply["path"] = dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
            }
        }
        else throw new Exception("알 수 없는 요청: " + type);
    }

    void SendFile(string path)
    {
        Dictionary<string, object> msg = new Dictionary<string, object>();
        msg["type"] = "openFile";
        msg["path"] = path;
        msg["name"] = Path.GetFileName(path);
        try { msg["b64"] = Convert.ToBase64String(File.ReadAllBytes(path)); }
        catch (Exception ex) { MessageBox.Show(this, path + "\n\n" + ex.Message, "파일을 열 수 없어요"); return; }
        web.CoreWebView2.PostWebMessageAsJson(json.Serialize(msg));
    }

    void ProcessInbox()
    {
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        if (!ready || !Directory.Exists(Program.Inbox)) return;
        foreach (string f in Directory.GetFiles(Program.Inbox, "*.txt"))
        {
            string path = null;
            try { path = File.ReadAllText(f).Trim(); File.Delete(f); } catch (Exception) { }
            if (!string.IsNullOrEmpty(path)) SendFile(path);
        }
    }

    // 닫기 전에 편집기에게 임시 저장을 마치게 한다 (최대 2초 기다림)
    void OnClosingForm(object sender, FormClosingEventArgs e)
    {
        SaveBounds();
        if (allowClose || !ready || web.CoreWebView2 == null) return;
        e.Cancel = true;
        if (closing) return;
        closing = true;
        web.CoreWebView2.ExecuteScriptAsync("window.__webpadFlush && window.__webpadFlush()");
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        t.Interval = 2000;
        t.Tick += (s, a) => { t.Stop(); allowClose = true; Close(); };
        t.Start();
    }

    string BoundsFile { get { return Path.Combine(Program.Data, "window.txt"); } }

    void LoadBounds()
    {
        Size = new Size(1200, 800);
        Rectangle wa = Screen.PrimaryScreen.WorkingArea;
        Location = new Point(wa.X + (wa.Width - Width) / 2, wa.Y + (wa.Height - Height) / 2);
        try
        {
            string[] p = File.ReadAllText(BoundsFile).Split(',');
            Rectangle r = new Rectangle(int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]), int.Parse(p[3]));
            foreach (Screen s in Screen.AllScreens)
                if (s.WorkingArea.IntersectsWith(r)) { Bounds = r; break; }
            if (p.Length > 4 && p[4] == "max") WindowState = FormWindowState.Maximized;
        }
        catch (Exception) { }
    }

    void SaveBounds()
    {
        try
        {
            Rectangle r = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            File.WriteAllText(BoundsFile, r.X + "," + r.Y + "," + r.Width + "," + r.Height + (WindowState == FormWindowState.Maximized ? ",max" : ""));
        }
        catch (Exception) { }
    }
}
