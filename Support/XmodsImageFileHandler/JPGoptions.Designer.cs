namespace Xmods.ImageFileHandler
{
    partial class JPGoptions
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
            this.label1 = new System.Windows.Forms.Label();
            this.Quality_trackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.Save_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.Quality_display = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Quality_trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(111, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "JPEG Save Options";
            // 
            // Quality_trackBar
            // 
            this.Quality_trackBar.Location = new System.Drawing.Point(74, 79);
            this.Quality_trackBar.Maximum = 100;
            this.Quality_trackBar.Name = "Quality_trackBar";
            this.Quality_trackBar.Size = new System.Drawing.Size(188, 56);
            this.Quality_trackBar.TabIndex = 1;
            this.Quality_trackBar.TickFrequency = 10;
            this.Quality_trackBar.Value = 90;
            this.Quality_trackBar.Scroll += new System.EventHandler(this.Quality_trackBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Quality:";
            // 
            // Save_button
            // 
            this.Save_button.Location = new System.Drawing.Point(71, 152);
            this.Save_button.Name = "Save_button";
            this.Save_button.Size = new System.Drawing.Size(90, 29);
            this.Save_button.TabIndex = 3;
            this.Save_button.Text = "Save";
            this.Save_button.UseVisualStyleBackColor = true;
            this.Save_button.Click += new System.EventHandler(this.Save_button_Click);
            // 
            // Cancel_button
            // 
            this.Cancel_button.Location = new System.Drawing.Point(194, 152);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(90, 29);
            this.Cancel_button.TabIndex = 4;
            this.Cancel_button.Text = "Cancel";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.Cancel_button_Click);
            // 
            // Quality_display
            // 
            this.Quality_display.Location = new System.Drawing.Point(269, 79);
            this.Quality_display.Name = "Quality_display";
            this.Quality_display.ReadOnly = true;
            this.Quality_display.Size = new System.Drawing.Size(55, 22);
            this.Quality_display.TabIndex = 5;
            // 
            // JPGoptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 216);
            this.Controls.Add(this.Quality_display);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.Save_button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Quality_trackBar);
            this.Controls.Add(this.label1);
            this.Name = "JPGoptions";
            this.Text = "JPG Options";
            ((System.ComponentModel.ISupportInitialize)(this.Quality_trackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar Quality_trackBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Save_button;
        private System.Windows.Forms.Button Cancel_button;
        private System.Windows.Forms.TextBox Quality_display;
    }
}