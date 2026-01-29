using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TaskbarLavaLamp
{
    public partial class Form1 : Form
    {
        // NativeMethods moved to NativeMethods.cs

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;

        // Costanti usate da UpdateLayeredWindow / BLENDFUNCTION
        private const byte AC_SRC_ALPHA = 0x01;       // Alpha channel present
        private const int ULW_ALPHA = 0x00000002;     // Use alpha

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;
        private const int WS_EX_LAYERED = 0x80000;

        private static int _instanceCounter = 0;
        private static bool _firstInstanceInitialised = false;
        private int _instanceId;
        private string _configFileName;

        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer stayOnTopTimer = new System.Windows.Forms.Timer();
        private List<LavaPoint> lavaPoints = new List<LavaPoint>();
        private Random random = new Random();
        private Rectangle lavaBounds;
        private Config currentConfig;
        private bool inSetupMode = false;

        public bool IsFloating { get; set; } = false;
        public bool IsFullScreenMode { get; set; } = false;

        public Form1(int? forcedId = null)
        {
            InitializeComponent();
            _instanceId = forcedId ?? ++_instanceCounter;
            _configFileName = $"lavalamp.config.{_instanceId}.json";
            this.MinimumSize = new Size(0, 0);

            // FIX ICONA: Se l'icona manca, ne mettiamo una di sistema per non far sparire la tray
            // Prefer the Form.Icon (set in designer/resources) for the tray; fallback to system icon
            try
            {
                this.notifyIcon1.Icon = this.Icon ?? SystemIcons.Application;
            }
            catch
            {
                try { this.notifyIcon1.Icon = SystemIcons.Application; } catch { }
            }

            // If Form.Icon changes at runtime the developer can update notifyIcon1 manually.

            // Assicuriamoci di agganciare una sola volta l'handler del timer
            animationTimer.Interval = 33;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override CreateParams CreateParams
        {
            get { CreateParams cp = base.CreateParams; cp.ExStyle |= WS_EX_TOOLWINDOW; return cp; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfiguration();

            if (!_firstInstanceInitialised && _instanceId == 1)
            {
                _firstInstanceInitialised = true;
                RestoreOtherLamps();
            }

            stayOnTopTimer.Interval = 150;
            stayOnTopTimer.Tick += (s, ev) =>
            {
                if (this.IsDisposed || this.Disposing || !this.IsHandleCreated) return;
                if (!inSetupMode && contextMenuStrip1 != null && !contextMenuStrip1.Visible)
                {
                    try
                    {
                        NativeMethods.SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }
                    catch { }
                }
            };

            if (IsFullScreenMode) { EnterFullScreenMode(); }
            else if (IsFloating || !File.Exists(_configFileName)) { EnterSetupMode(); }
            else { EnterLavaMode(currentConfig); }
        }

        // --- Nuovo metodo handler nominato per evitare attach multipli ---
        private void OnSetupMouseDown(object sender, MouseEventArgs ev)
        {
            if (ev.Button == MouseButtons.Left)
            {
                this.Activate();
                this.Capture = false;
                Message m = Message.Create(this.Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                base.WndProc(ref m);
            }
        }

        private void RestoreOtherLamps()
        {
            string[] configFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "lavalamp.config.*.json");
            foreach (string file in configFiles)
            {
                string fileName = Path.GetFileName(file);
                string[] parts = fileName.Split('.');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int id))
                {
                    if (id > 1)
                    {
                        Form1 extraLamp = new Form1(id);
                        extraLamp.IsFloating = true;
                        extraLamp.Show();
                        // assicurarsi che la nuova finestra prenda il focus così risponde a key/mouse subito
                        extraLamp.Activate();
                    }
                }
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFileName))
                {
                    currentConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_configFileName));
                }
            }
            catch { }
            if (currentConfig == null) currentConfig = new Config();
        }

        private void SaveConfiguration()
        {
            if (this.IsDisposed) return;
            currentConfig.X = this.Left;
            currentConfig.Y = this.Top;
            currentConfig.Width = this.Width;
            currentConfig.Height = this.Height;
            File.WriteAllText(_configFileName, JsonConvert.SerializeObject(currentConfig, Formatting.Indented));
        }

        private void EnterSetupMode()
        {
            inSetupMode = true;
            stayOnTopTimer.Stop();
            // Stop animation when entering setup to avoid UpdateLayer calls on a non-layered window
            animationTimer.Stop();
            this.Controls.Clear();
            this.FormBorderStyle = FormBorderStyle.None;

            // Rimuoviamo lo stile layered e transparent per tornare ad una finestra normale
            // Rimuoviamo WS_EX_LAYERED e WS_EX_TRANSPARENT in modo sicuro
            ApplyExStyle(WS_EX_LAYERED | WS_EX_TRANSPARENT, add: false);

            // Ora possiamo usare BackColor senza rompere il rendering della layered window
            this.BackColor = Color.Crimson;
            // Non impostare Form.Opacity: WinForms gestisce internamente UpdateLayeredWindow
            // e può causare ERROR_INVALID_PARAMETER se lo stato della finestra è incoerente.
            // Evitiamo l'uso di Opacity e mostriamo il contenuto direttamente.
            if (this.BackgroundImage != null)
            {
                try { this.BackgroundImage.Dispose(); } catch { }
                this.BackgroundImage = null;
            }
            this.ShowInTaskbar = true;
            this.TopMost = true;
            this.KeyPreview = true;
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Text = $"Lava Lamp #{_instanceId} [Setup]";

            // Evitiamo di agganciare più volte lo stesso handler
            this.MouseDown -= OnSetupMouseDown;
            this.MouseDown += OnSetupMouseDown;
        }

        private void EnterLavaMode(Config config)
        {
            inSetupMode = false;
            this.Controls.Clear();
            this.FormBorderStyle = FormBorderStyle.None;
            this.Location = new Point(config.X, config.Y);
            this.Size = new Size(config.Width, config.Height);
            this.BackColor = Color.LimeGreen; // può restare ma non usiamo più TransparencyKey
            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.Text = $"Lava Lamp #{_instanceId} [Attiva]";

            // Impostiamo WS_EX_LAYERED (+ eventualmente WS_EX_TRANSPARENT per il click-through)
            ApplyExStyle(WS_EX_LAYERED | WS_EX_TRANSPARENT, add: true);
            // Remove any background image used in non-layered mode
            if (this.BackgroundImage != null)
            {
                try { this.BackgroundImage.Dispose(); } catch { }
                this.BackgroundImage = null;
            }

            stayOnTopTimer.Start();
            StartAnimation();
            // Ensure we render immediately so the window doesn't show the BackColor before the first frame
            try { RenderAndUpdateLayered(dim: false); } catch { }
        }

        private void EnterFullScreenMode()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LimeGreen;
            this.DoubleBuffered = true;
            this.TopMost = true;
            ApplyExStyle(WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW, add: true);
            StartAnimation(300);
        }

        private void StartAnimation(int count = 0)
        {
            lavaBounds = this.ClientRectangle;

            int pCount;
            if (count > 0)
            {
                pCount = count;
            }
            else
            {
                // Calcolo basato sull'area per adattare la densità allo spazio verticale/orizzontale
                long area = Math.Max(1L, (long)lavaBounds.Width * Math.Max(1, lavaBounds.Height));
                // Density dal config, scalata anche da SizeMultiplier per mantenere proporzionalità con la dimensione dei punti
                double density = Math.Max(0.0001, currentConfig.ParticleDensity) * currentConfig.SizeMultiplier;
                long computed = (long)(area * density);
                // Clamp su range sensato: almeno 4, massimo MaxParticles
                pCount = (int)Math.Max(4, Math.Min(computed, currentConfig.MaxParticles));
            }

            InitializeLavaPoints(pCount);

            // Non ri-agganciare l'handler qui: è già registrato nel costruttore.
            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }
        }

        private void InitializeLavaPoints(int count)
        {
            lavaPoints.Clear();
            Color c1 = Color.FromArgb(currentConfig.LavaColorArgb != 0 ? currentConfig.LavaColorArgb : Color.Orange.ToArgb());
            Color c2 = Color.FromArgb(currentConfig.LavaColorArgb2 != 0 ? currentConfig.LavaColorArgb2 : Color.Red.ToArgb());
            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0f;
                Color c = InterpolateColor(c1, c2, t); // sfumatura tra i due colori invece di alternare
                lavaPoints.Add(new LavaPoint(lavaBounds, random, c, currentConfig.SelectedShape, currentConfig.SizeMultiplier));
            }
        }

        private Color InterpolateColor(Color a, Color b, float t)
        {
            int A = (int)Math.Round(a.A + (b.A - a.A) * t);
            int R = (int)Math.Round(a.R + (b.R - a.R) * t);
            int G = (int)Math.Round(a.G + (b.G - a.G) * t);
            int B = (int)Math.Round(a.B + (b.B - a.B) * t);
            return Color.FromArgb(A, R, G, B);
        }

        private void RenderAndUpdateLayered(bool dim = false)
        {
            // Render su bitmap ARGB premoltiplicato
            using (Bitmap bmp = new Bitmap(Math.Max(1, this.Width), Math.Max(1, this.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    foreach (var p in lavaPoints) p.Draw(g);

                    if (dim)
                    {
                        using (Brush overlay = new SolidBrush(Color.FromArgb(120, 0, 0, 0))) // leggera oscuramento
                        {
                            g.FillRectangle(overlay, 0, 0, bmp.Width, bmp.Height);
                        }
                    }
                }
                // Check if window is layered. If so, use UpdateLayeredWindow. Otherwise assign as BackgroundImage.
                bool isLayered = false;
                try
                {
                    IntPtr ex = NativeMethods.GetWindowLongPtr(this.Handle, GWL_EXSTYLE);
                    isLayered = (ex.ToInt64() & WS_EX_LAYERED) != 0;
                }
                catch (Exception ex)
                {
                    isLayered = false;
                    Logger.Log($"GetWindowLongPtr failed in RenderAndUpdateLayered: {ex}");
                }

                if (isLayered)
                {
                    bool ok = UpdateLayer(bmp, dim ? (byte)160 : (byte)255);
                    if (!ok) Logger.Log("UpdateLayer failed while window is layered");
                }
                else
                {
                    // Show rendering on non-layered window by setting BackgroundImage
                    try
                    {
                        var bmpClone = (Bitmap)bmp.Clone();
                        var old = this.BackgroundImage;
                        this.BackgroundImage = bmpClone;
                        if (old != null) old.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to set BackgroundImage fallback: {ex}");
                    }
                }
            }
        }

        private bool UpdateLayer(Bitmap bmp, byte globalAlpha = 255)
        {
            IntPtr screenDC = IntPtr.Zero;
            IntPtr memDC = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;
            bool result = false;
            try
            {
                screenDC = NativeMethods.GetDC(IntPtr.Zero);
                if (screenDC == IntPtr.Zero) throw new InvalidOperationException("GetDC failed");
                memDC = NativeMethods.CreateCompatibleDC(screenDC);
                if (memDC == IntPtr.Zero) throw new InvalidOperationException("CreateCompatibleDC failed");
                hBitmap = bmp.GetHbitmap(Color.FromArgb(0)); // bitmap con canale alpha
                if (hBitmap == IntPtr.Zero) throw new InvalidOperationException("GetHbitmap failed");
                oldBitmap = NativeMethods.SelectObject(memDC, hBitmap);

                Point topPos = new Point(this.Left, this.Top);
                Size size = new Size(bmp.Width, bmp.Height);
                Point srcLoc = new Point(0, 0);

                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = 0; // AC_SRC_OVER
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = globalAlpha;
                blend.AlphaFormat = AC_SRC_ALPHA;

                bool ok = NativeMethods.UpdateLayeredWindow(this.Handle, screenDC, ref topPos, ref size, memDC, ref srcLoc, 0, ref blend, ULW_ALPHA);
                if (!ok)
                {
                    int err = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"UpdateLayeredWindow failed: {err}");
                    Logger.Log($"UpdateLayeredWindow failed with code: {err}");
                    result = false;
                }
                else result = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateLayer exception: {ex}");
                result = false;
            }
            finally
            {
                try { if (memDC != IntPtr.Zero && oldBitmap != IntPtr.Zero) NativeMethods.SelectObject(memDC, oldBitmap); } catch { }
                try { if (hBitmap != IntPtr.Zero) NativeMethods.DeleteObject(hBitmap); } catch { }
                try { if (memDC != IntPtr.Zero) NativeMethods.DeleteDC(memDC); } catch { }
                try { if (screenDC != IntPtr.Zero) NativeMethods.ReleaseDC(IntPtr.Zero, screenDC); } catch { }
            }

            return result;
        }

        private volatile bool _transitionInProgress = false;

        // Render a snapshot bitmap of the current lava points (caller must dispose)
        private Bitmap RenderBitmapSnapshot()
        {
            Rectangle rect = this.ClientRectangle;
            Bitmap bmp = new Bitmap(Math.Max(1, rect.Width), Math.Max(1, rect.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Copy points to avoid concurrency
            List<LavaPoint> copy;
            lock (lavaPoints)
            {
                copy = new List<LavaPoint>(lavaPoints);
            }
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                foreach (var p in copy) p.Draw(g);
            }
            return bmp;
        }

        // Simple fade transition when toggling layered mode to improve UX
        private void TransitionFade(bool enableLayered, int steps = 8, int delayMs = 30)
        {
            if (_transitionInProgress) return;
            _transitionInProgress = true;
            try
            {
                if (enableLayered)
                {
                    // Ensure layered style is set before fading in
                    ApplyExStyle(WS_EX_LAYERED | WS_EX_TRANSPARENT, add: true);
                    for (int i = 0; i <= steps; i++)
                    {
                        int alpha = (int)Math.Round(255.0 * i / steps);
                        using (Bitmap bmp = RenderBitmapSnapshot())
                        {
                            UpdateLayer(bmp, (byte)alpha);
                        }
                        System.Threading.Thread.Sleep(delayMs);
                        Application.DoEvents();
                    }
                }
                else
                {
                    // Fade out then remove layered
                    for (int i = steps; i >= 0; i--)
                    {
                        int alpha = (int)Math.Round(255.0 * i / steps);
                        using (Bitmap bmp = RenderBitmapSnapshot())
                        {
                            UpdateLayer(bmp, (byte)alpha);
                        }
                        System.Threading.Thread.Sleep(delayMs);
                        Application.DoEvents();
                    }
                    ApplyExStyle(WS_EX_LAYERED | WS_EX_TRANSPARENT, add: false);
                    // Set BackgroundImage fallback
                    using (Bitmap bmp = RenderBitmapSnapshot())
                    {
                        try
                        {
                            var clone = (Bitmap)bmp.Clone();
                            var old = this.BackgroundImage;
                            this.BackgroundImage = clone;
                            if (old != null) old.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"TransitionFade fallback set BackgroundImage failed: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"TransitionFade exception: {ex}");
            }
            finally
            {
                _transitionInProgress = false;
            }
        }

        // Safely apply/remove extended window styles and ensure the changes are applied.
        private void ApplyExStyle(long styleMask, bool add)
        {
            if (!this.IsHandleCreated)
            {
                // Defer until handle is created
                try
                {
                    this.BeginInvoke((Action)(() => ApplyExStyle(styleMask, add)));
                }
                catch { }
                return;
            }

            IntPtr exStylePtr = NativeMethods.GetWindowLongPtr(this.Handle, GWL_EXSTYLE);
            long style = exStylePtr.ToInt64();
            style = add ? (style | styleMask) : (style & ~styleMask);
            IntPtr ret = IntPtr.Zero;
            try
            {
                ret = NativeMethods.SetWindowLongPtr(this.Handle, GWL_EXSTYLE, new IntPtr(style));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetWindowLongPtr threw: {ex}");
            }

            // Check if SetWindowLongPtr likely failed (on x86 returns IntPtr with low value)
            if (ret == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"SetWindowLongPtr may have failed: {err}");
            }

            bool sp = NativeMethods.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_FRAMECHANGED);
            if (!sp)
            {
                int err = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"SetWindowPos failed after SetWindowLongPtr: {err}");
                Logger.Log($"SetWindowPos failed after SetWindowLongPtr: {err}");
                try
                {
                    // Recreate handle to ensure styles are applied consistently
                    this.RecreateHandle();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecreateHandle failed: {ex}");
                    Logger.Log($"RecreateHandle failed: {ex}");
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (inSetupMode)
            {
                e.Graphics.DrawString($"ID: #{_instanceId} - Frecce: Muovi - SHIFT+Frecce: Altezza - INVIO: Salva",
                    new Font("Segoe UI", 9, FontStyle.Bold), Brushes.White, 5, 5);
                return;
            }
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var p in lavaPoints) p.Draw(e.Graphics);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (inSetupMode)
            {
                int step = 5;
                if (e.Shift)
                {
                    if (e.KeyCode == Keys.Left) this.Width = Math.Max(10, this.Width - step);
                    if (e.KeyCode == Keys.Right) this.Width += step;
                    if (e.KeyCode == Keys.Up) { this.Top -= step; this.Height += step; }
                    if (e.KeyCode == Keys.Down) { if (this.Height > step) { this.Top += step; this.Height -= step; } }
                }
                else
                {
                    if (e.KeyCode == Keys.Left) this.Left -= step;
                    if (e.KeyCode == Keys.Right) this.Left += step;
                    if (e.KeyCode == Keys.Up) this.Top -= step;
                    if (e.KeyCode == Keys.Down) this.Top += step;
                }
                if (e.KeyCode == Keys.Enter) { SaveConfiguration(); EnterLavaMode(currentConfig); e.Handled = true; }
            }
            if (IsFullScreenMode && e.KeyCode == Keys.Escape) this.Close();
        }

        // --- GESTIONE MENU ---

        private void impostazioniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
            SettingsForm settingsWindow = new SettingsForm(_configFileName);
            if (settingsWindow.ShowDialog() == DialogResult.OK) LoadConfiguration();
            if (!inSetupMode) StartAnimation();
        }

        private void riposizionaLampadaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            animationTimer.Stop(); EnterSetupMode();
        }

        private void dimenticaQuestaLampadaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Rimuovere definitivamente?", "Conferma", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (File.Exists(_configFileName)) File.Delete(_configFileName);
                this.Close();
            }
        }

        private void esciToolStripMenuItem_Click(object sender, EventArgs e) { this.Close(); }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            stayOnTopTimer.Stop();
            animationTimer.Stop();
            if (notifyIcon1 != null) notifyIcon1.Visible = false;
            base.OnFormClosing(e);
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Non cambiare Form.Opacity su layered window: può causare che il contenuto venga "perso" e mostrato il BackColor
            animationTimer.Stop();
            stayOnTopTimer.Stop();

            // renderizziamo una versione leggermente oscurata e fissa (evita "sfarfallio/verde")
            RenderAndUpdateLayered(dim: true);
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            // Ripristina lo stato e riavvia l'animazione senza toccare Opacity
            if (!inSetupMode) { stayOnTopTimer.Start(); animationTimer.Start(); }
            // render normale per assicurare immagine aggiornata
            RenderAndUpdateLayered(dim: false);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;

            // aggiornamento fisica...
            foreach (var p in lavaPoints) p.Update(lavaBounds, currentConfig.SpeedMultiplier);

            // renderizziamo su bitmap con alpha e aggiorniamo la layered window
            RenderAndUpdateLayered();
        }
    }

    // Sostituisci la dichiarazione di BLENDFUNCTION con una pubblica
    // BLENDFUNCTION: struttura usata da UpdateLayeredWindow
    [StructLayout(LayoutKind.Sequential)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }
}
