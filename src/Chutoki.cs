// Chutoki Free - le ventilo qui fait chut
// Copyright (c) 2026 Radjouh Slim - Licence MIT (voir LICENSE.txt)
//
// Pilote les ventilateurs de la carte mère pour un PC silencieux et frais :
// courbes douces, lissage des pics de température, montée rapide / descente lente,
// garde-fous (montée rapide de température, zone d'alerte, renfort du mode Calme),
// sécurités (100 % en cas d'urgence ou de capteur perdu, ventilo bloqué, retour BIOS à la fermeture).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using LibreHardwareMonitor.Hardware;

[assembly: AssemblyTitle("Chutoki Free")]
[assembly: AssemblyProduct("Chutoki Free")]
[assembly: AssemblyDescription("Chutoki Free - le ventilo qui fait chut")]
[assembly: AssemblyCompany("Radjouh Slim")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Radjouh Slim - Licence MIT")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]
[assembly: AssemblyInformationalVersion("1.2.0")]

namespace Chutoki {

// ====================================================================== application
static class App {
    public const string Name = "Chutoki";
    public const string Edition = "Free";
    public const string Version = "1.2.0";
    public const string Site = "https://slimouzz.github.io/chutoki/";
    public const string NewsUrl = Site + "news.json";
    public const string Repo = "https://github.com/Slimouzz/chutoki";
    public const string Author = "Radjouh Slim";
    public const string Email = "contact-chutoki@proton.me";
    public const string Year = "2026";
    public const string PawnIoUrl = "https://github.com/namazso/PawnIO.Setup/releases/latest";
    public const string TaskName = "Chutoki";
    public static readonly string[] Langs = { "fr", "en", "it", "es", "zh", "ja" };
    public static readonly string[] LangNames = { "Français", "English", "Italiano", "Español", "中文", "日本語" };
    public static string Lang = DetectLang();
    public static bool Preview;
    static string DetectLang() {
        string l = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return Langs.Contains(l) ? l : "en";
    }
    public static bool Cjk { get { return Lang == "zh" || Lang == "ja"; } }
    // Texte traduit : français, anglais, et les autres langues via Lang.cs (clé = texte anglais)
    public static string T(string fr, string en) {
        if (Lang == "fr") return fr;
        if (Lang == "en") return en;
        return Tr.Get(en, Lang) ?? en;
    }
    public static string T(string fr, string en, params object[] a) { return string.Format(T(fr, en), a); }
    public static string Dir { get { return Path.GetDirectoryName(Application.ExecutablePath); } }

    public static void Open(string target) {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); } catch { }
    }
    public static string Run(string exe, string args, out int code) {
        code = -1;
        try {
            var p = Process.Start(new ProcessStartInfo(exe, args) {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true });
            string o = p.StandardOutput.ReadToEnd(); p.StandardError.ReadToEnd(); p.WaitForExit(8000); code = p.ExitCode; return o;
        } catch { return ""; }
    }
    public static bool AutostartEnabled() { int c; Run("schtasks", "/query /tn " + TaskName, out c); return c == 0; }
    public static bool SetAutostart(bool on) {
        int c;
        if (on) Run("schtasks", "/create /f /tn " + TaskName + " /sc onlogon /rl highest /delay 0000:20 /tr \"\\\"" + Application.ExecutablePath + "\\\" --tray\"", out c);
        else Run("schtasks", "/delete /f /tn " + TaskName, out c);
        return AutostartEnabled() == on;
    }
}

// ====================================================================== thème (suit le mode clair / sombre de Windows)
static class Theme {
    public static bool Dark;
    public static Color Bg, Card, CardHover, Txt, Dim, Line, Accent, AccentTxt;
    public static readonly Color Ok = Color.FromArgb(46, 184, 114), Warm = Color.FromArgb(245, 160, 40),
        Hot = Color.FromArgb(232, 72, 85), Idle = Color.FromArgb(140, 146, 160);
    static Theme() {
        try {
            var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            object v = k != null ? k.GetValue("AppsUseLightTheme") : null;
            Dark = v is int && (int)v == 0;
        } catch { }
        Accent = Color.FromArgb(64, 132, 246); AccentTxt = Color.White;
        if (Dark) {
            Bg = Color.FromArgb(24, 26, 31); Card = Color.FromArgb(36, 39, 47); CardHover = Color.FromArgb(46, 50, 60);
            Txt = Color.FromArgb(236, 238, 242); Dim = Color.FromArgb(150, 156, 168); Line = Color.FromArgb(56, 60, 72);
        } else {
            Bg = Color.FromArgb(246, 247, 250); Card = Color.White; CardHover = Color.FromArgb(236, 240, 248);
            Txt = Color.FromArgb(28, 30, 36); Dim = Color.FromArgb(110, 116, 128); Line = Color.FromArgb(222, 226, 234);
        }
    }
    // Police adaptée à la langue (chinois et japonais ont besoin de leurs propres caractères)
    static string Family { get { return App.Lang == "zh" ? "Microsoft YaHei UI" : App.Lang == "ja" ? "Yu Gothic UI" : "Segoe UI"; } }
    public static Font F(float size, FontStyle st = FontStyle.Regular) { return new Font(Family, App.Cjk ? size * 0.95f : size, st); }
    public static Font Semi(float size) { return App.Cjk ? new Font(Family, size * 0.95f, FontStyle.Bold) : new Font("Segoe UI Semibold", size); }
    public static GraphicsPath Round(RectangleF r, float rad) {
        var p = new GraphicsPath(); float d = rad * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }
    public static void Prepare(Form f, int w, int h, string title) {
        f.AutoScaleDimensions = new SizeF(96, 96); f.AutoScaleMode = AutoScaleMode.Dpi;
        f.Text = title; f.ClientSize = new Size(w, h); f.BackColor = Bg; f.ForeColor = Txt; f.Font = F(10f);
        f.FormBorderStyle = FormBorderStyle.FixedSingle; f.MaximizeBox = false;
        try { f.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
    }
    public static Button Btn(string text, int x, int y, int w, int h = 34, bool primary = false) {
        var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = h, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            BackColor = primary ? Accent : Card, ForeColor = primary ? AccentTxt : Txt, Font = primary ? Semi(10.5f) : F(10f) };
        b.FlatAppearance.BorderColor = primary ? Accent : Line;
        return b;
    }
}

// ====================================================================== réglages
class Settings {
    readonly string path;
    readonly Dictionary<string, string> d = new Dictionary<string, string>();
    public readonly string Folder;
    public Settings() {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Folder = Path.Combine(appData, "Chutoki");
        Directory.CreateDirectory(Folder);
        path = Path.Combine(Folder, "settings.ini");
        Load(path);
        // Reprise des réglages de ventilateurs de l'ancienne version (Fan Blay4mok)
        string old = Path.Combine(appData, "FanBlay4mok", "settings.ini");
        if (!File.Exists(path) && File.Exists(old)) {
            var o = new Dictionary<string, string>(); Read(old, o);
            foreach (var kv in o) if (kv.Key.StartsWith("role.") || kv.Key.StartsWith("floor.") || kv.Key == "cooling" || kv.Key == "tray") d[kv.Key] = kv.Value;
            Save();
        }
    }
    void Load(string p) { Read(p, d); }
    static void Read(string p, Dictionary<string, string> into) {
        try {
            if (File.Exists(p))
                foreach (var line in File.ReadAllLines(p)) { int i = line.IndexOf('='); if (i > 0) into[line.Substring(0, i)] = line.Substring(i + 1); }
        } catch { }
    }
    public string Get(string k, string def) { string v; return d.TryGetValue(k, out v) ? v : def; }
    public void Set(string k, string v) { d[k] = v; Save(); }
    public void Save() { try { File.WriteAllLines(path, d.Select(kv => kv.Key + "=" + kv.Value).ToArray()); } catch { } }
}

// ====================================================================== moteur de régulation
class Curve {
    readonly float[] t, v;
    public Curve(float[] temps, float[] duty) { t = temps; v = duty; }
    public float Eval(float x) {
        if (float.IsNaN(x)) return 100;
        if (x <= t[0]) return v[0];
        for (int i = 1; i < t.Length; i++)
            if (x <= t[i]) { float f = (x - t[i - 1]) / (t[i] - t[i - 1]); return v[i - 1] + f * (v[i] - v[i - 1]); }
        return v[v.Length - 1];
    }
}

class Profile {
    public string Key; public Curve Cpu, Gpu; public float Up, Down, Min, Rise, Hyst;
    public string Title { get {
        return Key == "sil" ? App.T("Calme", "Quiet") : Key == "eq" ? App.T("Normal", "Normal") : App.T("Fort", "Strong"); } }
}

enum Role { Bios, Cpu, CpuGpu, Gpu }

class Channel {
    public string Id, Name; public ISensor Ctrl, Rpm; public Role Role;
    public float Duty, LastSent = -1, FloorBoost;
    public bool Controlled, WasSpinning, PumpLike, CpuLike;
    public DateTime LastSendTime = DateTime.MinValue;
    public int Stall, DeadSec;
}

enum Level { Ok, Warm, Hot, Idle }

class Engine {
    public Computer Comp;
    IHardware cpuHw, gpuHw, mbHw;
    ISensor cpuT, gpuT, cpuP, gpuP, cpuL, gpuL;
    public readonly List<Channel> Chans = new List<Channel>();
    public readonly Dictionary<string, Profile> Profiles = new Dictionary<string, Profile>();
    public Profile Cur;
    public string Mode = "sil";
    public bool AutoHeavy, Paused;
    // garde-fous
    public bool Fast, WarnZone, Boost;
    public float CpuWarn = 82, GpuWarn = 92, CpuCalmMax = 78, GpuCalmMax = 90;
    readonly Queue<float> histC = new Queue<float>(), histG = new Queue<float>();
    DateTime fastUntil = DateTime.MinValue;
    int calmHotSec, calmCoolSec;
    // profil réellement appliqué : le mode Calme se renforce en Normal s'il fait chaud trop longtemps
    public Profile Eff { get { return Boost && Cur.Key == "sil" ? Profiles["eq"] : Cur; } }
    public bool Laptop, PawnIoMissing, InitFailed;
    public string CpuName = "?", GpuName = "?", BoardName = "?", InitError;
    public bool GpuHotspot, Intel;
    public float CpuCrit = 95, CpuHotAlert = 90, GpuCrit = 100;
    public float? CpuTemp, GpuTemp, CpuW, GpuW, CpuLoad, GpuLoad;
    public float CpuS = float.NaN, GpuS = float.NaN, AvgDuty = float.NaN;
    public bool Emergency, SensorLost;
    public event Action<string, string, bool> Alerted;
    public event Action ModeChanged;
    readonly Settings cfg;

    DateTime heavyStart = DateTime.MinValue, lightStart = DateTime.MinValue;
    // statistiques de session
    public readonly DateTime SessionStart = DateTime.Now;
    public float MaxCpu = float.NaN, MaxGpu = float.NaN, MaxDuty;
    public double SumCpu; public int NCpu, SecSil, SecEq, SecPerf, Emergencies, AlertsCount;
    int lostSec, hotSec, idleHotSec; bool wasEmergency;

