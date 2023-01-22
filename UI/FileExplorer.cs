using System;
using System.Drawing;
using System.Windows.Forms;
using CustomCollections;
using ExceptionsNS;
using FileSystemNS;
using Text;

namespace UI
{
    public partial class FileExplorer : Form
    {
        private const string DIRECTORY = nameof(Directory);
        private const string TEXT = "Text";
        private const string RICH_TEXT = "Rich Text";

        private readonly List_<Directory> _historyList = new List_<Directory>();

        private Directory _curDir;
        private Flags _flags;
        private int _historyIndex = -1;

        public FileExplorer(Directory directory)
        {
            InitializeComponent();
            SetCurrentDirectory(directory);
            TreeViewDirectory.ImageList = SystemImages.List;
            ListViewObjects.SmallImageList = SystemImages.List;
            ListViewObjects.LargeImageList = SystemImages.List;
            ListViewObjects.Columns.Add("Name");
            ListViewObjects.Columns.Add("Type");
            ListViewObjects.Columns.Add("Size");
            MenuDirectory.Renderer = new MyRenderer();

            var viewItems = MenuView.DropDownItems;
            var viewNames = Enum.GetNames(typeof(View));

            for (int i = 0; i < viewNames.Length; i++)
            {
                View view = (View)i;
                viewItems.Add(viewNames[i]).Click += (object sender, EventArgs e) => ListViewObjects.View = view;
            }

            var newItems = ContextListNew.DropDownItems;
            newItems.Add(DIRECTORY).Name = DIRECTORY;
            newItems.Add(new ToolStripSeparator() { Enabled = false });
            newItems.Add(TEXT).Name = TEXT;
            newItems.Add(RICH_TEXT).Name = RICH_TEXT;
        }

        private void FileExplorer_Load(object sender, EventArgs e)
        {
            var node = TreeViewDirectory.Nodes.Add(_curDir.Name, _curDir.Name, nameof(Directory), nameof(Directory));

            if (_curDir.Directories.Count > 0)
            {
                string placeHolder = GetPlaceHolder(node.Name);
                node.Nodes.Add(placeHolder, placeHolder);
            }
        }

