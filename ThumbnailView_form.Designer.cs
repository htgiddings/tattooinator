namespace Tattooinator
{
    partial class ThumbnailView_form
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
            this.Thumb_pictureBox = new System.Windows.Forms.PictureBox();
            this.Okay_button = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Thumb_pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Thumb_pictureBox
            // 
            this.Thumb_pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Thumb_pictureBox.Location = new System.Drawing.Point(12, 12);
            this.Thumb_pictureBox.Name = "Thumb_pictureBox";
            this.Thumb_pictureBox.Size = new System.Drawing.Size(256, 256);
            this.Thumb_pictureBox.TabIndex = 0;
            this.Thumb_pictureBox.TabStop = false;
            // 
            // Okay_button
            // 
            this.Okay_button.Location = new System.Drawing.Point(82, 274);
            this.Okay_button.Name = "Okay_button";
            this.Okay_button.Size = new System.Drawing.Size(117, 34);
            this.Okay_button.TabIndex = 1;
            this.Okay_button.Text = "Okay";
            this.Okay_button.UseVisualStyleBackColor = true;
            this.Okay_button.Click += new System.EventHandler(this.Okay_button_Click);
            // 
            // ThumbnailView_form
            // 
            this.AcceptButton = this.Okay_button;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 320);
            this.Controls.Add(this.Okay_button);
            this.Controls.Add(this.Thumb_pictureBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThumbnailView_form";
            this.ShowIcon = false;
            this.Text = "Thumbnail Image";
            ((System.ComponentModel.ISupportInitialize)(this.Thumb_pictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Thumb_pictureBox;
        private System.Windows.Forms.Button Okay_button;
    }
}