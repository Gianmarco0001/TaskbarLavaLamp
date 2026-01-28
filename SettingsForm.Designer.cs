namespace TaskbarLavaLamp
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.panelColorPreview = new System.Windows.Forms.Panel();
            this.panelColorPreview2 = new System.Windows.Forms.Panel();
            this.btnSelectColor1 = new System.Windows.Forms.Button();
            this.btnSelectColor2 = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.comboBoxShape = new System.Windows.Forms.ComboBox();
            this.trackBarSpeed = new System.Windows.Forms.TrackBar();
            this.trackBarSize = new System.Windows.Forms.TrackBar();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.btnAddNewLamp = new System.Windows.Forms.Button();
            this.btnFullScreen = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).BeginInit();
            this.SuspendLayout();
            // 
            // panelColorPreview
            // 
            this.panelColorPreview.Location = new System.Drawing.Point(20, 20);
            this.panelColorPreview.Name = "panelColorPreview";
            this.panelColorPreview.Size = new System.Drawing.Size(40, 40);
            this.panelColorPreview.TabIndex = 1;
            // 
            // panelColorPreview2
            // 
            this.panelColorPreview2.Location = new System.Drawing.Point(20, 70);
            this.panelColorPreview2.Name = "panelColorPreview2";
            this.panelColorPreview2.Size = new System.Drawing.Size(40, 40);
            this.panelColorPreview2.TabIndex = 4;
            // 
            // btnSelectColor1
            // 
            this.btnSelectColor1.Location = new System.Drawing.Point(70, 28);
            this.btnSelectColor1.Name = "btnSelectColor1";
            this.btnSelectColor1.Size = new System.Drawing.Size(120, 23);
            this.btnSelectColor1.TabIndex = 0;
            this.btnSelectColor1.Text = "Scegli Colore 1...";
            this.btnSelectColor1.Click += new System.EventHandler(this.btnSelectColor1_Click);
            // 
            // btnSelectColor2
            // 
            this.btnSelectColor2.Location = new System.Drawing.Point(70, 78);
            this.btnSelectColor2.Name = "btnSelectColor2";
            this.btnSelectColor2.Size = new System.Drawing.Size(120, 23);
            this.btnSelectColor2.TabIndex = 5;
            this.btnSelectColor2.Text = "Scegli Colore 2...";
            this.btnSelectColor2.Click += new System.EventHandler(this.btnSelectColor2_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(20, 434);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(210, 30);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Salva e Chiudi";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Location = new System.Drawing.Point(20, 379);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(150, 24);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Avvia con Windows";
            // 
            // comboBoxShape
            // 
            this.comboBoxShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShape.Location = new System.Drawing.Point(20, 130);
            this.comboBoxShape.Name = "comboBoxShape";
            this.comboBoxShape.Size = new System.Drawing.Size(170, 21);
            this.comboBoxShape.TabIndex = 6;
            // 
            // trackBarSpeed
            // 
            this.trackBarSpeed.Location = new System.Drawing.Point(20, 185);
            this.trackBarSpeed.Minimum = 1;
            this.trackBarSpeed.Name = "trackBarSpeed";
            this.trackBarSpeed.Size = new System.Drawing.Size(170, 45);
            this.trackBarSpeed.TabIndex = 7;
            this.trackBarSpeed.Value = 10;
            this.trackBarSpeed.Scroll += new System.EventHandler(this.trackBarSpeed_Scroll);
            // 
            // trackBarSize
            // 
            this.trackBarSize.Location = new System.Drawing.Point(20, 250);
            this.trackBarSize.Minimum = 1;
            this.trackBarSize.Name = "trackBarSize";
            this.trackBarSize.Size = new System.Drawing.Size(170, 45);
            this.trackBarSize.TabIndex = 8;
            this.trackBarSize.Value = 10;
            this.trackBarSize.Scroll += new System.EventHandler(this.trackBarSize_Scroll);
            // 
            // lblSpeed
            // 
            this.lblSpeed.Location = new System.Drawing.Point(20, 165);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(150, 20);
            this.lblSpeed.TabIndex = 12;
            this.lblSpeed.Text = "Velocità: 1.0x";
            // 
            // lblSize
            // 
            this.lblSize.Location = new System.Drawing.Point(20, 230);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(150, 20);
            this.lblSize.TabIndex = 11;
            this.lblSize.Text = "Dimensione: 1.0x";
            // 
            // btnAddNewLamp
            // 
            this.btnAddNewLamp.Location = new System.Drawing.Point(20, 310);
            this.btnAddNewLamp.Name = "btnAddNewLamp";
            this.btnAddNewLamp.Size = new System.Drawing.Size(100, 23);
            this.btnAddNewLamp.TabIndex = 9;
            this.btnAddNewLamp.Text = "+ Nuova Lampada";
            this.btnAddNewLamp.Click += new System.EventHandler(this.btnAddNewLamp_Click);
            // 
            // btnFullScreen
            // 
            this.btnFullScreen.Location = new System.Drawing.Point(130, 310);
            this.btnFullScreen.Name = "btnFullScreen";
            this.btnFullScreen.Size = new System.Drawing.Size(100, 23);
            this.btnFullScreen.TabIndex = 10;
            this.btnFullScreen.Text = "Full Screen";
            this.btnFullScreen.Click += new System.EventHandler(this.btnFullScreen_Click);
            // 
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(260, 494);
            this.Controls.Add(this.btnFullScreen);
            this.Controls.Add(this.btnAddNewLamp);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.trackBarSize);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.trackBarSpeed);
            this.Controls.Add(this.comboBoxShape);
            this.Controls.Add(this.btnSelectColor2);
            this.Controls.Add(this.panelColorPreview2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.panelColorPreview);
            this.Controls.Add(this.btnSelectColor1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Impostazioni";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.Panel panelColorPreview2;
        private System.Windows.Forms.Button btnSelectColor1;
        private System.Windows.Forms.Button btnSelectColor2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ComboBox comboBoxShape;
        private System.Windows.Forms.TrackBar trackBarSpeed;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Button btnAddNewLamp;
        private System.Windows.Forms.Button btnFullScreen;
    }
}