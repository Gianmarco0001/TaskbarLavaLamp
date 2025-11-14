using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace TaskbarLavaLamp
{
    public partial class SettingsForm : Form
    {
        private const string CONFIG_FILE = "lavalamp.config.json";
        private Config currentConfig;
        private Color selectedColor;

        public SettingsForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Impostazioni";
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(CONFIG_FILE))
            {
                string json = File.ReadAllText(CONFIG_FILE);
                currentConfig = JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                currentConfig = new Config();
            }

            if (currentConfig.LavaColorArgb == 0)
            {
                selectedColor = Color.FromArgb(255, 245, 110, 30); // Default Arancione
            }
            else
            {
                selectedColor = Color.FromArgb(currentConfig.LavaColorArgb);
            }

            panelColorPreview.BackColor = selectedColor;
        }

        private void btnSelectColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = selectedColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedColor = colorDialog1.Color;
                panelColorPreview.BackColor = selectedColor;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            currentConfig.LavaColorArgb = selectedColor.ToArgb();

            string json = JsonConvert.SerializeObject(currentConfig, Formatting.Indented);
            File.WriteAllText(CONFIG_FILE, json);

            this.Close();
        }
    }
}