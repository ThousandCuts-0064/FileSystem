
namespace UI
{
    partial class FormMain
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
            this.listShortcuts = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // listShortcuts
            // 
            this.listShortcuts.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listShortcuts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listShortcuts.Location = new System.Drawing.Point(0, 0);
            this.listShortcuts.MultiSelect = false;
            this.listShortcuts.Name = "listShortcuts";
            this.listShortcuts.Size = new System.Drawing.Size(1067, 554);
            this.listShortcuts.TabIndex = 0;
            this.listShortcuts.UseCompatibleStateImageBehavior = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.listShortcuts);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMain";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listShortcuts;
    }
}