    public Engine(Settings s) {
        cfg = s;
        // CPU : température Tctl (AMD) ou Package (Intel). GPU : point chaud (ou coeur + 12 °C si absent).
        // Calme : les ventilateurs restent au minimum jusqu'à ~65 °C, réagissent peu aux pics courts
        // et ne changent de vitesse que très doucement (les garde-fous prennent le relais si ça chauffe).
        Add("sil", new[] { 65f, 72, 80, 87, 93 }, new[] { 20f, 24, 35, 60, 100 }, new[] { 70f, 80, 88, 96, 102 }, new[] { 20f, 26, 42, 72, 100 }, 2, 0.5f, 20, 0.12f, 3f);
        Add("eq", new[] { 50f, 65, 75, 85, 90 }, new[] { 25f, 33, 48, 72, 100 }, new[] { 60f, 72, 82, 92, 100 }, new[] { 25f, 38, 55, 85, 100 }, 5, 1.5f, 25, 0.30f, 1.5f);
        Add("perf", new[] { 45f, 60, 70, 80, 88 }, new[] { 35f, 50, 65, 85, 100 }, new[] { 55f, 68, 78, 88, 95 }, new[] { 35f, 50, 70, 90, 100 }, 8, 3, 30, 0.30f, 1.5f);
        Cur = Profiles["sil"];
    }
    void Add(string k, float[] ct, float[] cd, float[] gt, float[] gd, float up, float down, float min, float rise, float hyst) {
        Profiles[k] = new Profile { Key = k, Cpu = new Curve(ct, cd), Gpu = new Curve(gt, gd), Up = up, Down = down, Min = min, Rise = rise, Hyst = hyst };
    }

    public bool CanControl { get { return !Laptop && !PawnIoMissing && !InitFailed && Chans.Count > 0; } }

    public static bool DetectLaptop() {
        try {
            using (var s = new System.Management.ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure"))
                foreach (var o in s.Get()) {
                    var types = o["ChassisTypes"] as ushort[];
                    if (types != null && types.Any(t => t == 8 || t == 9 || t == 10 || t == 11 || t == 14 || t == 30 || t == 31 || t == 32)) return true;
                }
        } catch { }
        return false;
    }
    public static bool PawnIoInstalled() {
        try { return ServiceController.GetDevices().Any(s => string.Equals(s.ServiceName, "PawnIO", StringComparison.OrdinalIgnoreCase)); }
        catch { return true; }
    }

    public void Init(string cooling) {
        Laptop = DetectLaptop();
        PawnIoMissing = !PawnIoInstalled();
        try {
            Comp = new Computer { IsCpuEnabled = true, IsGpuEnabled = true, IsMotherboardEnabled = true };
            Comp.Open();
            float bestGpu = -1;
            foreach (var h in Comp.Hardware) {
                h.Update(); foreach (var s in h.SubHardware) s.Update();
                if (h.HardwareType == HardwareType.Cpu && cpuHw == null) cpuHw = h;
                if (h.HardwareType == HardwareType.Motherboard) mbHw = h;
                if (h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel) {
                    float score = 0;
                    if (h.Sensors.Any(x => x.SensorType == SensorType.Fan)) score += 100;
                    if (h.HardwareType == HardwareType.GpuNvidia) score += 50;
                    string n = h.Name;
                    if (n.Contains("(TM) Graphics") || n.Contains("UHD") || n.Contains("Iris") || n.EndsWith(" Graphics")) score -= 80;
                    if (score > bestGpu) { bestGpu = score; gpuHw = h; }
                }
            }
            if (cpuHw != null) {
                CpuName = cpuHw.Name;
                Intel = CpuName.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0;
                if (Intel) { CpuCrit = 98; CpuHotAlert = 95; CpuWarn = 86; CpuCalmMax = 82; }
                var temps = cpuHw.Sensors.Where(x => x.SensorType == SensorType.Temperature && !x.Name.Contains("Distance")).ToList();
                cpuT = temps.FirstOrDefault(x => x.Name.Contains("Tctl")) ?? temps.FirstOrDefault(x => x.Name.Contains("Package"))
                    ?? temps.FirstOrDefault(x => x.Name.Contains("Core Max")) ?? temps.FirstOrDefault();
                cpuP = cpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Power && x.Name.Contains("Package"));
                cpuL = cpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Load && x.Name.Contains("Total"));
            }
            if (gpuHw != null) {
                GpuName = gpuHw.Name;
                gpuT = gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Temperature && x.Name.Contains("Hot Spot"));
                GpuHotspot = gpuT != null;
                if (gpuT == null) gpuT = gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Temperature && x.Name.Contains("Core"))
                    ?? gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Temperature);
                gpuP = gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Power && x.Name.Contains("Package"))
                    ?? gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Power);
                gpuL = gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Load && x.Name == "GPU Core")
                    ?? gpuHw.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Load && x.Name.Contains("3D"));
            }
            if (mbHw != null) {
                BoardName = mbHw.Name;
                if (!Laptop)
                    foreach (var sio in mbHw.SubHardware)
                        foreach (var c in sio.Sensors.Where(x => x.SensorType == SensorType.Control && x.Control != null)) {
                            string id = c.Identifier.ToString();
                            string fid = id.Replace("/control/", "/fan/");
                            var rpm = sio.Sensors.FirstOrDefault(x => x.SensorType == SensorType.Fan && x.Identifier.ToString() == fid);
                            string ln = c.Name.ToLowerInvariant();
                            var ch = new Channel {
                                Id = id, Name = c.Name, Ctrl = c, Rpm = rpm, WasSpinning = Rpm(rpm) > 100,
                                PumpLike = ln.Contains("pump") || ln.Contains("aio") || ln.Contains("water"),
                                CpuLike = ln.Contains("cpu")
                            };
                            string v = cfg.Get("role." + id, null);
                            Role r;
                            if (v != null && Enum.TryParse(v, out r)) ch.Role = r;
                            else ch.Role = DefaultRole(ch, cooling);
                            float fb;
                            if (float.TryParse(cfg.Get("floor." + id, ""), NumberStyles.Float, CultureInfo.InvariantCulture, out fb)) ch.FloorBoost = fb;
                            Chans.Add(ch);
                        }
            }
        } catch (Exception ex) { InitFailed = true; InitError = ex.Message; }
    }

    // Par sécurité : une pompe ne doit jamais être ralentie. Avec un watercooling (ou si on ne sait pas),
    // les connecteurs CPU restent aussi au BIOS, car la pompe y est souvent branchée.
    public static Role DefaultRole(Channel ch, string cooling) {
        if (!ch.WasSpinning || ch.PumpLike) return Role.Bios;
        if (ch.CpuLike) return cooling == "air" ? Role.Cpu : Role.Bios;
        return Role.CpuGpu;
    }
    public bool Risky(Channel ch, string cooling) { return ch.PumpLike || (ch.CpuLike && cooling != "air"); }

    static float? Temp(ISensor s) {
        if (s == null || !s.Value.HasValue) return null;
        float v = s.Value.Value;
        if (float.IsNaN(v) || v <= -30 || v > 150) return null;
        return v;
    }
    static float? Num(ISensor s, float max) {
        if (s == null || !s.Value.HasValue || float.IsNaN(s.Value.Value) || s.Value.Value < 0 || s.Value.Value > max) return null;
        return s.Value.Value;
    }
    public static float Rpm(ISensor s) { float? v = Num(s, 20000); return v.HasValue ? v.Value : 0; }

    public void SetMode(string key) {
        if (key != "auto" && !Profiles.ContainsKey(key)) key = "sil";
        Mode = key; AutoHeavy = false; heavyStart = lightStart = DateTime.MinValue; Boost = false; calmHotSec = calmCoolSec = 0;
        Cur = key == "auto" ? Profiles["sil"] : Profiles[key];
        if (ModeChanged != null) ModeChanged();
    }

    // Température GPU ramenée à l'échelle "point chaud" pour les courbes et les seuils
    public float? GpuHot { get { return GpuTemp.HasValue ? GpuTemp.Value + (GpuHotspot ? 0 : 12) : (float?)null; } }

    public Level CpuLevel { get {
        if (!CpuTemp.HasValue) return Level.Idle;
        float ok = Intel ? 80 : 80, warm = Intel ? 95 : 90;
        return CpuTemp.Value < ok ? Level.Ok : CpuTemp.Value < warm ? Level.Warm : Level.Hot; } }
    public Level GpuLevel { get {
        float? g = GpuHot; if (!g.HasValue) return Level.Idle;
        return g.Value < 90 ? Level.Ok : g.Value < 100 ? Level.Warm : Level.Hot; } }
    public Level Overall { get {
        if (Emergency) return Level.Hot;
        var a = CpuLevel; var b = GpuLevel;
        if (a == Level.Idle && b == Level.Idle) return Level.Idle;
        if (a == Level.Hot || b == Level.Hot) return Level.Hot;
        if (a == Level.Warm || b == Level.Warm) return Level.Warm;
        return Level.Ok; } }

    // Données fictives pour l'aperçu des fenêtres (--preview), sans toucher au matériel
    public void Fake() {
        CpuName = "AMD Ryzen 5 9600X 6-Core"; GpuName = "AMD Radeon RX 7900 GRE"; BoardName = "ASRock X870 Steel Legend WiFi";
        CpuTemp = 64; GpuTemp = 71; CpuS = 64; GpuS = 71; GpuHotspot = true; AvgDuty = 34;
        string[] n = { "CPU Fan", "Pump Fan", "System Fan #2", "System Fan #3" };
        for (int i = 0; i < n.Length; i++)
            Chans.Add(new Channel { Id = "fake" + i, Name = n[i], Role = i == 0 ? Role.Cpu : i == 1 ? Role.Bios : Role.CpuGpu, Duty = 34, Controlled = i != 1, WasSpinning = true });
    }

    public void Tick() {
        if (InitFailed || App.Preview) return;
        try {
            if (cpuHw != null) cpuHw.Update();
            if (gpuHw != null) gpuHw.Update();
            if (mbHw != null) { mbHw.Update(); foreach (var s in mbHw.SubHardware) s.Update(); }
        } catch { }

        CpuTemp = Temp(cpuT); GpuTemp = Temp(gpuT);
        CpuW = Num(cpuP, 1000); GpuW = Num(gpuP, 2000); CpuLoad = Num(cpuL, 100); GpuLoad = Num(gpuL, 100);
        float? gh = GpuHot;
        float rise = Eff.Rise;
        if (CpuTemp.HasValue) CpuS = float.IsNaN(CpuS) ? CpuTemp.Value : CpuS + (CpuTemp.Value > CpuS ? rise : 0.08f) * (CpuTemp.Value - CpuS);
        if (gh.HasValue) GpuS = float.IsNaN(GpuS) ? gh.Value : GpuS + (gh.Value > GpuS ? rise : 0.08f) * (gh.Value - GpuS);
        SensorLost = !CpuTemp.HasValue;
        Emergency = (CpuTemp.HasValue && CpuTemp.Value >= CpuCrit) || (gh.HasValue && gh.Value >= GpuCrit);
        if (Mode == "auto") AutoSwitch();
        Guards(gh);

        // Garde-fou actif (chauffe rapide ou zone d'alerte) : on suit la vraie température sans lissage,
        // au moins avec la courbe Normal, et les ventilateurs accélèrent vite.
        var P = Eff; var E = Profiles["eq"];
        bool guard = Fast || WarnZone;
        float cin = guard && CpuTemp.HasValue ? Math.Max(CpuS, CpuTemp.Value) : CpuS;
        float gin = guard && gh.HasValue ? Math.Max(GpuS, gh.Value) : GpuS;
        float cT = P.Cpu.Eval(cin); if (guard) cT = Math.Max(cT, E.Cpu.Eval(cin));
        float gT = gh.HasValue ? (guard ? Math.Max(P.Gpu.Eval(gin), E.Gpu.Eval(gin)) : P.Gpu.Eval(gin)) : cT;
        float up = guard ? Math.Max(P.Up, 10) : P.Up;

        float sum = 0; int n = 0;
        if (CanControl)
            foreach (var ch in Chans) {
                float rpm = Rpm(ch.Rpm);
                try {
                    if (Paused || ch.Role == Role.Bios) {
                        if (ch.Controlled) { ch.Ctrl.Control.SetDefault(); ch.Controlled = false; ch.LastSent = -1; }
                        ch.DeadSec = 0; ch.Stall = 0;
                        continue;
                    }
                    float target = ch.Role == Role.Cpu ? cT : ch.Role == Role.Gpu ? gT : Math.Max(cT, gT);
                    float floor = Math.Min(100, P.Min + ch.FloorBoost);
                    target = Math.Min(100, Math.Max(target, floor));
                    if (!ch.Controlled) { float? cv = Num(ch.Ctrl, 100); ch.Duty = Math.Max(floor, cv.HasValue && cv.Value > 0 ? cv.Value : 60); }

                    if (SensorLost || Emergency) ch.Duty = 100;
                    else if (target > ch.Duty) ch.Duty = Math.Min(target, ch.Duty + up);
                    else if (target < ch.Duty - P.Hyst) ch.Duty = Math.Max(target, ch.Duty - P.Down);

                    // ventilateur qui cale à bas régime : on relève son minimum
                    if (ch.Controlled && ch.WasSpinning && rpm < 50 && ch.Duty < 100) {
                        if (++ch.Stall >= 6) {
                            ch.FloorBoost = Math.Min(60, ch.FloorBoost + 10);
                            ch.Duty = Math.Max(ch.Duty, Math.Min(100, P.Min + ch.FloorBoost));
                            ch.Stall = 0;
                            cfg.Set("floor." + ch.Id, ch.FloorBoost.ToString(CultureInfo.InvariantCulture));
                        }
                    } else ch.Stall = 0;
                    // ventilateur en panne
                    if (ch.Controlled && ch.WasSpinning && rpm < 50 && ch.Duty >= 40) {
                        if (++ch.DeadSec == 15)
                            Alert("fan." + ch.Id, App.T("Ventilateur en panne ?", "Fan failure?"),
                                App.T("{0} ne tourne plus. Vérifie son câble ou le ventilateur.", "{0} stopped spinning. Check its cable or the fan.", ch.Name), true);
                    } else ch.DeadSec = 0;

                    if (Math.Abs(ch.Duty - ch.LastSent) >= 0.5f || (DateTime.Now - ch.LastSendTime).TotalSeconds > 5) {
                        ch.Ctrl.Control.SetSoftware((float)Math.Round(ch.Duty, 1));
                        ch.LastSent = ch.Duty; ch.LastSendTime = DateTime.Now; ch.Controlled = true;
                    }
                    sum += ch.Duty; n++;
                } catch { }
            }
        AvgDuty = n > 0 ? sum / n : float.NaN;
        Track();
    }

    // Garde-fous :
    //  1. chauffe rapide : +8 °C en 5 s (CPU, au-dessus de 70 °C) ou +10 °C (GPU, au-dessus de 80 °C)
    //     -> réaction immédiate pendant 30 s ;
    //  2. zone d'alerte : CPU >= CpuWarn ou GPU >= GpuWarn -> au moins la courbe Normal, sans lissage ;
    //  3. renfort du mode Calme : chaud pendant 2 min -> Normal, retour au Calme après 1 min au frais.
    void Guards(float? gh) {
        DateTime now = DateTime.Now;
        bool r1 = Rising(histC, CpuTemp, 8, 70), r2 = Rising(histG, gh, 10, 80);
        if (r1 || r2) fastUntil = now.AddSeconds(30);
        Fast = now < fastUntil;
        WarnZone = (CpuTemp.HasValue && CpuTemp.Value >= CpuWarn) || (gh.HasValue && gh.Value >= GpuWarn);

        if (Cur.Key != "sil") { Boost = false; calmHotSec = calmCoolSec = 0; return; }
        bool hot = CpuS >= CpuCalmMax || (!float.IsNaN(GpuS) && GpuS >= GpuCalmMax);
        bool cool = CpuS < CpuCalmMax - 8 && (float.IsNaN(GpuS) || GpuS < GpuCalmMax - 8);
        if (!Boost) {
            calmCoolSec = 0;
            if (!hot) { calmHotSec = 0; return; }
            if (++calmHotSec >= 120) {
                Boost = true; calmHotSec = 0;
                Alert("renfort", App.T("Le mode Calme se renforce", "Quiet mode is boosting"),
                    App.T("Ton PC chauffe depuis 2 minutes : je passe en Normal le temps qu'il refroidisse.", "Your PC has been warm for 2 minutes: switching to Normal until it cools down."), false);
                if (ModeChanged != null) ModeChanged();
            }
        } else {
            calmHotSec = 0;
            if (!cool) { calmCoolSec = 0; return; }
            if (++calmCoolSec >= 60) { Boost = false; calmCoolSec = 0; if (ModeChanged != null) ModeChanged(); }
        }
    }
    static bool Rising(Queue<float> q, float? v, float delta, float min) {
        if (!v.HasValue) { q.Clear(); return false; }
        q.Enqueue(v.Value); while (q.Count > 6) q.Dequeue();
        return v.Value >= min && v.Value - q.Min() >= delta;
    }

    void AutoSwitch() {
        DateTime now = DateTime.Now;
        bool heavy = (GpuLoad.HasValue && GpuLoad.Value >= 75) || (CpuLoad.HasValue && CpuLoad.Value >= 60);
        bool light = (!GpuLoad.HasValue || GpuLoad.Value < 40) && (!CpuLoad.HasValue || CpuLoad.Value < 30);
        if (!AutoHeavy) {
            lightStart = DateTime.MinValue;
            if (!heavy) { heavyStart = DateTime.MinValue; return; }
            if (heavyStart == DateTime.MinValue) heavyStart = now;
            if ((now - heavyStart).TotalSeconds >= 30) { AutoHeavy = true; Cur = Profiles["eq"]; if (ModeChanged != null) ModeChanged(); }
        } else {
            heavyStart = DateTime.MinValue;
            if (!light) { lightStart = DateTime.MinValue; return; }
            if (lightStart == DateTime.MinValue) lightStart = now;
            if ((now - lightStart).TotalSeconds >= 60) { AutoHeavy = false; Cur = Profiles["sil"]; if (ModeChanged != null) ModeChanged(); }
        }
    }

    void Track() {
        if (CpuTemp.HasValue) { SumCpu += CpuTemp.Value; NCpu++; if (float.IsNaN(MaxCpu) || CpuTemp.Value > MaxCpu) MaxCpu = CpuTemp.Value; }
        if (GpuTemp.HasValue && (float.IsNaN(MaxGpu) || GpuTemp.Value > MaxGpu)) MaxGpu = GpuTemp.Value;
        foreach (var ch in Chans) if (ch.Controlled && ch.Duty > MaxDuty) MaxDuty = ch.Duty;
        if (CanControl && !Paused) { string k = Eff.Key; if (k == "sil") SecSil++; else if (k == "eq") SecEq++; else SecPerf++; }

        if (Emergency && !wasEmergency) {
            Emergencies++;
            Alert("urgence", App.T("Trop chaud !", "Too hot!"), App.T("Les ventilateurs passent à fond pour refroidir ton PC.", "Fans set to full speed to cool your PC down."), true);
        }
        wasEmergency = Emergency;
        if (SensorLost) { if (++lostSec == 5) Alert("capteur", App.T("Capteur perdu", "Sensor lost"), App.T("La température du processeur n'est plus lisible : ventilateurs à fond par sécurité.", "CPU temperature can't be read: fans at full speed for safety."), true); }
        else lostSec = 0;
        if (CpuTemp.HasValue && CpuTemp.Value >= CpuHotAlert) { if (++hotSec == 20) Alert("cpuchaud", App.T("Processeur très chaud", "CPU very hot"), App.T("Essaie le mode Fort, ou vérifie le ventilateur du processeur.", "Try Strong mode, or check the CPU cooler."), false); }
        else hotSec = 0;
        if (CpuTemp.HasValue && CpuLoad.HasValue && CpuTemp.Value >= 75 && CpuLoad.Value < 10) {
            if (++idleHotSec == 60) Alert("anormal", App.T("Refroidissement à vérifier", "Check your cooling"), App.T("Le processeur chauffe alors qu'il ne fait presque rien. Poussière ou ventilateur mal fixé ?", "The CPU is hot while almost idle. Dust or a loose cooler?"), false);
        } else idleHotSec = 0;
    }

    readonly Dictionary<string, DateTime> lastAlert = new Dictionary<string, DateTime>();
    void Alert(string key, string title, string msg, bool critical) {
        DateTime last;
        if (lastAlert.TryGetValue(key, out last) && (DateTime.Now - last).TotalMinutes < 10) return;
        lastAlert[key] = DateTime.Now; AlertsCount++;
        if (Alerted != null) Alerted(title, msg, critical);
    }

    public void RestoreAll() {
        foreach (var ch in Chans) { try { if (ch.Controlled) { ch.Ctrl.Control.SetDefault(); ch.Controlled = false; } } catch { } }
    }
    public void Close() { RestoreAll(); try { if (Comp != null) Comp.Close(); } catch { } }
}

