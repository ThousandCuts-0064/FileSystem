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
            this.ContextListItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ContextListRename = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextListObjects.SuspendLayout();
            this.MenuMain.SuspendLayout();
            this.MenuDirectory.SuspendLayout();
            this.ContextListItem.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeViewDirectory
            // 
            this.TreeViewDirectory.Dock = System.Windows.Forms.DockStyle.Left;
            this.TreeViewDirectory.Location = new System.Drawing.Point(0, 59);
            this.TreeViewDirectory.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TreeViewDirectory.Name = "TreeViewDirectory";
            this.TreeViewDirectory.Size = new System.Drawing.Size(220, 391);
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
            this.ListViewObjects.LabelEdit = true;
            this.ListViewObjects.Location = new System.Drawing.Point(220, 59);
            this.ListViewObjects.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ListViewObjects.Name = "ListViewObjects";
            this.ListViewObjects.Size = new System.Drawing.Size(580, 391);
            this.ListViewObjects.TabIndex = 0;
            this.ListViewObjects.UseCompatibleStateImageBehavior = false;
            this.ListViewObjects.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ListViewObjects_AfterLabelEdit);
            this.ListViewObjects.ItemActivate += new System.EventHandler(this.ListViewObjects_ItemActivate);
            this.ListViewObjects.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListViewObjects_MouseDown);
            // 
            // ContextListObjects
            // 
            this.ContextListObjects.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextListObjects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContextListNew});
            this.ContextListObjects.Name = "ContextMenuStrip";
            this.ContextListObjects.Size = new System.Drawing.Size(211, 56);
            // 
            // ContextListNew
            // 
            this.ContextListNew.Name = "ContextListNew";
            this.ContextListNew.Size = new System.Drawing.Size(108, 24);
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
            this.MenuMain.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.MenuMain.Size = new System.Drawing.Size(800, 28);
            this.MenuMain.TabIndex = 2;
            this.MenuMain.Text = "MenuStrip";
            // 
            // MenuView
            // 
            this.MenuView.Name = "MenuView";
            this.MenuView.Size = new System.Drawing.Size(55, 24);
            this.MenuView.Text = "View";
            // 
            // MenuShow
            // 
            this.MenuShow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Formats,
            this.Hidden});
            this.MenuShow.Name = "MenuShow";
            this.MenuShow.Size = new System.Drawing.Size(59, 24);
            this.MenuShow.Text = "Show";
            this.MenuShow.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MenuShow_DropDownItemClicked);
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
            // MenuDirectory
            // 
            this.MenuDirectory.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MenuDirectory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuBack,
            this.MenuForward,
            this.MenuParent,
            this.MenuPath});
            this.MenuDirectory.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.MenuDirectory.Location = new System.Drawing.Point(0, 28);
            this.MenuDirectory.Name = "MenuDirectory";
            this.MenuDirectory.Size = new System.Drawing.Size(800, 31);
            this.MenuDirectory.TabIndex = 3;
            this.MenuDirectory.Text = "MenuStripDirectory";
            // 
            // MenuBack
            // 
            this.MenuBack.Enabled = false;
            this.MenuBack.Name = "MenuBack";
            this.MenuBack.Size = new System.Drawing.Size(54, 27);
            this.MenuBack.Text = "Back";
            this.MenuBack.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuBack_MouseDown);
            // 
            // MenuForward
            // 
            this.MenuForward.Enabled = false;
            this.MenuForward.Name = "MenuForward";
            this.MenuForward.Size = new System.Drawing.Size(77, 27);
            this.MenuForward.Text = "Forward";
            this.MenuForward.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuForward_MouseDown);
            // 
            // MenuParent
            // 
            this.MenuParent.Enabled = false;
            this.MenuParent.Name = "MenuParent";
            this.MenuParent.Size = new System.Drawing.Size(64, 27);
            this.MenuParent.Text = "Parent";
            this.MenuParent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MenuParent_MouseDown);
            // 
            // MenuPath
            // 
            this.MenuPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuPath.Name = "MenuPath";
            this.MenuPath.Size = new System.Drawing.Size(132, 27);
            this.MenuPath.Leave += new System.EventHandler(this.MenuPath_Leave);
            this.MenuPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MenuPath_KeyDown);
            // 
            // ContextListItem
            // 
            this.ContextListItem.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ContextListItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContextListRename});
            this.ContextListItem.Name = "ContextListItem";
            this.ContextListItem.Size = new System.Drawing.Size(133, 28);
            // 
            // ContextListRename
            // 
            this.ContextListRename.Name = "ContextListRename";
            this.ContextListRename.Size = new System.Drawing.Size(210, 24);
            this.ContextListRename.Text = "Rename";
            this.ContextListRename.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ContextListRename_MouseDown);
            // 
            // FileExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ListViewObjects);
            this.Controls.Add(this.TreeViewDirectory);
            this.Controls.Add(this.MenuDirectory);
            this.Controls.Add(this.MenuMain);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuMain;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FileExplorer";
            this.Text = "File Explorer";
            this.Load += new System.EventHandler(this.FileExplorer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FileExplorer_KeyDown);
            this.ContextListObjects.ResumeLayout(false);
            this.MenuMain.ResumeLayout(false);
            this.MenuMain.PerformLayout();
            this.MenuDirectory.ResumeLayout(false);
            this.MenuDirectory.PerformLayout();
            this.ContextListItem.ResumeLayout(false);
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
        private System.Windows.Forms.ContextMenuStrip ContextListItem;
        private System.Windows.Forms.ToolStripMenuItem ContextListRename;
    }
}