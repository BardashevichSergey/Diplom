using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using System.Diagnostics;
using Emgu.CV.Util;

namespace GraphNodes
{
    public partial class ImageForm : Form
    {
        ImageBox box;
        public event EventHandler ChoosenSourse;
        public ImageForm()
        {
            InitializeComponent();
        }
        public void SetImage( IImage image)
        {
            imageBox1.ClientSize = image.Size;
            imageBox1.Image = image;
        }

        public Bitmap GetImage()
        {
            return box.Image.Bitmap;
        }
        private void buttonChooseSrc_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog(); 
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"; 
            if (open_dialog.ShowDialog() == DialogResult.OK) 
            {
                box.Image = new Image<Bgr,byte>(open_dialog.FileName);
                OnButtonClicked(EventArgs.Empty);
            }

        }
        protected void OnButtonClicked(EventArgs e)
        {
            var evt = ChoosenSourse;
            if (evt != null) evt(this, e);
        }
    }
}
