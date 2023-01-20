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
    public partial class FormTxt : Form
    {
        private readonly File _file;
        private bool _textWasChanged;

        public FormTxt(File file)
        {
            _file = file.Format == FileFormat.Txt ? file : throw new InvalidOperationException();

            InitializeComponent();
            Text = _file.Name;
            textBox.MaxLength = int.MaxValue;
        }

        private void FormTxt_Load(object sender, EventArgs e)
        {
            if (_file.TryLoad().IsError())
            {
                Close();
                return;
            }

            textBox.Text = (string)_file.Object;
            textBox.TextChanged += (object sender1, EventArgs e1) =>
            {
                if (!_textWasChanged)
                    Text = '*' + Text;

                _textWasChanged = true;
            };
        }

        private void FormTxt_KeyDown(object sender, KeyEventArgs e)
        {

            if (!e.Control || e.KeyCode != Keys.S)
                return;

            if (_textWasChanged)
                Text = Text.Substring_(1);

            e.Handled = true;
            e.SuppressKeyPress = true;

            _textWasChanged = false;
            if (_file.TrySetObject(textBox.Text).IsError() || 
                _file.TrySave().IsError())
                Close();
        }

        private void FormTxt_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_textWasChanged && 
                (_file.TrySetObject(textBox.Text).IsError() ||
                _file.TrySave().IsError()))
                DialogResult = DialogResult.Abort;
        }
    }
}