// ====================================================================== nouveautés (nouvelle version, messages)
// Une fois par jour, l'appli lit le petit fichier public news.json sur le site de Chutoki.
// Rien n'est envoyé sur l'utilisateur (pas d'identifiant, pas de statistiques).
// Exemple de news.json :
// { "latest": "1.1.0", "url": "https://slimouzz.github.io/chutoki/",
//   "message": { "id": "2026-11-20", "text": { "fr": "Coucou !", "en": "Hi!" }, "url": "https://..." } }
class News {
    public string Latest, Url, MsgId, MsgText, MsgUrl;
    public bool HasUpdate { get { return Newer(Latest, App.Version); } }

    public static bool Newer(string a, string b) {
        Version x, y;
        return a != null && Version.TryParse(a, out x) && Version.TryParse(b, out y) && x > y;
    }
    // On n'ouvre que des liens https, pour ne jamais lancer autre chose qu'une page web
    static string SafeUrl(object o) {
        string s = o as string;
        return s != null && s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && s.Length < 500 ? s : null;
    }
    static string Text(object o) {
        string s = o as string; if (s == null) return null;
        s = s.Trim(); return s.Length == 0 ? null : s.Length > 280 ? s.Substring(0, 280) : s;
    }

    public static News Fetch() {
        try {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var req = (HttpWebRequest)WebRequest.Create(App.NewsUrl);
            req.UserAgent = "Chutoki/" + App.Version; req.Timeout = 10000; req.ReadWriteTimeout = 10000;
            req.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            string json;
            using (var resp = req.GetResponse()) using (var sr = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.UTF8)) {
                var buf = new char[32768]; int n = sr.ReadBlock(buf, 0, buf.Length); json = new string(buf, 0, n);
            }
            var o = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
            if (o == null) return null;
            var nw = new News();
            object v;
            if (o.TryGetValue("latest", out v)) nw.Latest = v as string;
            nw.Url = o.TryGetValue("url", out v) ? SafeUrl(v) : null;
            if (nw.Url == null) nw.Url = App.Site;
            Dictionary<string, object> m = o.TryGetValue("message", out v) ? v as Dictionary<string, object> : null;
            if (m != null) {
                nw.MsgId = m.TryGetValue("id", out v) ? Text(v) : null;
                var t = m.TryGetValue("text", out v) ? v as Dictionary<string, object> : null;
                if (t != null) {
                    object s;
                    nw.MsgText = (t.TryGetValue(App.Lang, out s) ? Text(s) : null) ?? (t.TryGetValue("en", out s) ? Text(s) : null) ?? (t.TryGetValue("fr", out s) ? Text(s) : null);
                }
                nw.MsgUrl = m.TryGetValue("url", out v) ? SafeUrl(v) : null;
                if (nw.MsgId == null || nw.MsgText == null) nw.MsgId = nw.MsgText = nw.MsgUrl = null;
            }
            return nw;
        } catch { return null; }
    }
}

