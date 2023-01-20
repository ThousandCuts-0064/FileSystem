
namespace UI
{
    partial class FileExplorer
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
            this.components = new System.ComponentModel.Container();
            this.TreeViewDirectory = new System.Windows.Forms.TreeView();
            this.ListViewObjects = new System.Windows.Forms.ListView();
            this.ContextListObjects = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.New = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.View = new System.Windows.Forms.ToolStripMenuItem();
            this.Show = new System.Windows.Forms.ToolStripMenuItem();
            this.Formats = new System.Windows.Forms.ToolStripMenuItem();
            this.Hidden = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextListObjects.SuspendLayout();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeViewDirectory
            // 
            this.TreeViewDirectory.Dock = System.Windows.Forms.DockStyle.Left;
            this.TreeViewDirectory.Location = new System.Drawing.Point(0, 28);
            this.TreeViewDirectory.Name = "TreeViewDirectory";
            this.TreeViewDirectory.Size = new System.Drawing.Size(258, 422);
            this.TreeViewDirectory.TabIndex = 0;
            this.TreeViewDirectory.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewDirectory_AfterCollapse);
            this.TreeViewDirectory.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeViewDirectory_BeforeExpand);
            this.TreeViewDirectory.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewDirectory_NodeMouseClick);
            // 
            // ListViewObjects
            // 
            this.ListViewObjects.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.ListViewObjects.ContextMenuStrip = this.ContextListObjects;
            this.ListViewObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListViewObjects.HideSelection = false;
            this.ListViewObjects.Location = new System.Drawing.Point(258, 28);
            this.ListViewObjects.Name = "ListViewObjects";
            this.ListViewObjects.Size = new System.Drawing.Size(542, 422);
            this.ListViewObjects.TabIndex = 1;
            this.ListViewObjects.UseCompatibleStateImageBehavior = false;
            this.ListViewObjects.ItemActivate += new System.EventHandler(this.ListViewObjects_ItemActivate);
            // 
            // ContextListObjects
            // 
            this.ContextListObjects.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextListObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.New});
            this.ContextListObjects.Name = "ContextMenuStrip";
            this.ContextListObjects.Size = new System.Drawing.Size(211, 56);
            // 
            // New
            // 
            this.New.Name = "New";
            this.New.Size = new System.Drawing.Size(210, 24);
            this.New.Text = "New";
            this.New.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.New_DropDownItemClicked);
            // 
            // MenuStrip
            // 
            this.MenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.View,
            this.Show});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(800, 28);
            this.MenuStrip.TabIndex = 2;
            this.MenuStrip.Text = "MenuStrip";
            // 
            // View
            // 
            this.View.Name = "View";
            this.View.Size = new System.Drawing.Size(55, 24);
            this.View.Text = "View";
            // 
            // Show
            // 
            this.Show.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Formats,
            this.Hidden});
            this.Show.Name = "Show";
            this.Show.Size = new System.Drawing.Size(59, 24);
            this.Show.Text = "Show";
            this.Show.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Show_DropDownItemClicked);
            // 
            // Formats
            // 
            this.Formats.CheckOnClick = true;
            this.Formats.Name = "Formats";
            this.Formats.Size = new System.Drawing.Size(145, 26);
            this.Formats.Tag = "eee";
            this.Formats.Text = "Formats";
            // 
            // Hidden
            // 
            this.Hidden.CheckOnClick = true;
            this.Hidden.Name = "Hidden";
            this.Hidden.Size = new System.Drawing.Size(145, 26);
            this.Hidden.Text = "Hidden";
            // 
            // FileExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ListViewObjects);
            this.Controls.Add(this.TreeViewDirectory);
            this.Controls.Add(this.MenuStrip);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "FileExplorer";
            this.Text = "File Explorer";
            this.Load += new System.EventHandler(this.FileExplorer_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FileExplorer_MouseDown);
            this.ContextListObjects.ResumeLayout(false);
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView TreeViewDirectory;
        private System.Windows.Forms.ListView ListViewObjects;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem View;
        private System.Windows.Forms.ToolStripMenuItem Show;
        private System.Windows.Forms.ToolStripMenuItem Formats;
        private System.Windows.Forms.ToolStripMenuItem Hidden;
        private System.Windows.Forms.ContextMenuStrip ContextListObjects;
        private System.Windows.Forms.ToolStripMenuItem New;
    }
}