using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileSystemNS;
using Text;

namespace UI
{
    public partial class FormRtf : Form
    {
        private readonly File _file;
        private RichTextBox _richTextBox;
        private bool _textWasChanged;

        public FormRtf(File file)
        {
            _file = file.Format == FileFormat.Rtf ? file : throw new InvalidOperationException();

            InitializeComponent();
            Text = _file.Name;
        }

        private void FormRtf_Load(object sender, EventArgs e)
        {
            if (_file.TryLoad().IsError())
            {
                Close();
                return;
            }

            _richTextBox = (RichTextBox)_file.Object;
            if (_richTextBox is null)
            {
                _richTextBox = new RichTextBox();
                if (_file.TrySetObject(_richTextBox).IsError())
                {
                    Close();
                    return;
                }
            }

            Controls.Add(_richTextBox);
            _richTextBox.Dock = DockStyle.Fill;
            _richTextBox.MaxLength = int.MaxValue;
            _richTextBox.TextChanged += (object sender1, EventArgs e1) =>
            {
                if (!_textWasChanged)
                    Text = '*' + Text;

                _textWasChanged = true;
            };
        }

        private void FormRtf_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.S)
                return;

            if (_textWasChanged)
                Text = Text.Substring_(1);

            e.Handled = true;
            e.SuppressKeyPress = true;

            _textWasChanged = false;
            if (_file.TrySave().IsError())
                Close();
        }

        private void FormRtf_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_textWasChanged &&
                _file.TrySave().IsError())
                DialogResult = DialogResult.Abort;
        }
    }
}