// ====================================================================== rapport et journal d'alertes
class Report {
    readonly Engine e; readonly string csv, log;
    List<string> history = new List<string>();
    public readonly List<string> Alerts = new List<string>();
    public DateTime LastSave = DateTime.Now;
    public string Folder { get { return Path.GetDirectoryName(csv); } }
    public static string Header { get { return App.T(
        "Début;Durée (min);CPU moyen (°C);CPU max (°C);GPU max (°C);Ventilateurs max (%);Calme (min);Normal (min);Fort (min);Passages à fond;Alertes",
        "Start;Duration (min);CPU avg (°C);CPU max (°C);GPU max (°C);Fans max (%);Quiet (min);Normal (min);Strong (min);Full-speed events;Alerts"); } }
    public Report(Engine engine, string folder) {
        e = engine; csv = Path.Combine(folder, "rapport.csv"); log = Path.Combine(folder, "alertes.log");
        try {
            if (File.Exists(csv)) history = File.ReadAllLines(csv, System.Text.Encoding.UTF8).Skip(1).Where(l => l.Trim().Length > 0).ToList();
            if (File.Exists(log)) Alerts.AddRange(File.ReadAllLines(log, System.Text.Encoding.UTF8).Reverse().Take(30));
        } catch { }
        if (history.Count > 200) history = history.Skip(history.Count - 200).ToList();
    }
    static string F0(float v) { return float.IsNaN(v) ? "-" : v.ToString("0"); }
    static string M(int s) { return (s / 60.0).ToString("0"); }
    public string Current() {
        return e.SessionStart.ToString("yyyy-MM-dd HH:mm") + ";" + (int)(DateTime.Now - e.SessionStart).TotalMinutes + ";" +
            (e.NCpu > 0 ? (e.SumCpu / e.NCpu).ToString("0") : "-") + ";" + F0(e.MaxCpu) + ";" + F0(e.MaxGpu) + ";" + e.MaxDuty.ToString("0") + ";" +
            M(e.SecSil) + ";" + M(e.SecEq) + ";" + M(e.SecPerf) + ";" + e.Emergencies + ";" + e.AlertsCount;
    }
    public IEnumerable<string> Rows() { yield return Current(); foreach (var l in Enumerable.Reverse(history).Take(50)) yield return l; }
    public void Save() {
        LastSave = DateTime.Now;
        try {
            var lines = new List<string> { Header }; lines.AddRange(history); lines.Add(Current());
            File.WriteAllLines(csv, lines.ToArray(), new System.Text.UTF8Encoding(true));
        } catch { }
    }
    public void AddAlert(string title, string msg) {
        string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + title + " - " + msg;
        Alerts.Insert(0, line); if (Alerts.Count > 50) Alerts.RemoveAt(50);
        try { File.AppendAllText(log, line + Environment.NewLine, System.Text.Encoding.UTF8); } catch { }
    }
}

// ====================================================================== contrôles dessinés
class FaceView : Control {
    public Level Level = Level.Idle;
    public string Title = "", Sub = "";
    public FaceView() { DoubleBuffered = true; SetStyle(ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true); }
    protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Theme.Bg);
        Color c = Level == Level.Ok ? Theme.Ok : Level == Level.Warm ? Theme.Warm : Level == Level.Hot ? Theme.Hot : Theme.Idle;
        float d = Math.Min(Width, Height - 70) - 10; if (d < 60) d = 60;
        var r = new RectangleF((Width - d) / 2, 4, d, d);
        using (var b = new SolidBrush(Color.FromArgb(Theme.Dark ? 45 : 32, c))) g.FillEllipse(b, r);
        using (var p = new Pen(c, d * 0.05f)) g.DrawEllipse(p, r.X + d * 0.03f, r.Y + d * 0.03f, d * 0.94f, d * 0.94f);
        // le bébé de Chutoki : il dort quand tout va bien, se réveille quand ça chauffe, pleure si c'est très chaud
        float cx = r.X + d / 2, cy = r.Y + d / 2;
        float ex = d * 0.17f, ey = cy - d * 0.06f, ew = d * 0.13f;
        using (var pk = new SolidBrush(Color.FromArgb(Theme.Dark ? 90 : 70, 255, 120, 150))) {   // joues roses
            g.FillEllipse(pk, cx - d * 0.30f, cy + d * 0.04f, d * 0.12f, d * 0.08f);
            g.FillEllipse(pk, cx + d * 0.18f, cy + d * 0.04f, d * 0.12f, d * 0.08f);
        }
        using (var p = new Pen(c, d * 0.04f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
        using (var b = new SolidBrush(c)) {
            // mèche de cheveux
            g.DrawBezier(p, cx - d * 0.02f, r.Y + d * 0.20f, cx - d * 0.10f, r.Y + d * 0.08f, cx + d * 0.10f, r.Y + d * 0.06f, cx + d * 0.04f, r.Y + d * 0.17f);
            bool sleep = Level == Level.Ok || Level == Level.Idle;
            if (sleep) {
                // yeux fermés
                g.DrawArc(p, cx - ex - ew / 2, ey - ew * 0.35f, ew, ew * 0.6f, 20, 140);
                g.DrawArc(p, cx + ex - ew / 2, ey - ew * 0.35f, ew, ew * 0.6f, 20, 140);
            } else {
                float eye = d * (Level == Level.Hot ? 0.10f : 0.08f);
                g.FillEllipse(b, cx - ex - eye / 2, ey - eye / 2, eye, eye);
                g.FillEllipse(b, cx + ex - eye / 2, ey - eye / 2, eye, eye);
            }
            float mw = d * 0.20f, my = cy + d * 0.16f;
            if (Level == Level.Ok) g.DrawArc(p, cx - mw / 2, my - mw * 0.45f, mw, mw * 0.55f, 20, 140);
            else if (Level == Level.Hot) g.FillEllipse(b, cx - d * 0.06f, my - d * 0.03f, d * 0.12f, d * 0.13f);
            else g.DrawLine(p, cx - mw / 2.6f, my, cx + mw / 2.6f, my);
            if (Level == Level.Hot)   // larme
                using (var tb = new SolidBrush(Color.FromArgb(120, 180, 255))) g.FillEllipse(tb, cx + ex - d * 0.02f, ey + d * 0.07f, d * 0.05f, d * 0.08f);
            if (Level == Level.Warm)  // goutte de sueur
                using (var tb = new SolidBrush(Color.FromArgb(120, 180, 255))) g.FillEllipse(tb, cx + d * 0.30f, cy - d * 0.20f, d * 0.05f, d * 0.08f);
        }
        if (Level == Level.Ok)   // zzz
            using (var zf = new Font("Segoe UI", d * 0.09f, FontStyle.Bold, GraphicsUnit.Pixel)) using (var zb = new SolidBrush(c)) {
                g.DrawString("z", zf, zb, r.Right - d * 0.22f, r.Y + d * 0.14f);
                g.DrawString("Z", zf, zb, r.Right - d * 0.14f, r.Y + d * 0.04f);
            }
        using (var f = Theme.Semi(17f)) using (var b = new SolidBrush(Theme.Txt)) {
            var sz = g.MeasureString(Title, f); g.DrawString(Title, f, b, (Width - sz.Width) / 2, r.Bottom + 8);
        }
        using (var f = Theme.F(10f)) using (var b = new SolidBrush(Theme.Dim)) {
            var sz = g.MeasureString(Sub, f); g.DrawString(Sub, f, b, (Width - sz.Width) / 2, r.Bottom + 42);
        }
    }
}

class ModeButton : Control {
    public string Key, Title, Sub; public bool Selected; bool hover;
    public ModeButton() { DoubleBuffered = true; Cursor = Cursors.Hand; SetStyle(ControlStyles.ResizeRedraw, true); }
    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }
    protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.Clear(Theme.Bg);
        var r = new RectangleF(1, 1, Width - 3, Height - 3);
        Color fill = Selected ? Theme.Accent : hover && Enabled ? Theme.CardHover : Theme.Card;
        using (var path = Theme.Round(r, 14)) {
            using (var b = new SolidBrush(fill)) g.FillPath(b, path);
            if (!Selected) using (var p = new Pen(Theme.Line, 1.2f)) g.DrawPath(p, path);
        }
        Color t = !Enabled ? Theme.Dim : Selected ? Theme.AccentTxt : Theme.Txt;
        Color s = !Enabled ? Theme.Dim : Selected ? Color.FromArgb(225, 235, 255) : Theme.Dim;
        using (var f = Theme.Semi(14f)) using (var b = new SolidBrush(t)) g.DrawString(Title, f, b, 16, 12);
        using (var f = Theme.F(9f)) using (var b = new SolidBrush(s)) g.DrawString(Sub, f, b, 17, 44);
    }
}

class GraphView : Control {
    public readonly List<float> Cpu = new List<float>(), Gpu = new List<float>(), Duty = new List<float>();
    const int Max = 300;
    public GraphView() { DoubleBuffered = true; SetStyle(ControlStyles.ResizeRedraw, true); }
    public void Push(float c, float g, float d) {
        Cpu.Add(c); Gpu.Add(g); Duty.Add(d);
        if (Cpu.Count > Max) { Cpu.RemoveAt(0); Gpu.RemoveAt(0); Duty.RemoveAt(0); }
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e) {
        var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Theme.Card);
        var a = new RectangleF(34, 24, Width - 46, Height - 34);
        using (var pen = new Pen(Theme.Line)) using (var f = Theme.F(8f)) using (var tb = new SolidBrush(Theme.Dim)) {
            for (int v = 20; v <= 100; v += 20) { float y = a.Bottom - a.Height * v / 100f; g.DrawLine(pen, a.Left, y, a.Right, y); g.DrawString(v.ToString(), f, tb, 6, y - 7); }
            g.DrawString(App.T("5 dernières minutes (°C et %)", "Last 5 minutes (°C and %)"), f, tb, 34, 4);
        }
        Line(g, Cpu, Theme.Warm, a); Line(g, Gpu, Theme.Hot, a); Line(g, Duty, Theme.Accent, a);
        using (var f = Theme.F(8.5f, FontStyle.Bold)) {
            float x = Width - 330;
            Leg(g, f, App.T("Processeur", "CPU"), Theme.Warm, ref x);
            Leg(g, f, App.T("Carte graphique", "Graphics card"), Theme.Hot, ref x);
            Leg(g, f, App.T("Ventilateurs", "Fans"), Theme.Accent, ref x);
        }
    }
    static void Leg(Graphics g, Font f, string s, Color c, ref float x) {
        using (var b = new SolidBrush(c)) { g.FillRectangle(b, x, 9, 10, 3); g.DrawString(s, f, b, x + 13, 3); }
        x += g.MeasureString(s, f).Width + 22;
    }
    static void Line(Graphics g, List<float> d, Color c, RectangleF a) {
        var pts = new List<PointF>();
        for (int i = 0; i < d.Count; i++) {
            if (float.IsNaN(d[i])) continue;
            pts.Add(new PointF(a.Right - (d.Count - 1 - i) * a.Width / (Max - 1), a.Bottom - a.Height * Math.Max(0, Math.Min(100, d[i])) / 100f));
        }
        if (pts.Count > 1) using (var p = new Pen(c, 2f)) g.DrawLines(p, pts.ToArray());
    }
}

