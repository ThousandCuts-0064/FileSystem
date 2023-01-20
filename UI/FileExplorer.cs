using System;
using System.Windows.Forms;
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

        private Flags _flags;
        private Directory _directory;

        public FileExplorer(Directory directory)
        {
            InitializeComponent();
            _directory = directory;
            TreeViewDirectory.ImageList = SystemImages.List;
            ListViewObjects.SmallImageList = SystemImages.List;
            ListViewObjects.LargeImageList = SystemImages.List;
            ListViewObjects.Columns.Add("Name");
            ListViewObjects.Columns.Add("Type");
            ListViewObjects.Columns.Add("Size");

            var viewItems = ((ToolStripDropDownItem)MenuStrip.Items[nameof(View)]).DropDownItems;
            var viewNames = Enum.GetNames(typeof(View));

            for (int i = 0; i < viewNames.Length; i++)
            {
                View view = (View)i;
                viewItems.Add(viewNames[i]).Click += (object sender, EventArgs e) => ListViewObjects.View = view;
            }

           var newItems = ((ToolStripDropDownItem)ContextListObjects.Items[nameof(New)]).DropDownItems;
            newItems.Add(DIRECTORY).Name = DIRECTORY;
            newItems.Add(new ToolStripSeparator() { Enabled = false });
            newItems.Add(TEXT).Name = TEXT;
            newItems.Add(RICH_TEXT).Name = RICH_TEXT;
        }

        private void FileExplorer_Load(object sender, EventArgs e)
        {
            var node = TreeViewDirectory.Nodes.Add(_directory.Name, _directory.Name, nameof(Directory), nameof(Directory));

            if (_directory.Directories.Count > 0)
            {
                string placeHolder = GetPlaceHolder(node.Name);
                node.Nodes.Add(placeHolder, placeHolder);
            }

            ReloadListViewObjects();
        }

        private void FileExplorer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextListObjects.Show(e.Location, ToolStripDropDownDirection.Default);
        }

        private void Show_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == Formats) _flags ^= Flags.Formats;
            else if (e.ClickedItem == Hidden) _flags ^= Flags.Hidden;
            else throw new InvalidOperationException();

            ReloadListViewObjects();
        }

        private void New_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Name)
            {
                case DIRECTORY:
                    break;

                case TEXT:
                    string name = "New Text.txt";
                    var result = _directory.TryCreateFile(name, out _);
                    result.IsError(error => MessageBox.Show(error), name);
                    break;

                case RICH_TEXT:
                    break;

                default: throw new UnreachableException();
            }

            ReloadListViewObjects();
        }

        private void TreeViewDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (_directory.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _).IsError())
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
            if (_directory.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _).IsError())
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
            if (_directory.TryFindDirectory(e.Node.FullPath, out Directory curDir, out _).IsError())
            {
                Close();
                return;
            }

            if (e.Node.TreeView.HitTest(e.Location).Location == TreeViewHitTestLocations.PlusMinus)
                return;

            _directory = curDir;
            ReloadListViewObjects();
        }

        private void ListViewObjects_ItemActivate(object sender, EventArgs e)
        {
            if (_directory.TryFindObject(ListViewObjects.FocusedItem.Name, out var obj).IsError())
            {
                Close();
                return;
            }

            if (obj is Directory dir)
            {
                _directory = dir;
                ReloadListViewObjects();
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

        private void ReloadListViewObjects()
        {
            ListViewObjects.Items.Clear();

            for (int i = 0; i < _directory.Directories.Count; i++)
            {
                Directory dir = _directory.Directories[i];
                ListViewObjects.Items
                    .Add(dir.Name, dir.Name, nameof(Directory))
                    .SubItems.AddRange(new string[] { nameof(Directory), dir.ByteCount.ToString() });
            }

            for (int i = 0; i < _directory.Files.Count; i++)
            {
                File file = _directory.Files[i];
                ListViewObjects.Items
                    .Add(file.Name, _flags.HasFlag(Flags.Formats) ? file.Name : File.GetPureName(file.Name), SystemImages.FormatToKey(file.Format))
                    .SubItems.AddRange(new string[] { file.Format.ToString(), file.ByteCount.ToString() });
            }
        }

        private string GetPlaceHolder(string name) => '/' + name;

        private enum Flags
        {
            None = 0,
            Formats = 1 << 0,
            Hidden = 1 << 1
        }
    }
}
