/*
 * Importazioni delle librerie (namespace).
 * - System.*: Classi di base.
 * - System.Drawing/Drawing2D: Per il rendering GDI+ (forme, colori, pennelli).
 * - System.Windows.Forms: Il core di WinForms (Form, Button, Timer).
 * - System.Runtime.InteropServices: FONDAMENTALE. Permette il "Platform Invoke" (P/Invoke),
 * ovvero la chiamata a funzioni C/C++ non gestite (le API di Windows).
 * - System.IO: Per leggere/scrivere il file di configurazione.
 * - Newtonsoft.Json: Libreria di terze parti (via NuGet) per serializzare/deserializzare
 * facilmente il nostro oggetto 'Config' in formato JSON.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;

namespace TaskbarLavaLamp
{
    // La nostra classe Form principale. Eredita da 'Form',
    // che ci dà tutta la logica di base di una finestra.
    public partial class Form1 : Form
    {
        // --- Interoperabilità API Win32 (P/Invoke) ---

        // Stiamo importando 'SetWindowLong' e 'GetWindowLong' dalla libreria
        // 'user32.dll'. Questo ci permette di manipolare gli stili di
        // una finestra (come renderla trasparente ai click)
        // a un livello molto più basso di quanto WinForms permetterebbe.
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        // Costanti che sono "magic number" dell'API Win32.
        // Sono essenzialmente flag per 'SetWindowLong'.
        private const int GWL_EXSTYLE = -20;        // Flag: "vogliamo modificare gli stili estesi"
        private const int WS_EX_TRANSPARENT = 0x20; // Flag: "rendi questa finestra trasparente ai click"
        private const int WS_EX_TOOLWINDOW = 0x80;  // Flag: "questa è una finestra 'strumento', non mostrarla sulla taskbar"

        // --- Stato della Lampada ---
        // Questo timer è il nostro "game loop" o "update tick".
        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();

        // Il nostro "modello" (in un pattern MVC). È la lista di tutte
        // le particelle (LavaPoint) che la nostra simulazione deve gestire.
        private List<LavaPoint> lavaPoints = new List<LavaPoint>();

        // Un'istanza 'Random' singola. È cruciale crearla una sola volta
        // e passarla (Dependency Injection) ai nostri LavaPoint,
        // altrimenti se creati nello stesso millisecondo avrebbero tutti lo stesso seme.
        private Random random = new Random();

        // Costanti di rendering. PIXEL_SIZE = 2 significa che il nostro
        // rendering "pixel art" disegnerà blocchi 2x2.
        private const int PIXEL_SIZE = 2;

        // Il pennello che useremo per disegnare. Lo teniamo come variabile
        // per poter cambiare il colore dinamicamente.
        private SolidBrush lavaBrush;

        // Il rettangolo (bounds) dove l'animazione è permessa.
        private Rectangle lavaBounds;

        // --- Stato dell'Applicazione (Macchina a Stati) ---
        private const string CONFIG_FILE = "lavalamp.config.json"; // Dove salviamo le impostazioni
        private bool inSetupMode = false; // La nostra flag di stato: 'true' = Setup, 'false' = Lava
        private Config currentConfig;     // Oggetto che tiene in memoria la configurazione caricata

        // --- Stato del Dragging (per Setup Mode) ---
        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        // --- Costruttore ---
        public Form1()
        {
            // Metodo auto-generato dal Designer.
            // Inizializza tutti i componenti che abbiamo trascinato
            // (NotifyIcon, ContextMenuStrip, ecc.)
            InitializeComponent();
        }

        // --- Override Fondamentale: CreateParams ---
        // Questo è il modo "corretto" e robusto per nascondere l'app dalla taskbar.
        // Stiamo intercettando i parametri di creazione della finestra PRIMA
        // che venga creata, e aggiungiamo il flag 'WS_EX_TOOLWINDOW'
        // usando una bitmask (l'operatore '|' OR).
        // Questo è molto più stabile di 'this.ShowInTaskbar = false;',
        // che causava il bug del teletrasporto all'angolo (0,0).
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        // --- Entry Point Logico dell'Applicazione ---
        private void Form1_Load(object sender, EventArgs e)
        {
            // Questa è la nostra logica di "bootstrap".
            // Controlla se esiste un file di configurazione.
            if (File.Exists(CONFIG_FILE))
            {
                // Se SÌ:
                // 1. Leggi il file
                string json = File.ReadAllText(CONFIG_FILE);
                // 2. Deserializza il JSON nell'oggetto 'currentConfig'
                currentConfig = JsonConvert.DeserializeObject<Config>(json);
                // 3. Controlla i valori di default (per retro-compatibilità)
                if (currentConfig.LavaColorArgb == 0)
                {
                    currentConfig.LavaColorArgb = Color.FromArgb(255, 245, 110, 30).ToArgb();
                }
                // 4. Entra in modalità "Lava Lamp"
                EnterLavaMode(currentConfig);
            }
            else
            {
                // Se NO (primo avvio, o file cancellato):
                // 1. Entra in modalità "Setup"
                EnterSetupMode();
            }
        }

        // --- STATO 1: MODALITÀ SETUP ---
        private void EnterSetupMode()
        {
            // Imposta la flag di stato
            inSetupMode = true;
            if (notifyIcon1 != null)
                notifyIcon1.Visible = false; // Nascondi l'icona della tray

            // --- Riconfigura la finestra per il Setup ---

            // 1. Rimuovi il click-through. Usiamo una bitmask AND (~ NOT)
            //    per rimuovere solo il flag WS_EX_TRANSPARENT,
            //    lasciando tutti gli altri stili intatti.
            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle & ~WS_EX_TRANSPARENT);

            // 2. Rendi la finestra visibile e non-fantasma
            this.FormBorderStyle = FormBorderStyle.None; // Niente bordi (per bypassare i limiti di altezza di Windows)
            this.Text = "";
            this.BackColor = Color.Crimson; // Sfondo rosso visibile
            this.Opacity = 0.75;            // Semi-trasparente
            this.TopMost = true;            // Sempre sopra per il setup
            this.Size = new Size(300, 48);  // Dimensione di default

            // 3. "Attacca" (Subscribe) i gestori di eventi
            //    per il controllo manuale (mouse e tastiera).
            this.KeyPreview = true; // Permette al Form di intercettare i tasti
            this.KeyDown += Setup_KeyDown;
            this.MouseDown += Setup_MouseDown;
            this.MouseMove += Setup_MouseMove;
            this.MouseUp += Setup_MouseUp;
        }

        // --- Gestori Eventi per il Dragging (Setup Mode) ---
        private void Setup_MouseDown(object sender, MouseEventArgs e)
        {
            if (!inSetupMode) return;
            isDragging = true;
            dragCursorPoint = Cursor.Position; // Salva la posizione del mouse
            dragFormPoint = this.Location;   // Salva la posizione della finestra
        }

        private void Setup_MouseMove(object sender, MouseEventArgs e)
        {
            if (!inSetupMode || !isDragging) return;
            // Calcola il delta (differenza) tra ora e l'inizio del drag
            Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            // Applica il delta alla posizione originale della finestra
            this.Location = Point.Add(dragFormPoint, new Size(diff));
        }

        private void Setup_MouseUp(object sender, MouseEventArgs e)
        {
            if (!inSetupMode) return;
            isDragging = false;
        }

        // --- Gestore Eventi per Tasti (Setup Mode) ---
        private void Setup_KeyDown(object sender, KeyEventArgs e)
        {
            if (!inSetupMode) return;

            // Logica di Scaling (Ridimensionamento)
            if (e.Shift) // Se SHIFT è premuto
            {
                switch (e.KeyCode)
                {
                    case Keys.Left: this.Width -= 1; break;
                    case Keys.Right: this.Width += 1; break;
                    case Keys.Up: this.Height -= 1; break;
                    case Keys.Down: this.Height += 1; break;
                }
            }
            // Logica di Translating (Spostamento di precisione)
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Left: this.Left -= 1; break;
                    case Keys.Right: this.Left += 1; break;
                    case Keys.Up: this.Top -= 1; break;
                    case Keys.Down: this.Top += 1; break;
                }
            }

            // Logica di Salvataggio (Transizione di Stato)
            if (e.KeyCode == Keys.Enter)
            {
                // 1. "Sgancia" (Unsubscribe) i gestori di eventi
                //    per non consumare risorse.
                inSetupMode = false;
                this.KeyDown -= Setup_KeyDown;
                this.MouseDown -= Setup_MouseDown;
                this.MouseMove -= Setup_MouseMove;
                this.MouseUp -= Setup_MouseUp;

                // 2. Logica di "UPSERT" (Update/Insert)
                //    Preserva il colore salvato se esiste,
                //    altrimenti usa il default.
                int savedColor = (currentConfig != null && currentConfig.LavaColorArgb != 0)
                    ? currentConfig.LavaColorArgb
                    : Color.FromArgb(255, 245, 110, 30).ToArgb();

                // (Qui andrebbe anche la logica per salvare Speed/Size)

                // 3. Crea il nuovo oggetto Config con i dati della finestra
                Config config = new Config
                {
                    X = this.Location.X,
                    Y = this.Location.Y,
                    Width = this.Width,
                    Height = this.Height,
                    LavaColorArgb = savedColor
                };

                // 4. Serializza e salva su disco
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(CONFIG_FILE, json);

                // 5. Aggiorna la variabile di stato in memoria
                currentConfig = config;

                // 6. Transizione allo stato "Lava Mode"
                EnterLavaMode(config);
            }
        }

        // --- STATO 2: MODALITÀ LAVA LAMP ---
        private void EnterLavaMode(Config config)
        {
            // 1. Riconfigura la finestra per la modalità "Fantasma"
            this.Controls.Clear(); // Rimuove eventuali bottoni/testo
            this.Text = "";
            this.Location = new Point(config.X, config.Y);
            this.Size = new Size(config.Width, config.Height);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 1.0; // Opacità piena
            this.TopMost = true; // Sempre sopra

            // Ottimizzazione del rendering per ridurre lo sfarfallio
            this.DoubleBuffered = true;

            // 'BackColor' è il colore che 'OnPaint' usa per pulire.
            // 'TransparencyKey' dice a Windows: "ogni pixel di questo
            // colore (LimeGreen) deve diventare completamente trasparente".
            this.BackColor = Color.LimeGreen;
            this.TransparencyKey = Color.LimeGreen;

            // 2. Applica il click-through (WS_EX_TRANSPARENT)
            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_TRANSPARENT);

            // 3. Rendi visibile l'icona di controllo
            if (notifyIcon1 != null)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.Text = "Taskbar Lava Lamp";
            }

            // 4. Inizializza il pennello con il colore caricato
            Color lavaColor = Color.FromArgb(config.LavaColorArgb);
            lavaBrush = new SolidBrush(lavaColor);

            // 5. Avvia il "game loop"
            StartAnimation();
        }

        // --- Logica di Animazione (Game Loop) ---
        private void StartAnimation()
        {
            // Definisce i bordi della simulazione
            lavaBounds = this.ClientRectangle;
            // Crea le particelle
            InitializeLavaPoints();

            animationTimer.Interval = 33; // Target: ~30 FPS (1000ms / 30fps)
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        // Spawner delle particelle
        private void InitializeLavaPoints()
        {
            lavaPoints.Clear(); // Rimuovi le vecchie particelle
            if (lavaBounds.Width < 1) return;

            // Logica di scalabilità: numero di bolle
            // proporzionale alla larghezza della finestra.
            int numPoints = (int)(lavaBounds.Width / (80f / 5f)); // ~5 punti ogni 80px
            if (numPoints < 2) numPoints = 2; // Minimo
            if (numPoints > 50) numPoints = 50; // Massimo (per performance)

            for (int i = 0; i < numPoints; i++)
            {
                // Chiama il costruttore di LavaPoint
                // (Qui andrebbe la logica per passare Speed/Size da currentConfig)
                lavaPoints.Add(new LavaPoint(lavaBounds, random));
            }
        }

        // --- L'UPDATE LOOP (30 volte al secondo) ---
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (lavaBounds.Width < 1) return;

            // Soluzione "Brute-force" per il bug del focus.
            // Costringe la finestra a tornare in primo piano
            // 30 volte al secondo, impedendo alla taskbar
            // di coprirla quando si clicca su un'altra app.
            this.BringToFront();

            // Fase di "Update" della simulazione:
            // Itera su tutte le particelle e aggiorna la loro fisica.
            foreach (var point in lavaPoints)
            {
                point.Update(lavaBounds);
            }

            // "Invalida" la finestra. Questo dice a Windows:
            // "Il contenuto è cambiato, devi ridisegnarla".
            // Questo comando forza la chiamata a OnPaint().
            this.Invalidate();
        }

        // --- IL RENDER LOOP (Chiamato da Invalidate) ---
        protected override void OnPaint(PaintEventArgs e)
        {
            // 1. Pulisci la tela
            //    Pulisce lo sfondo col 'BackColor'
            //    (Crimson in setup, LimeGreen in lava)
            base.OnPaint(e);

            // 2. Macchina a stati per il rendering
            if (inSetupMode)
            {
                // Render dello stato Setup: Disegna le istruzioni
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                string helpText = "Trascina per muovere\nSHIFT + Frecce per ridimensionare\nFrecce per muovere (precisione)\nINVIO per salvare";
                Font font = new Font("Arial", 8, FontStyle.Bold);
                e.Graphics.DrawString(helpText, font, Brushes.Black, new PointF(11, 11)); // Ombra
                e.Graphics.DrawString(helpText, font, Brushes.White, new PointF(10, 10)); // Testo
            }
            else
            {
                // Render dello stato Lava: Esegui l'algoritmo Metaball
                if (lavaBounds.Width < 1) return;

                // Ottimizzazione GDI+: 'NearestNeighbor' è
                // essenziale per lo stile pixel art,
                // previene l'anti-aliasing (sfocatura).
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                // Iteriamo su una GRIGLIA di pixel (saltando di PIXEL_SIZE)
                for (int y = lavaBounds.Top; y < lavaBounds.Bottom; y += PIXEL_SIZE)
                {
                    for (int x = lavaBounds.Left; x < lavaBounds.Right; x += PIXEL_SIZE)
                    {
                        float totalInfluence = 0; // Campo potenziale

                        // Calcola l'influenza di TUTTE le particelle
                        // su questo singolo punto (x, y)
                        foreach (var point in lavaPoints)
                        {
                            float dx = x - point.Position.X;
                            float dy = y - point.Position.Y;
                            // Usiamo le distanze al quadrato per evitare
                            // una costosa operazione di Radice Quadrata (sqrt)
                            float distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared == 0) distanceSquared = 0.001f;

                            // Formula Metaball: Somma(r^2 / dist^2)
                            totalInfluence += (point.Radius * point.Radius) / distanceSquared;
                        }

                        // Applica la soglia (Thresholding)
                        // Se l'influenza totale supera 3.0...
                        if (totalInfluence > 3.0f)
                        {
                            // ...disegna il nostro "macro-pixel"
                            e.Graphics.FillRectangle(lavaBrush, x, y, PIXEL_SIZE, PIXEL_SIZE);
                        }
                    }
                }
            }
        }

        // --- GESTORI DEL MENU DELLA TRAY ---

        // Logica per il "Riposiziona"
        private void riposizionaLampadaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Invece di riavviare, eseguiamo una transizione
            // di stato non distruttiva (Lava -> Setup)
            animationTimer.Stop();
            EnterSetupMode();
        }

        // Logica per "Esci"
        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Pulisci l'icona prima di uscire
            if (notifyIcon1 != null)
                notifyIcon1.Visible = false;
            Application.Exit();
        }

        // Logica per "Impostazioni"
        private void impostazioniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 1. Ferma l'animazione e il TopMost (per sicurezza,
            //    anche se 'Opening' l'ha già fatto)
            animationTimer.Stop();
            this.TopMost = false;

            // 2. Apri la finestra 'SettingsForm' come
            //    finestra di dialogo MODALE (blocca l'esecuzione
            //    finché non viene chiusa).
            SettingsForm settingsWindow = new SettingsForm();
            settingsWindow.ShowDialog();

            // 3. Esecuzione riprende. Ricarica il file config
            //    (che SettingsForm ha appena salvato).
            if (File.Exists(CONFIG_FILE))
            {
                string json = File.ReadAllText(CONFIG_FILE);
                currentConfig = JsonConvert.DeserializeObject<Config>(json);

                // 4. Ricrea il pennello con il nuovo colore
                Color lavaColor = Color.FromArgb(currentConfig.LavaColorArgb);
                if (lavaBrush != null)
                    lavaBrush.Dispose(); // Libera la vecchia risorsa
                lavaBrush = new SolidBrush(lavaColor);
            }

            // 5. Riattiva e riavvia l'animazione
            this.TopMost = true;
            animationTimer.Start();
        }

        // --- CORREZIONE BUG DEL MENU ---
        // Gestori per il bug del menu che si chiude da solo.

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // PRIMA che il menu si apra:
            if (!inSetupMode)
            {
                // 1. Ferma l'animazione (e il 'BringToFront')
                animationTimer.Stop();
                // 2. Rimuovi temporaneamente 'TopMost'
                this.TopMost = false;
            }
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            // DOPO che il menu si è chiuso:
            if (!inSetupMode)
            {
                // 1. Riattiva 'TopMost'
                this.TopMost = true;
                // 2. Riavvia l'animazione
                animationTimer.Start();
            }
        }
    }
}