        private void FileExplorer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                switch (ActiveControl)
                {
                    case ListView listView:
                    {
                        if (listView.SelectedItems.Count == 0)
                            return;

                        ListViewItem[] selected = new ListViewItem[listView.SelectedItems.Count];
                        listView.SelectedItems.CopyTo(selected, 0);
                        for (int i = 0; i < selected.Length; i++)
                        {
                            var item = selected[i];
                            var result = item.Index < _curDir.Directories.Count
                                ? _curDir.TryRemoveDirectory(item.Name, out _)
                                : _curDir.TryRemoveFile(item.Name);

                            if (result.IsError(error => MessageBox.Show(error)))
                                return;

                            listView.Items.Remove(item);
                        }

                        break;
                    }

                    case TreeView treeView:
                    {
                        var result = _curDir.TryRemoveDirectory(treeView.SelectedNode.FullPath, out string faultedName, out Directory parent);
                        if (result.IsError(error => MessageBox.Show(error), faultedName))
                            return;

                        if (_curDir.IsChildOf(parent))
                            SetCurrentDirectory(parent);

                        treeView.SelectedNode.Remove();
                        break;
                    }

                    default: break;
                }
            }
        }

        private void TreeViewDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (_curDir.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _).IsError())
            {
                Close();
                return;
            }

            e.Node.Nodes.RemoveByKey(GetPlaceHolder(e.Node.Name));

            var nodes = e.Node.Nodes;
            for (int i = 0; i < curDir.Directories.Count; i++)
            {
                Directory dir = curDir.Directories[i];
                var node = nodes.Add(dir.Name, dir.Name, nameof(Directory), nameof(Directory));

                if (dir.Directories.Count > 0)
                {
                    string placeHolder = GetPlaceHolder(node.Name);
                    node.Nodes.Add(placeHolder, placeHolder);
                }
            }
        }

        private void TreeViewDirectory_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (_curDir.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _).IsError())
            {
                Close();
                return;
            }

            e.Node.Nodes.Clear();

            if (curDir.Directories.Count > 0)
            {
                string placeHolder = GetPlaceHolder(e.Node.Name);
                e.Node.Nodes.Add(placeHolder, placeHolder);
            }
        }

        private void TreeViewDirectory_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_curDir.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _)
                .IsError(error => MessageBox.Show(error)))
            {
                Close();
                return;
            }

            if (e.Node.TreeView.HitTest(e.Location).Location == TreeViewHitTestLocations.PlusMinus)
                return;

            SetCurrentDirectory(curDir);
        }

        private void ListViewObjects_ItemActivate(object sender, EventArgs e)
        {
            if (_curDir.TryFindObject(ListViewObjects.FocusedItem.Name, out var obj)
                .IsError(error => MessageBox.Show(error)))
            {
                Close();
                return;
            }

            if (obj is Directory dir)
            {
                SetCurrentDirectory(dir);
                return;
            }

            File file = (File)obj;
            switch (file.Format)
            {
                case FileFormat.Txt:
                    new FormTxt(file).ShowDialog();
                    break;

                case FileFormat.Rtf:
                    break;

                case FileFormat.Bmp:
                    break;
                case FileFormat.Emf:
                    break;
                case FileFormat.Wmf:
                    break;
                case FileFormat.Gif:
                    break;
                case FileFormat.Jpeg:
                    break;
                case FileFormat.Png:
                    break;
                case FileFormat.Tiff:
                    break;
                case FileFormat.Exif:
                    break;
                case FileFormat.Icon:
                    break;

                case FileFormat.Wav:
                    break;

                default: throw new UnreachableException();
            }
        }

        private void MenuShow_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == Formats) _flags ^= Flags.Formats;
            else if (e.ClickedItem == Hidden) _flags ^= Flags.Hidden;
            else throw new InvalidOperationException();

            ReloadListView();
        }

        private void ContextListNew_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case DIRECTORY:
                {
                    string name = _curDir.GetNameWithRepeatCount("New Directory");
                    var item = ListViewAddDirectory(name, 0);
                    var result = _curDir.TryCreateDirectory(name, out _);
                    if (result.IsError(error => MessageBox.Show(error), name))
                        ListViewObjects.Items.Remove(item);
                    break;
                }

                case TEXT:
                {
                    string name = _curDir.GetNameWithRepeatCount("New Text.txt");
                    var item = ListViewAddFile(name, FileFormat.Txt, 0);
                    var result = _curDir.TryCreateFile(name, out _);
                    if (result.IsError(error => MessageBox.Show(error), name))
                        ListViewObjects.Items.Remove(item);
                    break;
                }

                case RICH_TEXT:
                    break;

                default: throw new UnreachableException();
            }
        }

        private void MenuBack_MouseDown(object sender, MouseEventArgs e)
        {
            _curDir = _historyList[--_historyIndex];
            ReloadListView();
            if (_historyIndex == 0)
                MenuBack.Enabled = false;
            MenuForward.Enabled = true;
        }

        private void MenuForward_MouseDown(object sender, MouseEventArgs e)
        {
            _curDir = _historyList[++_historyIndex];
            ReloadListView();
            MenuBack.Enabled = true;
            if (_historyIndex == _historyList.Count - 1)
                MenuForward.Enabled = false;
        }

        private void MenuParent_MouseDown(object sender, MouseEventArgs e) =>
            SetCurrentDirectory(_curDir.Parent);

        private void MenuPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            var result = _curDir.TryFindDirectory(MenuPath.Text, out Directory directory, out string faultedName);
            if (result.IsError(error => MessageBox.Show(error), faultedName))
                MenuPath.Text = _curDir.FullName;
            else
                SetCurrentDirectory(directory);

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void MenuPath_Leave(object sender, EventArgs e) =>
            MenuPath.Text = _curDir.FullName;

        private void SetCurrentDirectory(Directory directory)
        {
            if (_curDir == directory)
                return;

            _curDir = directory;
            ReloadListView();

            MenuBack.Enabled = _historyIndex > -1;
            MenuParent.Enabled = _curDir.Parent != null;
            if (_historyIndex != _historyList.Count - 1)
            {
                _historyList.RemoveLast(_historyList.Count - 1 - _historyIndex);
                MenuForward.Enabled = false;
            }
            _historyIndex++;
            _historyList.Add(_curDir);
        }

        private void ReloadListView()
        {
            ListViewObjects.Items.Clear();

            MenuPath.Text = _curDir.FullName;

            for (int i = 0; i < _curDir.Directories.Count; i++)
            {
                Directory dir = _curDir.Directories[i];
                ListViewAddDirectory(dir.Name, dir.ByteCount);
            }

            for (int i = 0; i < _curDir.Files.Count; i++)
            {
                File file = _curDir.Files[i];
                ListViewAddFile(file.Name, file.Format, file.ByteCount);
            }
        }

        private ListViewItem ListViewAddDirectory(string name, long byteCount)
        {
            var item = ListViewObjects.Items.Add(name, name, nameof(Directory));
            item.SubItems.AddRange(new string[] { nameof(Directory), byteCount.ToString() });
            return item;
        }

        private ListViewItem ListViewAddFile(string name, FileFormat format, long byteCount)
        {
            var item = ListViewObjects.Items.Add(name, _flags.HasFlag(Flags.Formats) ? name : File.GetPureName(name), SystemImages.FormatToKey(format));
            item.SubItems.AddRange(new string[] { format.ToString(), byteCount.ToString() });
            return item;
        }

        private string GetPlaceHolder(string name) => '/' + name;

        private enum Flags
        {
            None = 0,
            Formats = 1 << 0,
            Hidden = 1 << 1
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public MyRenderer() : base(new MyColorTable()) { }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                base.OnRenderMenuItemBackground(e);
                if (e.Item.Enabled && e.Item.Selected)
                {
                    using (var pen = new Pen(((MyColorTable)ColorTable).MenuItemEnabledBorder))
                    {
                        var r = new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1);
                        e.Graphics.DrawRectangle(pen, r);
                    }
                }
            }

            private class MyColorTable : ProfessionalColorTable
            {
                public override Color MenuItemBorder => Color.Transparent;
                public Color MenuItemEnabledBorder => base.MenuItemBorder;
            }
        }
    }
}