// ====================================================================== premier lancement
class WelcomeForm : Form {
    public string Cooling = "unknown";
    public WelcomeForm(string current) {
        if (current == "air" || current == "water") Cooling = current;
        Theme.Prepare(this, 480, 580, App.Name + " " + App.Edition);
        StartPosition = FormStartPosition.CenterScreen; MinimizeBox = false;
        int y = 22;
        Controls.Add(new Label { Text = App.T("Bienvenue !", "Welcome!"), Left = 28, Top = y, AutoSize = true, Font = Theme.Semi(20f) });
        y += 50;
        Controls.Add(new Label { Left = 28, Top = y, Width = 424, Height = 48, ForeColor = Theme.Dim,
            Text = App.T("Chutoki, le ventilo qui fait chut : ton PC devient silencieux tout en restant bien au frais.",
                         "Chutoki, the fan that goes shhh: your PC gets quiet while staying nice and cool.") });
        y += 54;
        Controls.Add(new Label { Left = 28, Top = y, Width = 424, Height = 122,
            Text = App.T("•  Il règle tout seul la vitesse des ventilateurs.\n•  Si ça chauffe vite ou trop, il réagit tout de suite.\n•  Quand tu le fermes, ton PC reprend ses réglages habituels.",
                         "•  It adjusts your fan speeds automatically.\n•  If things heat up fast or too much, it reacts right away.\n•  When you close it, your PC goes back to its usual settings.") });
        y += 128;
        Controls.Add(new Label { Text = App.T("Ton processeur est refroidi par :", "Your CPU is cooled by:"), Left = 28, Top = y, AutoSize = true, Font = Theme.Semi(11f) });
        y += 32;
        var rAir = new RadioButton { Text = App.T("De l'air (un ventilateur sur le processeur)", "Air (a fan on the CPU)"), Left = 36, Top = y, AutoSize = true };
        var rWater = new RadioButton { Text = App.T("Du watercooling (avec une pompe)", "Water cooling (with a pump)"), Left = 36, Top = y + 30, AutoSize = true };
        var rUnk = new RadioButton { Text = App.T("Je ne sais pas", "I don't know"), Left = 36, Top = y + 60, AutoSize = true };
        (Cooling == "air" ? rAir : Cooling == "water" ? rWater : rUnk).Checked = true;
        rAir.CheckedChanged += delegate { if (rAir.Checked) Cooling = "air"; };
        rWater.CheckedChanged += delegate { if (rWater.Checked) Cooling = "water"; };
        rUnk.CheckedChanged += delegate { if (rUnk.Checked) Cooling = "unknown"; };
        Controls.Add(rAir); Controls.Add(rWater); Controls.Add(rUnk);
        y += 104;
        var cb = new CheckBox { Left = 28, Top = y, Width = 424, Height = 60,
            Text = App.T("J'ai compris : ce logiciel gratuit est fourni tel quel, sans garantie, et je l'utilise sous ma responsabilité.",
                         "I understand: this free software is provided as is, without warranty, and I use it at my own risk.") };
        Controls.Add(cb);
        var go = Theme.Btn(App.T("C'est parti !", "Let's go!"), 28, 512, 424, 46, true);
        go.Enabled = false; go.BackColor = Theme.Line;
        cb.CheckedChanged += delegate { go.Enabled = cb.Checked; go.BackColor = cb.Checked ? Theme.Accent : Theme.Line; };
        go.Click += delegate { DialogResult = DialogResult.OK; Close(); };
        Controls.Add(go);
        AcceptButton = go;
    }
}

// ====================================================================== à propos
class AboutForm : Form {
    public AboutForm() {
        Theme.Prepare(this, 460, 770, App.T("À propos", "About"));
        StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; ShowInTaskbar = false;
        int y = 22;
        var lName = new Label { Text = App.Name, Left = 28, Top = y, AutoSize = true, Font = Theme.Semi(20f) };
        Controls.Add(lName);
        Controls.Add(new Label { Text = App.Edition, Left = 28 + lName.PreferredWidth + 6, Top = y + 12, AutoSize = true, Font = Theme.Semi(10f), ForeColor = Theme.Accent });
        y += 46;
        Controls.Add(new Label { Text = App.T("Version ", "Version ") + App.Version + App.T(" — gratuite", " — free"), Left = 28, Top = y, AutoSize = true, ForeColor = Theme.Dim });
        y += 34;
        Controls.Add(new Label { Text = App.T("Créé par ", "Made by ") + App.Author, Left = 28, Top = y, AutoSize = true, Font = Theme.Semi(11f) });
        y += 28;
        var mail = new LinkLabel { Text = App.Email, Left = 28, Top = y, AutoSize = true, LinkColor = Theme.Accent, ActiveLinkColor = Theme.Accent };
        mail.LinkClicked += delegate { App.Open("mailto:" + App.Email + "?subject=" + Uri.EscapeDataString(App.Name + " " + App.Version)); };
        Controls.Add(mail);
        y += 38;
        Controls.Add(new Label { Text = App.T("Pourquoi Chutoki ?", "Why Chutoki?"), Left = 28, Top = y, AutoSize = true, Font = Theme.Semi(11f) });
        y += 28;
        Controls.Add(new Label { Left = 28, Top = y, Width = 404, Height = 130, Font = Theme.F(9.5f),
            Text = App.T(Story.Fr, Story.En) });
        y += 138;
        Controls.Add(new Label { Left = 28, Top = y, Width = 404, Height = 262, ForeColor = Theme.Dim, Font = Theme.F(8.5f),
            Text = App.T(
                "Ce logiciel est gratuit, sans pub, et fourni « tel quel », sans aucune garantie. Il modifie la vitesse des ventilateurs de ta carte mère : tu l'utilises sous ta responsabilité. " +
                "Des garde-fous font réagir les ventilateurs si ça chauffe, et ton PC reprend ses réglages quand l'appli est fermée.",
                "This software is free, ad-free, and provided \"as is\", without any warranty. It changes your motherboard fan speeds: you use it at your own risk. " +
                "Safeguards make the fans react if things heat up, and your PC returns to its own settings when the app is closed.") + "\n\n" +
              App.T(
                "Vie privée : aucune donnée personnelle n'est collectée. Une fois par jour, l'appli lit un petit fichier sur le site de Chutoki pour savoir s'il y a une nouvelle version ou un message (désactivable dans ⚙) ; rien sur toi n'est envoyé. Tes réglages et rapports restent sur ton PC (%APPDATA%\\Chutoki).",
                "Privacy: no personal data is collected. Once a day, the app reads a small file on the Chutoki website to check for a new version or a message (can be turned off in ⚙); nothing about you is sent. Your settings and reports stay on your PC (%APPDATA%\\Chutoki).") + "\n\n" +
              App.T(
                "Composants utilisés : LibreHardwareMonitor (MPL-2.0), pilote PawnIO (namazso), HidSharp (Apache-2.0), bibliothèques .NET (MIT) et autres — détails dans « Composants tiers ».",
                "Components: LibreHardwareMonitor (MPL-2.0), PawnIO driver (namazso), HidSharp (Apache-2.0), .NET libraries (MIT) and others — see \"Third-party\".") });
        var bLic = Theme.Btn(App.T("Licence", "License"), 28, 714, 120);
        bLic.Click += delegate { App.Open(Path.Combine(App.Dir, "LICENSE.txt")); };
        var b3 = Theme.Btn(App.T("Composants tiers", "Third-party"), 156, 714, 150);
        b3.Click += delegate { App.Open(Path.Combine(App.Dir, "THIRD-PARTY-NOTICES.txt")); };
        var bOk = Theme.Btn("OK", 332, 714, 100, 34, true);
        bOk.Click += delegate { Close(); };
        Controls.Add(bLic); Controls.Add(b3); Controls.Add(bOk);
        AcceptButton = bOk;
    }
}

// ====================================================================== réglages avancés
class AdvancedForm : Form {
    readonly Engine e; readonly Settings cfg; readonly Report rep; readonly string cooling;
    readonly DataGridView grid; public readonly GraphView Graph; readonly Button bPause; readonly Label lInfo;
    static readonly string[] RoleNames = {
        App.T("Ne pas toucher (BIOS)", "Don't touch (BIOS)"), App.T("Processeur", "CPU"),
        App.T("Processeur + carte graphique", "CPU + graphics card"), App.T("Carte graphique", "Graphics card") };
    bool loading;

