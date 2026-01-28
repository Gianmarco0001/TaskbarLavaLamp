using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TaskbarLavaLamp
{
    public partial class SettingsForm : Form
    {
        private string _p;
        private Config c;

        public SettingsForm(string path)
        {
            InitializeComponent();
            _p = path;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(_p)) c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(_p));
            else c = new Config();

            // Inizializza preview colori (se presenti valori salvati)
            try
            {
                panelColorPreview.BackColor = Color.FromArgb(c.LavaColorArgb);
                panelColorPreview2.BackColor = Color.FromArgb(c.LavaColorArgb2);
            }
            catch { /* valori ARGB non validi: ignora */ }

            // Popola la combobox delle forme se vuota e seleziona la forma salvata
            if (comboBoxShape.Items.Count == 0)
            {
                comboBoxShape.Items.AddRange(Enum.GetValues(typeof(LavaShape)).Cast<object>().ToArray());
            }
            comboBoxShape.SelectedItem = c.SelectedShape;

            // Inizializza trackbar velocità/dimensione dai moltiplicatori salvati
            int speedVal = (int)Math.Round(c.SpeedMultiplier * 10f);
            if (speedVal < trackBarSpeed.Minimum) speedVal = trackBarSpeed.Minimum;
            if (speedVal > trackBarSpeed.Maximum) speedVal = trackBarSpeed.Maximum;
            trackBarSpeed.Value = speedVal;
            lblSpeed.Text = $"Velocità: {trackBarSpeed.Value * 0.1:F1}x";

            int sizeVal = (int)Math.Round(c.SizeMultiplier * 10f);
            if (sizeVal < trackBarSize.Minimum) sizeVal = trackBarSize.Minimum;
            if (sizeVal > trackBarSize.Maximum) sizeVal = trackBarSize.Maximum;
            trackBarSize.Value = sizeVal;
            lblSize.Text = $"Dimensione: {trackBarSize.Value * 0.1:F1}x";
        }

        private void btnSelectColor1_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    c.LavaColorArgb = cd.Color.ToArgb();
                    panelColorPreview.BackColor = cd.Color; // aggiorna preview immediatamente
                }
            }
        }

        private void btnSelectColor2_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    c.LavaColorArgb2 = cd.Color.ToArgb();
                    panelColorPreview2.BackColor = cd.Color; // aggiorna preview immediatamente
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Aggiorna configurazione dalle UI prima di salvare
            c.SpeedMultiplier = trackBarSpeed.Value * 0.1f;
            c.SizeMultiplier = trackBarSize.Value * 0.1f;
            if (comboBoxShape.SelectedItem is LavaShape ls) c.SelectedShape = ls;

            File.WriteAllText(_p, JsonConvert.SerializeObject(c, Formatting.Indented));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAddNewLamp_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.Show();
            // Assicuriamoci che la nuova finestra abbia il focus per ricevere input subito
            f.Activate();
        }
        
        
        private void btnFullScreen_Click(object sender, EventArgs e) { Form1 f = new Form1(); f.IsFullScreenMode = true; f.Show(); }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) { /* Logica Startup */ }

        private void trackBarSpeed_Scroll(object sender, EventArgs e)
        {
            // Aggiorna l'etichetta della velocità in base al valore della trackbar
            lblSpeed.Text = $"Velocità: {trackBarSpeed.Value * 0.1:F1}x";
        }

        private void trackBarSize_Scroll(object sender, EventArgs e)
        {
            // Aggiorna l'etichetta della dimensione in base al valore della trackbar
            lblSize.Text = $"Dimensione: {trackBarSize.Value * 0.1:F1}x";
        }
    }
}