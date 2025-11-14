namespace TaskbarLavaLamp
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Button btnSelectColor;
            this.panelColorPreview = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            btnSelectColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSelectColor
            // 
            btnSelectColor.Location = new System.Drawing.Point(20, 57);
            btnSelectColor.Name = "btnSelectColor";
            btnSelectColor.Size = new System.Drawing.Size(112, 23);
            btnSelectColor.TabIndex = 0;
            btnSelectColor.Text = "Scegli Colore...";
            btnSelectColor.UseVisualStyleBackColor = true;
            btnSelectColor.Click += new System.EventHandler(this.btnSelectColor_Click);
            // 
            // panelColorPreview
            // 
            this.panelColorPreview.Location = new System.Drawing.Point(20, 132);
            this.panelColorPreview.Name = "panelColorPreview";
            this.panelColorPreview.Size = new System.Drawing.Size(200, 100);
            this.panelColorPreview.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(20, 294);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Salva e Chiudi";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.panelColorPreview);
            this.Controls.Add(btnSelectColor);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelColorPreview;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}