﻿namespace UI
{
    partial class FormRtf
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
            this.SuspendLayout();
            // 
            // FormRtf
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.KeyPreview = true;
            this.Name = "FormRtf";
            this.Text = "FormRtf";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRtf_FormClosing);
            this.Load += new System.EventHandler(this.FormRtf_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormRtf_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}