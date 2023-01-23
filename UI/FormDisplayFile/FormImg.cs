using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileSystemNS;

namespace UI
{
    public partial class FormImg : Form
    {
        private readonly File _file;
        private readonly ImageFormat _imageFormat;
        private readonly Image _img;

        public FormImg(File file)
        {
            _file = file;
            _imageFormat = _file.Format.ToImageFormat();
            InitializeComponent();
        }

        public FormImg(Image img)
        {
            InitializeComponent();
            _img = img;
        }

        private void FormImg_Load(object sender, EventArgs e)
        {
            if (_img != null)
            {
                PictureBox.Image = _img;
                return;
            }    

            if (_file.TryLoad()
                .IsError(error => MessageBox.Show(error)))
            {
                Close();
                return;
            }

            Image image = (Image)_file.Object;
            if (image is null)
            {
                Close();
                MessageBox.Show($"{nameof(Image)} is empty.");
                return;
            }

            PictureBox.Image = image;
        }
    }
}
