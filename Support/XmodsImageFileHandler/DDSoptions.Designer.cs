namespace Xmods.ImageFileHandler
{
    partial class DDSoptions
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
            this.DDScancel_button = new System.Windows.Forms.Button();
            this.DDSsaveGo_button = new System.Windows.Forms.Button();
            this.Compression_comboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Mipmap_checkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // DDScancel_button
            // 
            this.DDScancel_button.Location = new System.Drawing.Point(215, 175);
            this.DDScancel_button.Name = "DDScancel_button";
            this.DDScancel_button.Size = new System.Drawing.Size(119, 35);
            this.DDScancel_button.TabIndex = 9;
            this.DDScancel_button.Text = "Cancel";
            this.DDScancel_button.UseVisualStyleBackColor = true;
            this.DDScancel_button.Click += new System.EventHandler(this.DDScancel_button_Click);
            // 
            // DDSsaveGo_button
            // 
            this.DDSsaveGo_button.Location = new System.Drawing.Point(28, 175);
            this.DDSsaveGo_button.Name = "DDSsaveGo_button";
            this.DDSsaveGo_button.Size = new System.Drawing.Size(119, 35);
            this.DDSsaveGo_button.TabIndex = 8;
            this.DDSsaveGo_button.Text = "Save";
            this.DDSsaveGo_button.UseVisualStyleBackColor = true;
            this.DDSsaveGo_button.Click += new System.EventHandler(this.DDSsaveGo_button_Click);
            // 
            // Compression_comboBox
            // 
            this.Compression_comboBox.FormattingEnabled = true;
            this.Compression_comboBox.Items.AddRange(new object[] {
            "None",
            "DXT1",
            "DXT3",
            "DXT5"});
            this.Compression_comboBox.Location = new System.Drawing.Point(125, 85);
            this.Compression_comboBox.Name = "Compression_comboBox";
            this.Compression_comboBox.Size = new System.Drawing.Size(209, 24);
            this.Compression_comboBox.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Compression:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(108, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "DDS Save Options";
            // 
            // Mipmap_checkBox
            // 
            this.Mipmap_checkBox.AutoSize = true;
            this.Mipmap_checkBox.Checked = true;
            this.Mipmap_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Mipmap_checkBox.Location = new System.Drawing.Point(107, 126);
            this.Mipmap_checkBox.Name = "Mipmap_checkBox";
            this.Mipmap_checkBox.Size = new System.Drawing.Size(158, 21);
            this.Mipmap_checkBox.TabIndex = 11;
            this.Mipmap_checkBox.Text = "Generate Mipmaps?";
            this.Mipmap_checkBox.UseVisualStyleBackColor = true;
            // 
            // DDSoptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 250);
            this.Controls.Add(this.Mipmap_checkBox);
            this.Controls.Add(this.DDScancel_button);
            this.Controls.Add(this.DDSsaveGo_button);
            this.Controls.Add(this.Compression_comboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "DDSoptions";
            this.Text = "DDS save options";
            this.Load += new System.EventHandler(this.DDSoptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DDScancel_button;
        private System.Windows.Forms.Button DDSsaveGo_button;
        private System.Windows.Forms.ComboBox Compression_comboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox Mipmap_checkBox;
    }
}