    public AdvancedForm(Engine engine, Settings s, Report r, string cool) {
        e = engine; cfg = s; rep = r; cooling = cool;
        Theme.Prepare(this, 760, 668, App.T("Réglages avancés", "Advanced settings"));
        StartPosition = FormStartPosition.CenterScreen;

        lInfo = new Label { Left = 20, Top = 14, Width = 720, Height = 40, ForeColor = Theme.Dim, Font = Theme.F(9f) };
        Controls.Add(lInfo);
        Controls.Add(new Label { Text = App.T("Ventilateurs", "Fans"), Left = 20, Top = 58, AutoSize = true, Font = Theme.Semi(11f) });
        var lTest = new LinkLabel { Text = "📨 " + App.T("Envoyer un rapport de test", "Send a test report"), AutoSize = false, Left = 400, Top = 58, Width = 340, Height = 24,
            TextAlign = ContentAlignment.MiddleRight, LinkColor = Theme.Accent, ActiveLinkColor = Theme.Accent, Font = Theme.F(9.5f) };
        lTest.LinkClicked += delegate { ShowTestReport(); };
        Controls.Add(lTest);

        grid = new DataGridView {
            Left = 20, Top = 86, Width = 720, Height = 230, BackgroundColor = Theme.Card, BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, EnableHeadersVisualStyles = false, EditMode = DataGridViewEditMode.EditOnEnter,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, ColumnHeadersHeight = 32,
            GridColor = Theme.Line, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        };
        StyleGrid(grid);
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = App.T("Connecteur", "Header"), ReadOnly = true, Width = 170 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = App.T("Vitesse (tr/min)", "Speed (RPM)"), ReadOnly = true, Width = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = App.T("Puissance", "Power"), ReadOnly = true, Width = 130 });
        var cc = new DataGridViewComboBoxColumn { HeaderText = App.T("Suit la température de", "Follows the temperature of"), Width = 288, FlatStyle = FlatStyle.Flat };
        cc.Items.AddRange(RoleNames);
        grid.Columns.Add(cc);
        loading = true;
        foreach (var ch in e.Chans) { int i = grid.Rows.Add(ch.Name, "", "", RoleNames[(int)ch.Role]); grid.Rows[i].Tag = ch; }
        loading = false;
        grid.CurrentCellDirtyStateChanged += delegate { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        grid.CellValueChanged += OnRoleChanged;
        grid.DataError += (o, a) => { a.ThrowException = false; };
        Controls.Add(grid);

        Graph = new GraphView { Left = 20, Top = 330, Width = 720, Height = 220 };
        Controls.Add(Graph);

        var bRep = Theme.Btn(App.T("Rapport", "Report"), 20, 570, 130);
        bRep.Click += delegate { ShowReport(); };
        bPause = Theme.Btn("", 160, 570, 200);
        bPause.Click += delegate { e.Paused = !e.Paused; RefreshNow(); };
        var bPlan = Theme.Btn(App.T("Plan Windows équilibré", "Windows balanced plan"), 370, 570, 230);
        bPlan.Click += delegate {
            int c; App.Run("powercfg", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e", out c);
            MessageBox.Show(this, c == 0 ? App.T("Plan « Utilisation normale » activé : ton processeur chauffera moins au repos.", "Balanced power plan enabled: your CPU will run cooler when idle.")
                                         : App.T("Impossible de changer le plan d'alimentation.", "Could not change the power plan."), App.Name);
        };
        var cbTray = new CheckBox { Text = App.T("La croix garde l'appli près de l'horloge", "Closing keeps the app near the clock"), Left = 20, Top = 612, AutoSize = true, ForeColor = Theme.Dim, Font = Theme.F(9f),
            Checked = cfg.Get("tray", "1") == "1" };
        cbTray.CheckedChanged += delegate { cfg.Set("tray", cbTray.Checked ? "1" : "0"); };
        var cbNews = new CheckBox { Text = App.T("Rechercher les nouveautés (une fois par jour)", "Check for news once a day"), Left = 20, Top = 638, AutoSize = true, ForeColor = Theme.Dim, Font = Theme.F(9f),
            Checked = cfg.Get("news", "1") == "1" };
        cbNews.CheckedChanged += delegate { cfg.Set("news", cbNews.Checked ? "1" : "0"); };
        Controls.Add(cbNews);
        var bClose = Theme.Btn(App.T("Fermer", "Close"), 610, 570, 130, 34, true);
        bClose.Click += delegate { Close(); };
        // langue (automatique selon Windows, modifiable ici)
        var cbLang = new ComboBox { Left = 590, Top = 610, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Card, ForeColor = Theme.Txt, Font = new Font("Segoe UI", 9f) };
        cbLang.Items.AddRange(App.LangNames);
        cbLang.SelectedIndex = Array.IndexOf(App.Langs, App.Lang);
        cbLang.SelectedIndexChanged += delegate {
            string l = App.Langs[cbLang.SelectedIndex]; if (l == App.Lang) return;
            cfg.Set("lang", l);
            MessageBox.Show(this, App.T("La langue changera au prochain lancement de Chutoki.", "The language will change the next time Chutoki starts."), App.Name);
        };
        Controls.Add(new Label { Text = "🌐", Left = 562, Top = 612, AutoSize = true, Font = new Font("Segoe UI Emoji", 10f) });
        Controls.Add(bRep); Controls.Add(bPause); Controls.Add(bPlan); Controls.Add(bClose); Controls.Add(cbTray); Controls.Add(cbLang);
        RefreshNow();
    }

    static void StyleGrid(DataGridView g) {
        g.DefaultCellStyle.BackColor = Theme.Card; g.DefaultCellStyle.ForeColor = Theme.Txt;
        g.DefaultCellStyle.SelectionBackColor = Theme.CardHover; g.DefaultCellStyle.SelectionForeColor = Theme.Txt;
        g.ColumnHeadersDefaultCellStyle.BackColor = Theme.Bg; g.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Dim;
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Bg;
        g.RowTemplate.Height = 28;
    }

    void OnRoleChanged(object o, DataGridViewCellEventArgs a) {
        if (loading || a.RowIndex < 0 || a.ColumnIndex != 3) return;
        var row = grid.Rows[a.RowIndex]; var ch = row.Tag as Channel; if (ch == null) return;
        int idx = Array.IndexOf(RoleNames, row.Cells[3].Value as string); if (idx < 0) return;
        var role = (Role)idx;
        if (role != Role.Bios && ch.Role == Role.Bios && e.Risky(ch, cooling)) {
            var ok = MessageBox.Show(this,
                App.T("Attention : si une pompe de watercooling est branchée sur « {0} », la ralentir peut faire surchauffer ton processeur.\n\nC'est bien un ventilateur ?",
                      "Warning: if a water-cooling pump is connected to \"{0}\", slowing it down can overheat your CPU.\n\nIs it really a fan?", ch.Name),
                App.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (ok != DialogResult.Yes) { loading = true; row.Cells[3].Value = RoleNames[(int)ch.Role]; loading = false; return; }
        }
        ch.Role = role; cfg.Set("role." + ch.Id, role.ToString());
    }

    public void RefreshNow() {
        lInfo.Text = App.T("Processeur : ", "CPU: ") + e.CpuName + "     " + App.T("Carte graphique : ", "Graphics: ") + e.GpuName + "\n" +
            App.T("Carte mère : ", "Motherboard: ") + e.BoardName + "     " + App.T("Mode : ", "Mode: ") + MainForm.ModeText(e);
        bPause.Text = e.Paused ? App.T("Reprendre le pilotage", "Resume control") : App.T("Pause (rendre au BIOS)", "Pause (back to BIOS)");
        bPause.Enabled = e.CanControl;
        foreach (DataGridViewRow row in grid.Rows) {
            var ch = (Channel)row.Tag; float r = Engine.Rpm(ch.Rpm);
            row.Cells[1].Value = r > 0 ? r.ToString("0") : App.T("arrêté / absent", "stopped / none");
            row.Cells[2].Value = (ch.Role == Role.Bios || e.Paused || !e.CanControl) ? "BIOS" : ch.Duty.ToString("0") + " %" + (ch.FloorBoost > 0 ? "  (min +" + ch.FloorBoost.ToString("0") + ")" : "");
        }
    }

    // ---------- rapport de test : uniquement le matériel et le comportement, rien de personnel.
    // La personne voit tout le texte avant de choisir de l'envoyer (rien ne part automatiquement).
    string TestReportText() {
        var sb = new System.Text.StringBuilder();
        Func<float, string> f0 = v => float.IsNaN(v) ? "-" : v.ToString("0", CultureInfo.InvariantCulture);
        sb.AppendLine("Chutoki " + App.Version + " (" + App.Lang + ")");
        sb.AppendLine("Windows: " + WinVersion());
        sb.AppendLine("CPU: " + e.CpuName);
        sb.AppendLine("GPU: " + e.GpuName + (e.GpuHotspot ? " (hotspot)" : ""));
        sb.AppendLine("Motherboard: " + e.BoardName);
        sb.AppendLine("Cooling: " + cooling + " | Laptop: " + (e.Laptop ? "yes" : "no") + " | PawnIO: " + (e.PawnIoMissing ? "missing" : "ok") +
            (e.InitFailed ? " | Init error: " + e.InitError : ""));
        sb.AppendLine("Mode: " + e.Mode + " (" + e.Eff.Key + ")" + (e.Paused ? " paused" : ""));
        sb.AppendLine("Now: CPU " + (e.CpuTemp.HasValue ? e.CpuTemp.Value.ToString("0") : "-") + " C, GPU " + (e.GpuTemp.HasValue ? e.GpuTemp.Value.ToString("0") : "-") +
            " C, CPU load " + (e.CpuLoad.HasValue ? e.CpuLoad.Value.ToString("0") : "-") + " %, GPU load " + (e.GpuLoad.HasValue ? e.GpuLoad.Value.ToString("0") : "-") + " %");
        sb.AppendLine("Session: " + (int)(DateTime.Now - e.SessionStart).TotalMinutes + " min, CPU max " + f0(e.MaxCpu) + " C, GPU max " + f0(e.MaxGpu) +
            " C, fans max " + e.MaxDuty.ToString("0") + " %, emergencies " + e.Emergencies + ", alerts " + e.AlertsCount);
        sb.AppendLine("Fans (" + e.Chans.Count + "):");
        foreach (var ch in e.Chans)
            sb.AppendLine("  - " + ch.Name + " [" + ch.Id + "] role=" + ch.Role + " rpm=" + Engine.Rpm(ch.Rpm).ToString("0") +
                " duty=" + (ch.Controlled ? ch.Duty.ToString("0") + "%" : "BIOS") + (ch.FloorBoost > 0 ? " floor+" + ch.FloorBoost.ToString("0") : "") +
                (ch.PumpLike ? " pump-like" : ""));
        return sb.ToString();
    }
    static string WinVersion() {
        try {
            var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string build = k.GetValue("CurrentBuild") as string ?? "";
            string name = (k.GetValue("ProductName") as string ?? "Windows");
            int b; if (int.TryParse(build, out b) && b >= 22000) name = name.Replace("Windows 10", "Windows 11");
            return name + " " + (k.GetValue("DisplayVersion") as string ?? "") + " (build " + build + ")";
        } catch { return Environment.OSVersion.VersionString; }
    }

    void ShowTestReport() {
        string text = TestReportText();
        var f = new Form(); Theme.Prepare(f, 640, 560, App.T("Rapport de test", "Test report"));
        f.StartPosition = FormStartPosition.CenterParent; f.MinimizeBox = false; f.ShowInTaskbar = false;
        f.Controls.Add(new Label { Left = 16, Top = 14, Width = 608, Height = 64, Font = Theme.F(9.5f),
            Text = App.T("Merci de tester Chutoki ! Ce rapport contient seulement ton matériel et ce que fait l'appli (rien de personnel). Ajoute ce que tu as remarqué (bruit, problèmes), puis envoie-le.",
                         "Thanks for testing Chutoki! This report only contains your hardware and what the app does (nothing personal). Add what you noticed (noise, problems), then send it.") });
        var box = new TextBox { Left = 16, Top = 84, Width = 608, Height = 300, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9f), BackColor = Theme.Card, ForeColor = Theme.Txt, Text = text.Replace("\n", "\r\n").Replace("\r\r", "\r") };
        f.Controls.Add(box);
        f.Controls.Add(new Label { Text = App.T("Ton avis (facultatif) :", "Your feedback (optional):"), Left = 16, Top = 394, AutoSize = true, Font = Theme.Semi(10f) });
        var note = new TextBox { Left = 16, Top = 420, Width = 608, Height = 60, Multiline = true, BackColor = Theme.Card, ForeColor = Theme.Txt, Font = Theme.F(9.5f) };
        f.Controls.Add(note);
        Func<string> full = () => text + (note.Text.Trim().Length > 0 ? "\nFeedback:\n" + note.Text.Trim() + "\n" : "");
        var bGit = Theme.Btn(App.T("Envoyer sur GitHub", "Send on GitHub"), 16, 504, 200, 38, true);
        bGit.Click += delegate {
            string title = "[Test] " + e.BoardName;
            string url = App.Repo + "/issues/new?template=test-report.yml&title=" + Uri.EscapeDataString(title) + "&config=" + Uri.EscapeDataString(full());
            if (url.Length > 7500) url = url.Substring(0, 7500);
            App.Open(url);
        };
        var bMail = Theme.Btn(App.T("Envoyer par email", "Send by email"), 224, 504, 170, 38);
        bMail.Click += delegate {
            string body = full(); if (body.Length > 1800) body = body.Substring(0, 1800);
            App.Open("mailto:" + App.Email + "?subject=" + Uri.EscapeDataString("Chutoki - test " + e.BoardName) + "&body=" + Uri.EscapeDataString(body));
        };
        var bCopy = Theme.Btn(App.T("Copier", "Copy"), 402, 504, 110, 38);
        bCopy.Click += delegate { try { Clipboard.SetText(full()); bCopy.Text = "✓"; } catch { } };
        var bClose = Theme.Btn(App.T("Fermer", "Close"), 520, 504, 104, 38);
        bClose.Click += delegate { f.Close(); };
        f.Controls.Add(bGit); f.Controls.Add(bMail); f.Controls.Add(bCopy); f.Controls.Add(bClose);
        f.ShowDialog(this); f.Dispose();
    }

    void ShowReport() {
        rep.Save();
        var f = new Form(); Theme.Prepare(f, 820, 600, App.T("Rapport", "Report"));
        f.StartPosition = FormStartPosition.CenterParent; f.MinimizeBox = false; f.ShowInTaskbar = false;
        f.Controls.Add(new Label { Text = App.T("Sessions (la première ligne = maintenant)", "Sessions (first row = now)"), Left = 16, Top = 14, AutoSize = true, Font = Theme.Semi(11f) });
        var g = new DataGridView {
            Left = 16, Top = 44, Width = 788, Height = 300, BackgroundColor = Theme.Card, BorderStyle = BorderStyle.None, ReadOnly = true,
            AllowUserToAddRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false, EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersHeight = 46, GridColor = Theme.Line, CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        StyleGrid(g);
        g.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True; g.ColumnHeadersDefaultCellStyle.Font = Theme.F(8f);
        foreach (var h in Report.Header.Split(';')) g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, SortMode = DataGridViewColumnSortMode.NotSortable });
        g.Columns[0].FillWeight = 190;
        bool first = true;
        foreach (var line in rep.Rows()) {
            int i = g.Rows.Add(line.Split(';').Take(g.Columns.Count).ToArray());
            if (first) { g.Rows[i].DefaultCellStyle.ForeColor = Theme.Accent; first = false; }
        }
        g.ClearSelection(); f.Controls.Add(g);
        f.Controls.Add(new Label { Text = App.T("Dernières alertes", "Latest alerts"), Left = 16, Top = 356, AutoSize = true, Font = Theme.Semi(11f) });
        var lb = new ListBox { Left = 16, Top = 384, Width = 788, Height = 150, BackColor = Theme.Card, ForeColor = Theme.Txt, BorderStyle = BorderStyle.None, HorizontalScrollbar = true };
        if (rep.Alerts.Count == 0) lb.Items.Add(App.T("Aucune alerte. Tout va bien.", "No alerts. All good."));
        else lb.Items.AddRange(rep.Alerts.ToArray());
        f.Controls.Add(lb);
        var bDir = Theme.Btn(App.T("Ouvrir le dossier", "Open folder"), 16, 552, 180);
        bDir.Click += delegate { App.Open(rep.Folder); };
        var bOk = Theme.Btn("OK", 704, 552, 100, 34, true);
        bOk.Click += delegate { f.Close(); };
        f.Controls.Add(bDir); f.Controls.Add(bOk); f.AcceptButton = bOk;
        f.ShowDialog(this); f.Dispose();
    }
}

