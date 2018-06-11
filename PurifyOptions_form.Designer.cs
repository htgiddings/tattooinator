namespace Tattooinator
{
    partial class PurifyOptions_form
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
            this.PurifyAmount_trackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PurifyAlpha_checkBox = new System.Windows.Forms.CheckBox();
            this.PurifyBlue_checkBox = new System.Windows.Forms.CheckBox();
            this.PurifyGreen_checkBox = new System.Windows.Forms.CheckBox();
            this.PurifyRed_checkBox = new System.Windows.Forms.CheckBox();
            this.Accept_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.Preview_pictureBox = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Preview_checkBox = new System.Windows.Forms.CheckBox();
            this.BGcolor_panel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Brightness_trackBar = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.PurifyAmount_trackBar)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Preview_pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Brightness_trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // PurifyAmount_trackBar
            // 
            this.PurifyAmount_trackBar.Location = new System.Drawing.Point(102, 45);
            this.PurifyAmount_trackBar.Maximum = 50;
            this.PurifyAmount_trackBar.Minimum = -50;
            this.PurifyAmount_trackBar.Name = "PurifyAmount_trackBar";
            this.PurifyAmount_trackBar.Size = new System.Drawing.Size(278, 56);
            this.PurifyAmount_trackBar.TabIndex = 1;
            this.PurifyAmount_trackBar.TickFrequency = 10;
            this.PurifyAmount_trackBar.ValueChanged += new System.EventHandler(this.PurifyAmount_trackBar_Changed);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 196);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Channels:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.PurifyAlpha_checkBox);
            this.panel1.Controls.Add(this.PurifyBlue_checkBox);
            this.panel1.Controls.Add(this.PurifyGreen_checkBox);
            this.panel1.Controls.Add(this.PurifyRed_checkBox);
            this.panel1.Location = new System.Drawing.Point(93, 182);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(281, 37);
            this.panel1.TabIndex = 5;
            // 
            // PurifyAlpha_checkBox
            // 
            this.PurifyAlpha_checkBox.AutoSize = true;
            this.PurifyAlpha_checkBox.Location = new System.Drawing.Point(209, 13);
            this.PurifyAlpha_checkBox.Name = "PurifyAlpha_checkBox";
            this.PurifyAlpha_checkBox.Size = new System.Drawing.Size(66, 21);
            this.PurifyAlpha_checkBox.TabIndex = 3;
            this.PurifyAlpha_checkBox.Text = "Alpha";
            this.PurifyAlpha_checkBox.UseVisualStyleBackColor = true;
            this.PurifyAlpha_checkBox.CheckedChanged += new System.EventHandler(this.PurifyAlpha_checkBox_CheckedChanged);
            // 
            // PurifyBlue_checkBox
            // 
            this.PurifyBlue_checkBox.AutoSize = true;
            this.PurifyBlue_checkBox.Location = new System.Drawing.Point(144, 13);
            this.PurifyBlue_checkBox.Name = "PurifyBlue_checkBox";
            this.PurifyBlue_checkBox.Size = new System.Drawing.Size(58, 21);
            this.PurifyBlue_checkBox.TabIndex = 2;
            this.PurifyBlue_checkBox.Text = "Blue";
            this.PurifyBlue_checkBox.UseVisualStyleBackColor = true;
            this.PurifyBlue_checkBox.CheckedChanged += new System.EventHandler(this.PurifyBlue_checkBox_CheckedChanged);
            // 
            // PurifyGreen_checkBox
            // 
            this.PurifyGreen_checkBox.AutoSize = true;
            this.PurifyGreen_checkBox.Location = new System.Drawing.Point(67, 13);
            this.PurifyGreen_checkBox.Name = "PurifyGreen_checkBox";
            this.PurifyGreen_checkBox.Size = new System.Drawing.Size(70, 21);
            this.PurifyGreen_checkBox.TabIndex = 1;
            this.PurifyGreen_checkBox.Text = "Green";
            this.PurifyGreen_checkBox.UseVisualStyleBackColor = true;
            this.PurifyGreen_checkBox.CheckedChanged += new System.EventHandler(this.PurifyGreen_checkBox_CheckedChanged);
            // 
            // PurifyRed_checkBox
            // 
            this.PurifyRed_checkBox.AutoSize = true;
            this.PurifyRed_checkBox.Location = new System.Drawing.Point(4, 13);
            this.PurifyRed_checkBox.Name = "PurifyRed_checkBox";
            this.PurifyRed_checkBox.Size = new System.Drawing.Size(56, 21);
            this.PurifyRed_checkBox.TabIndex = 0;
            this.PurifyRed_checkBox.Text = "Red";
            this.PurifyRed_checkBox.UseVisualStyleBackColor = true;
            this.PurifyRed_checkBox.CheckedChanged += new System.EventHandler(this.PurifyRed_checkBox_CheckedChanged);
            // 
            // Accept_button
            // 
            this.Accept_button.Location = new System.Drawing.Point(79, 304);
            this.Accept_button.Name = "Accept_button";
            this.Accept_button.Size = new System.Drawing.Size(115, 36);
            this.Accept_button.TabIndex = 9;
            this.Accept_button.Text = "Accept";
            this.Accept_button.UseVisualStyleBackColor = true;
            this.Accept_button.Click += new System.EventHandler(this.Accept_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(249, 304);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(115, 36);
            this.Cancel_button.TabIndex = 10;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // Preview_pictureBox
            // 
            this.Preview_pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Preview_pictureBox.Location = new System.Drawing.Point(386, 12);
            this.Preview_pictureBox.Name = "Preview_pictureBox";
            this.Preview_pictureBox.Size = new System.Drawing.Size(350, 350);
            this.Preview_pictureBox.TabIndex = 6;
            this.Preview_pictureBox.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(11, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Contrast";
            // 
            // Preview_checkBox
            // 
            this.Preview_checkBox.AutoSize = true;
            this.Preview_checkBox.Location = new System.Drawing.Point(93, 246);
            this.Preview_checkBox.Name = "Preview_checkBox";
            this.Preview_checkBox.Size = new System.Drawing.Size(79, 21);
            this.Preview_checkBox.TabIndex = 6;
            this.Preview_checkBox.Text = "Preview";
            this.Preview_checkBox.UseVisualStyleBackColor = true;
            // 
            // BGcolor_panel
            // 
            this.BGcolor_panel.BackColor = System.Drawing.Color.Tan;
            this.BGcolor_panel.Location = new System.Drawing.Point(320, 241);
            this.BGcolor_panel.Name = "BGcolor_panel";
            this.BGcolor_panel.Size = new System.Drawing.Size(36, 34);
            this.BGcolor_panel.TabIndex = 8;
            this.BGcolor_panel.Click += new System.EventHandler(this.BGcolor_panel_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(230, 250);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Background:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Intensity";
            // 
            // Brightness_trackBar
            // 
            this.Brightness_trackBar.Location = new System.Drawing.Point(102, 109);
            this.Brightness_trackBar.Maximum = 50;
            this.Brightness_trackBar.Minimum = -50;
            this.Brightness_trackBar.Name = "Brightness_trackBar";
            this.Brightness_trackBar.Size = new System.Drawing.Size(278, 56);
            this.Brightness_trackBar.TabIndex = 3;
            this.Brightness_trackBar.TickFrequency = 10;
            this.Brightness_trackBar.ValueChanged += new System.EventHandler(this.Brightness_trackBar_Changed);
            // 
            // PurifyOptions_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 374);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Brightness_trackBar);
            this.Controls.Add(this.BGcolor_panel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Preview_checkBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Preview_pictureBox);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Accept_button);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PurifyAmount_trackBar);
            this.Name = "PurifyOptions_form";
            this.Text = "Contrast/Intensity Options";
            ((System.ComponentModel.ISupportInitialize)(this.PurifyAmount_trackBar)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Preview_pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Brightness_trackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar PurifyAmount_trackBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox PurifyAlpha_checkBox;
        private System.Windows.Forms.CheckBox PurifyBlue_checkBox;
        private System.Windows.Forms.CheckBox PurifyGreen_checkBox;
        private System.Windows.Forms.CheckBox PurifyRed_checkBox;
        private System.Windows.Forms.Button Accept_button;
        private System.Windows.Forms.Button Cancel_button;
        private System.Windows.Forms.PictureBox Preview_pictureBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox Preview_checkBox;
        private System.Windows.Forms.Panel BGcolor_panel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar Brightness_trackBar;
    }
}