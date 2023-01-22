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
            this.ContextListNew = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuMain = new System.Windows.Forms.MenuStrip();
            this.MenuView = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuShow = new System.Windows.Forms.ToolStripMenuItem();
            this.Formats = new System.Windows.Forms.ToolStripMenuItem();
            this.Hidden = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuDirectory = new System.Windows.Forms.MenuStrip();
            this.MenuBack = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuForward = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuParent = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuPath = new System.Windows.Forms.ToolStripTextBox();
            this.ContextListObjects.SuspendLayout();
            this.MenuMain.SuspendLayout();
            this.MenuDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeViewDirectory
            // 
            this.TreeViewDirectory.Dock = System.Windows.Forms.DockStyle.Left;
            this.TreeViewDirectory.Location = new System.Drawing.Point(0, 51);
            this.TreeViewDirectory.Margin = new System.Windows.Forms.Padding(2);
            this.TreeViewDirectory.Name = "TreeViewDirectory";
            this.TreeViewDirectory.Size = new System.Drawing.Size(166, 315);
            this.TreeViewDirectory.TabIndex = 1;
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
            this.ListViewObjects.Location = new System.Drawing.Point(166, 51);
            this.ListViewObjects.Margin = new System.Windows.Forms.Padding(2);
            this.ListViewObjects.Name = "ListViewObjects";
            this.ListViewObjects.Size = new System.Drawing.Size(434, 315);
            this.ListViewObjects.TabIndex = 0;
            this.ListViewObjects.UseCompatibleStateImageBehavior = false;
            this.ListViewObjects.ItemActivate += new System.EventHandler(this.ListViewObjects_ItemActivate);
            // 
            // ContextListObjects
            // 
            this.ContextListObjects.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextListObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContextListNew});
            this.ContextListObjects.Name = "ContextMenuStrip";
            this.ContextListObjects.Size = new System.Drawing.Size(99, 26);
            // 
            // ContextListNew
            // 
            this.ContextListNew.Name = "ContextListNew";
            this.ContextListNew.Size = new System.Drawing.Size(98, 22);
            this.ContextListNew.Text = "New";
            this.ContextListNew.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ContextListNew_DropDownItemClicked);
            // 
            // MenuMain
            // 
            this.MenuMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuView,
            this.MenuShow});
            this.MenuMain.Location = new System.Drawing.Point(0, 0);
            this.MenuMain.Name = "MenuMain";
            this.MenuMain.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.MenuMain.Size = new System.Drawing.Size(600, 24);
            this.MenuMain.TabIndex = 2;
            this.MenuMain.Text = "MenuStrip";
            // 
            // MenuView
            // 
            this.MenuView.Name = "MenuView";
            this.MenuView.Size = new System.Drawing.Size(44, 20);
            this.MenuView.Text = "View";
            // 
            // MenuShow
            // 
            this.MenuShow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Formats,
            this.Hidden});
            this.MenuShow.Name = "MenuShow";
            this.MenuShow.Size = new System.Drawing.Size(48, 20);
            this.MenuShow.Text = "Show";
            this.MenuShow.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MenuShow_DropDownItemClicked);
            // 
            // Formats
            // 
            this.Formats.CheckOnClick = true;
            this.Formats.Name = "Formats";
            this.Formats.Size = new System.Drawing.Size(180, 22);
            this.Formats.Tag = "eee";
            this.Formats.Text = "Formats";
            // 
            // Hidden
            // 
            this.Hidden.CheckOnClick = true;
            this.Hidden.Name = "Hidden";
            this.Hidden.Size = new System.Drawing.Size(180, 22);
            this.Hidden.Text = "Hidden";
            // 
            // MenuDirectory
            // 
            this.MenuDirectory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuBack,
            this.MenuForward,
            this.MenuParent,
            this.MenuPath});
            this.MenuDirectory.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.MenuDirectory.Location = new System.Drawing.Point(0, 24);
            this.MenuDirectory.Name = "MenuDirectory";
            this.MenuDirectory.Size = new System.Drawing.Size(600, 27);
            this.MenuDirectory.TabIndex = 3;
            this.MenuDirectory.Text = "MenuStripDirectory";
            // 
            // MenuBack
            // 
            this.MenuBack.Enabled = false;
            this.MenuBack.Name = "MenuBack";
            this.MenuBack.Size = new System.Drawing.Size(44, 23);
            this.MenuBack.Text = "Back";
            this.MenuBack.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuBack_MouseDown);
            // 
            // MenuForward
            // 
            this.MenuForward.Enabled = false;
            this.MenuForward.Name = "MenuForward";
            this.MenuForward.Size = new System.Drawing.Size(62, 23);
            this.MenuForward.Text = "Forward";
            this.MenuForward.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuForward_MouseDown);
            // 
            // MenuParent
            // 
            this.MenuParent.Name = "MenuParent";
            this.MenuParent.Size = new System.Drawing.Size(53, 23);
            this.MenuParent.Text = "Parent";
            this.MenuParent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuParent_MouseDown);
            // 
            // MenuPath
            // 
            this.MenuPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuPath.Name = "MenuPath";
            this.MenuPath.Size = new System.Drawing.Size(100, 23);
            this.MenuPath.Leave += new System.EventHandler(this.MenuPath_Leave);
            this.MenuPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MenuPath_KeyDown);
            // 
            // FileExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.ListViewObjects);
            this.Controls.Add(this.TreeViewDirectory);
            this.Controls.Add(this.MenuDirectory);
            this.Controls.Add(this.MenuMain);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuMain;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FileExplorer";
            this.Text = "File Explorer";
            this.Load += new System.EventHandler(this.FileExplorer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileExplorer_KeyDown);
            this.ContextListObjects.ResumeLayout(false);
            this.MenuMain.ResumeLayout(false);
            this.MenuMain.PerformLayout();
            this.MenuDirectory.ResumeLayout(false);
            this.MenuDirectory.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView TreeViewDirectory;
        private System.Windows.Forms.ListView ListViewObjects;
        private System.Windows.Forms.MenuStrip MenuMain;
        private System.Windows.Forms.ToolStripMenuItem MenuView;
        private System.Windows.Forms.ToolStripMenuItem MenuShow;
        private System.Windows.Forms.ToolStripMenuItem Formats;
        private System.Windows.Forms.ToolStripMenuItem Hidden;
        private System.Windows.Forms.ContextMenuStrip ContextListObjects;
        private System.Windows.Forms.ToolStripMenuItem ContextListNew;
        private System.Windows.Forms.MenuStrip MenuDirectory;
        private System.Windows.Forms.ToolStripMenuItem MenuBack;
        private System.Windows.Forms.ToolStripMenuItem MenuForward;
        private System.Windows.Forms.ToolStripMenuItem MenuParent;
        private System.Windows.Forms.ToolStripTextBox MenuPath;
    }
}