// ====================================================================== fenêtre principale (simple)
class MainForm : Form {
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr h);
    readonly Settings cfg; readonly Engine e; readonly Report rep; readonly string cooling;
    readonly FaceView face; readonly ModeButton[] modes; readonly Label lFans; readonly CheckBox cbAuto;
    readonly NotifyIcon tray; readonly System.Windows.Forms.Timer timer;
    AdvancedForm adv; IntPtr lastIcon = IntPtr.Zero; bool exiting, startHidden;

    public MainForm(Settings s, string cool, bool hidden) {
        cfg = s; cooling = cool; startHidden = hidden;
        Theme.Prepare(this, 400, 640, App.Name + " " + App.Edition);
        StartPosition = FormStartPosition.CenterScreen;

        e = new Engine(cfg);
        if (App.Preview) e.Fake(); else e.Init(cooling);
        rep = new Report(e, cfg.Folder);

        // barre du haut
        var lName = new Label { Text = App.Name, Left = 20, Top = 16, AutoSize = true, Font = Theme.Semi(13f) };
        Controls.Add(lName);
        Controls.Add(new Label { Text = App.Edition, Left = 20 + lName.PreferredWidth + 4, Top = 20, AutoSize = true, Font = Theme.Semi(9f), ForeColor = Theme.Accent });
        var bAbout = Theme.Btn("?", 300, 12, 36, 36); bAbout.Font = Theme.Semi(12f);
        var bAdv = Theme.Btn("⚙", 344, 12, 36, 36); bAdv.Font = new Font("Segoe UI Symbol", 13f);
        new ToolTip().SetToolTip(bAbout, App.T("À propos", "About"));
        new ToolTip().SetToolTip(bAdv, App.T("Réglages avancés", "Advanced settings"));
        bAbout.Click += delegate { using (var a = new AboutForm()) a.ShowDialog(this); };
        bAdv.Click += delegate { OpenAdvanced(); };
        Controls.Add(bAbout); Controls.Add(bAdv);

        face = new FaceView { Left = 20, Top = 62, Width = 360, Height = 270 };
        Controls.Add(face);

        string[] keys = { "auto", "sil", "eq", "perf" };
        string[] titles = { App.T("Auto", "Auto"), App.T("Calme", "Quiet"), App.T("Normal", "Normal"), App.T("Fort", "Strong") };
        string[] subs = { App.T("je m'occupe de tout", "I handle everything"), App.T("chut, bébé dort", "shhh, baby's asleep"),
                          App.T("bon équilibre", "good balance"), App.T("refroidit au maximum", "maximum cooling") };
        modes = new ModeButton[4];
        for (int i = 0; i < 4; i++) {
            var m = new ModeButton { Key = keys[i], Title = titles[i], Sub = subs[i], Width = 172, Height = 74,
                Left = 20 + (i % 2) * 188, Top = 348 + (i / 2) * 86 };
            m.Click += delegate { SetMode(m.Key); };
            modes[i] = m; Controls.Add(m);
        }

        lFans = new Label { Left = 20, Top = 528, Width = 360, Height = 44, ForeColor = Theme.Dim, TextAlign = ContentAlignment.MiddleCenter, Font = Theme.F(9.5f) };
        Controls.Add(lFans);
        cbAuto = new CheckBox { Text = App.T("Démarrer avec Windows", "Start with Windows"), Left = 20, Top = 586, Width = 360, Height = 32,
            Font = Theme.F(10.5f), Checked = App.AutostartEnabled(), CheckAlign = ContentAlignment.MiddleLeft };
        cbAuto.CheckedChanged += delegate {
            if (!App.SetAutostart(cbAuto.Checked)) MessageBox.Show(this, App.T("Impossible de modifier le démarrage automatique.", "Could not change autostart."), App.Name);
        };
        Controls.Add(cbAuto);

        // bandeau des nouveautés (nouvelle version ou message), caché tant qu'il n'y a rien
        banner = new Label { Left = 20, Top = 628, Width = 360, Height = 46, Visible = false, BackColor = Theme.Accent, ForeColor = Theme.AccentTxt,
            Font = Theme.Semi(9.5f), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 30, 0), Cursor = Cursors.Hand, AutoEllipsis = true };
        var bannerX = new Label { Text = "×", Left = 334, Top = 0, Width = 26, Height = 46, BackColor = Theme.Accent, ForeColor = Theme.AccentTxt,
            Font = new Font("Segoe UI", 13f), TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };
        new ToolTip().SetToolTip(bannerX, App.T("Masquer", "Hide"));
        banner.Controls.Add(bannerX);
        banner.Click += delegate {
            if (bannerMsgId != null) { if (bannerUrl != null) App.Open(bannerUrl); cfg.Set("msg.seen", bannerMsgId); RefreshBanner(false); }
            else if (bannerUrl != null) App.Open(bannerUrl);
        };
        bannerX.Click += delegate {
            if (bannerMsgId != null) cfg.Set("msg.seen", bannerMsgId);
            else if (news != null && news.Latest != null) cfg.Set("update.skip", news.Latest);
            RefreshBanner(false);
        };
        Controls.Add(banner);

        // zone de notification
        var menu = new ContextMenuStrip();
        menu.Items.Add(App.T("Ouvrir", "Open"), null, delegate { ShowMain(); });
        menu.Items.Add(new ToolStripSeparator());
        for (int i = 0; i < 4; i++) { string k = keys[i]; menu.Items.Add(titles[i], null, delegate { SetMode(k); }); }
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(App.T("Quitter", "Quit"), null, delegate { exiting = true; Close(); });
        tray = new NotifyIcon { Text = App.Name, Visible = true, ContextMenuStrip = menu, Icon = Icon };
        tray.DoubleClick += delegate { ShowMain(); };

        e.Alerted += (title, msg, crit) => {
            rep.AddAlert(title, msg);
            tray.ShowBalloonTip(8000, App.Name + " — " + title, msg, crit ? ToolTipIcon.Error : ToolTipIcon.Warning);
            if (crit) System.Media.SystemSounds.Exclamation.Play();
        };
        e.ModeChanged += delegate { RefreshModes(); };

        if (!e.CanControl) {
            foreach (var m in modes) m.Enabled = false;
        }
        e.SetMode(cfg.Get("mode", "sil"));
        RefreshModes();

        timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += delegate { Tick(); };
        if (!App.Preview) timer.Start();
        Tick();

        // nouveautés : première vérification 30 s après le démarrage, puis une fois par jour
        newsTimer = new System.Windows.Forms.Timer { Interval = 30000 };
        newsTimer.Tick += delegate { CheckNews(); };
        if (!App.Preview) newsTimer.Start();
        SystemEvents.SessionEnding += delegate { rep.Save(); e.RestoreAll(); };
        if (!App.Preview) {
            Shown += delegate { WarnIfNeeded(); };
            if (startHidden) WarnIfNeeded();
        }
    }

    // ---------- nouveautés
    readonly Label banner; readonly System.Windows.Forms.Timer newsTimer;
    string bannerUrl, bannerMsgId; News news; DateTime nextNews = DateTime.MinValue; bool fetching;

    void CheckNews() {
        if (cfg.Get("news", "1") != "1") { if (banner.Visible) HideBanner(); return; }
        if (fetching || DateTime.Now < nextNews) return;
        fetching = true;
        System.Threading.ThreadPool.QueueUserWorkItem(delegate {
            var n = News.Fetch();
            try { BeginInvoke((Action)delegate { fetching = false; nextNews = DateTime.Now.AddHours(n == null ? 1 : 24); ApplyNews(n); }); } catch { }
        });
    }
    public void ApplyNews(News n) { if (n == null) return; news = n; RefreshBanner(true); }

    void RefreshBanner(bool notify) {
        var n = news;
        if (n == null) { HideBanner(); return; }
        if (n.HasUpdate && cfg.Get("update.skip", "") != n.Latest) {
            ShowBanner(App.T("Nouvelle version {0} disponible ! Clique pour la télécharger.", "New version {0} available! Click to download.", n.Latest), n.Url, null);
            if (notify && !App.Preview && cfg.Get("update.notified", "") != n.Latest) {
                cfg.Set("update.notified", n.Latest);
                tray.ShowBalloonTip(8000, App.Name, banner.Text, ToolTipIcon.Info);
            }
            return;
        }
        if (n.MsgId != null && cfg.Get("msg.seen", "") != n.MsgId) {
            ShowBanner(n.MsgText, n.MsgUrl, n.MsgId);
            if (notify && !App.Preview && cfg.Get("msg.notified", "") != n.MsgId) {
                cfg.Set("msg.notified", n.MsgId);
                tray.ShowBalloonTip(8000, App.Name + " — " + App.T("Message de Chutoki", "News from Chutoki"), n.MsgText, ToolTipIcon.Info);
            }
            return;
        }
        HideBanner();
    }
    // la fenêtre s'agrandit juste ce qu'il faut pour le bandeau (k = échelle d'affichage)
    void ShowBanner(string text, string url, string msgId) {
        banner.Text = text; bannerUrl = url; bannerMsgId = msgId;
        new ToolTip().SetToolTip(banner, text);
        if (banner.Visible) return;
        float k = banner.Height / 46f;
        banner.Visible = true;
        ClientSize = new Size(ClientSize.Width, ClientSize.Height + (int)Math.Round(56 * k));
    }
    void HideBanner() {
        if (!banner.Visible) return;
        float k = banner.Height / 46f;
        banner.Visible = false;
        ClientSize = new Size(ClientSize.Width, ClientSize.Height - (int)Math.Round(56 * k));
    }

    public AdvancedForm Advanced() { if (adv == null || adv.IsDisposed) adv = new AdvancedForm(e, cfg, rep, cooling); return adv; }
    public void HideTray() { tray.Visible = false; }

    bool warned;
    void WarnIfNeeded() {
        if (warned) return; warned = true;
        if (e.PawnIoMissing) {
            var r = MessageBox.Show(App.T(
                "Pour régler les ventilateurs, Chutoki a besoin d'un petit composant gratuit et sûr : PawnIO.\n\nOuvrir sa page officielle de téléchargement ? (installe « PawnIO_setup.exe », puis relance Chutoki)",
                "To control fans, Chutoki needs a small free and safe component: PawnIO.\n\nOpen its official download page? (install \"PawnIO_setup.exe\", then restart Chutoki)"),
                App.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (r == DialogResult.Yes) App.Open(App.PawnIoUrl);
        }
    }

    void SetMode(string k) { e.SetMode(k); cfg.Set("mode", k); RefreshModes(); }
    void RefreshModes() { foreach (var m in modes) { m.Selected = m.Key == e.Mode; m.Invalidate(); } if (adv != null && !adv.IsDisposed) adv.RefreshNow(); }

    void OpenAdvanced() {
        if (adv == null || adv.IsDisposed) adv = new AdvancedForm(e, cfg, rep, cooling);
        adv.Show(); adv.WindowState = FormWindowState.Normal; adv.Activate();
    }

    static string Deg(float? v) { return v.HasValue ? v.Value.ToString("0") + "°" : "–"; }
    public static string ModeText(Engine e) {
        string s = e.Mode == "auto" ? "Auto (" + e.Eff.Title + ")" : e.Cur.Title;
        if (e.Boost) s = e.Cur.Title + " " + App.T("(renforcé)", "(boosted)");
        if (e.Fast || e.WarnZone) s += " · " + App.T("garde-fou actif", "safeguard on");
        return s;
    }

    void Tick() {
        e.Tick();
        var lv = e.CanControl && e.Paused ? Level.Idle : e.Overall;
        face.Level = lv;
        if (e.InitFailed) { face.Title = App.T("Capteurs illisibles", "Sensors unavailable"); face.Level = Level.Idle; }
        else if (e.Emergency) face.Title = App.T("Trop chaud ! Ventilos à fond", "Too hot! Fans at full speed");
        else if (e.CanControl && e.Paused) face.Title = App.T("En pause", "Paused");
        else face.Title = lv == Level.Ok ? App.T("Tout va bien", "All good") : lv == Level.Warm ? App.T("Ça chauffe un peu", "Getting warm") : lv == Level.Hot ? App.T("Très chaud !", "Very hot!") : App.T("Températures inconnues", "Temperatures unknown");
        face.Sub = App.T("Processeur ", "CPU ") + Deg(e.CpuTemp) + "   ·   " + App.T("Carte graphique ", "Graphics ") + Deg(e.GpuTemp);
        face.Invalidate();

        if (e.Laptop) lFans.Text = App.T("PC portable : ses ventilateurs sont gérés par le fabricant.\nChutoki surveille seulement les températures.", "Laptop: its fans are managed by the manufacturer.\nChutoki only monitors temperatures.");
        else if (e.PawnIoMissing) lFans.Text = App.T("Composant PawnIO manquant : surveillance seulement.\nInstalle-le puis relance l'appli (bouton ?).", "PawnIO component missing: monitoring only.\nInstall it, then restart the app.");
        else if (e.InitFailed) lFans.Text = App.T("Lance l'appli en administrateur.", "Run the app as administrator.");
        else if (e.Chans.Count == 0) lFans.Text = App.T("Ta carte mère n'est pas encore compatible.\nChutoki surveille seulement les températures.", "Your motherboard isn't supported yet.\nChutoki only monitors temperatures.");
        else if (e.Paused) lFans.Text = App.T("Ton PC gère ses ventilateurs lui-même (pause).", "Your PC is managing its fans (paused).");
        else if (float.IsNaN(e.AvgDuty)) lFans.Text = App.T("Aucun ventilateur piloté (voir ⚙).", "No fan under control (see ⚙).");
        else if (e.Fast || e.WarnZone) lFans.Text = App.T("Ça chauffe : je fais réagir les ventilateurs ({0} %)", "Heating up: fans are reacting ({0} %)", e.AvgDuty.ToString("0"));
        else if (e.Boost) lFans.Text = App.T("Ventilateurs à {0} % (Calme renforcé le temps de refroidir)", "Fans at {0} % (Quiet boosted while cooling down)", e.AvgDuty.ToString("0"));
        else lFans.Text = App.T("Les ventilateurs tournent à {0} %", "Fans are running at {0} %", e.AvgDuty.ToString("0")) +
            (e.Mode == "auto" ? "  (" + e.Eff.Title + ")" : "");

        if (adv != null && !adv.IsDisposed && adv.Visible) {
            adv.RefreshNow();
            adv.Graph.Push(e.CpuTemp ?? float.NaN, e.GpuTemp ?? float.NaN, e.AvgDuty);
        }
        if ((DateTime.Now - rep.LastSave).TotalSeconds >= 60) rep.Save();
        UpdateTray(lv);
    }

    void UpdateTray(Level lv) {
        Color c = lv == Level.Ok ? Theme.Ok : lv == Level.Warm ? Theme.Warm : lv == Level.Hot ? Theme.Hot : Theme.Idle;
        Icon ic;
        using (var bmp = new Bitmap(32, 32))
        using (var g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias; g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (var b = new SolidBrush(c)) g.FillEllipse(b, 0, 0, 31, 31);
            string s = e.CpuTemp.HasValue ? e.CpuTemp.Value.ToString("0") : "?";
            using (var f = new Font("Segoe UI", s.Length > 2 ? 10f : 13f, FontStyle.Bold)) using (var w = new SolidBrush(Color.White)) {
                var sz = g.MeasureString(s, f); g.DrawString(s, f, w, (32 - sz.Width) / 2, (32 - sz.Height) / 2 + 1);
            }
            ic = Icon.FromHandle(bmp.GetHicon());
        }
        tray.Icon = ic;
        if (lastIcon != IntPtr.Zero) DestroyIcon(lastIcon);
        lastIcon = ic.Handle;
        string t = App.Name + " — " + face.Title;
        tray.Text = t.Length > 63 ? t.Substring(0, 63) : t;
    }

    void ShowMain() { Show(); WindowState = FormWindowState.Normal; Activate(); }

    protected override void SetVisibleCore(bool value) {
        if (startHidden && !IsHandleCreated) { CreateHandle(); value = false; startHidden = false; }
        base.SetVisibleCore(value);
    }

    bool hintShown;
    protected override void OnFormClosing(FormClosingEventArgs ev) {
        if (!exiting && ev.CloseReason == CloseReason.UserClosing && cfg.Get("tray", "1") == "1") {
            ev.Cancel = true; Hide();
            if (!hintShown) { hintShown = true; tray.ShowBalloonTip(3000, App.Name, App.T("Je reste actif près de l'horloge. Clic droit > Quitter pour m'arrêter.", "Still running near the clock. Right-click > Quit to stop me."), ToolTipIcon.Info); }
            return;
        }
        timer.Stop(); rep.Save(); e.Close(); tray.Visible = false;
        base.OnFormClosing(ev);
    }
}

// ====================================================================== point d'entrée
static class Program {
    // Aperçu : rend chaque fenêtre en PNG avec des données fictives (aucun accès matériel)
    static void PreviewShots(string outDir, string lang) {
        App.Preview = true; App.Lang = App.Langs.Contains(lang) ? lang : "en";
        Directory.CreateDirectory(outDir);
        Action<Form, string> shot = (f, name) => {
            f.StartPosition = FormStartPosition.Manual; f.Location = new Point(-5000, -5000); f.ShowInTaskbar = false;
            f.Show(); Application.DoEvents(); Thread.Sleep(150); Application.DoEvents();
            using (var bmp = new Bitmap(f.Width, f.Height)) { f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height)); bmp.Save(Path.Combine(outDir, name + "-" + lang + ".png")); }
        };
        var main = new MainForm(new Settings(), "air", false);
        shot(main, "1-main");
        using (var w = new WelcomeForm("unknown")) shot(w, "0-welcome");
        using (var a = new AboutForm()) shot(a, "2-about");
        shot(main.Advanced(), "3-advanced");
        main.ApplyNews(new News { Latest = "9.9.0", Url = App.Site });
        shot(main, "4-news");
        main.HideTray();
    }

    // Passage depuis les anciennes versions (FanSilence, Fan Blay4mok) : on les arrête pour ne pas être deux
    // à piloter les ventilateurs, et leur démarrage automatique est remplacé par celui de Chutoki.
    static void Upgrade() {
        foreach (var name in new[] { "FanBlay4mok", "FanSilence" })
            foreach (var p in Process.GetProcessesByName(name)) { try { p.Kill(); p.WaitForExit(3000); } catch { } }
        bool hadTask = false;
        foreach (var task in new[] { "FanBlay4mok", "FanSilence" }) {
            int c; App.Run("schtasks", "/query /tn " + task, out c);
            if (c == 0) { hadTask = true; App.Run("schtasks", "/delete /f /tn " + task, out c); }
        }
        if (hadTask && !App.AutostartEnabled()) App.SetAutostart(true);
    }

    [STAThread]
    static void Main(string[] args) {
        if (args.Length >= 2 && args[0] == "--preview") {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            PreviewShots(args[1], args.Length >= 3 ? args[2] : "fr");
            return;
        }
        bool created;
        var cfg = new Settings();
        string lang = cfg.Get("lang", "");
        if (App.Langs.Contains(lang)) App.Lang = lang;
        using (var m = new Mutex(true, "Global\\ChutokiSingleInstance", out created)) {
            if (!created) { MessageBox.Show(App.T("Chutoki est déjà lancé (icône près de l'horloge).", "Chutoki is already running (icon near the clock)."), App.Name); return; }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Upgrade();
            if (cfg.Get("accepted", "") == "") {
                using (var w = new WelcomeForm(cfg.Get("cooling", "unknown"))) {
                    if (w.ShowDialog() != DialogResult.OK) return;
                    cfg.Set("cooling", w.Cooling);
                    cfg.Set("accepted", App.Version);
                }
            }
            bool tray = args.Any(a => a == "--tray");
            Application.Run(new MainForm(cfg, cfg.Get("cooling", "unknown"), tray));
        }
    }
}
}
