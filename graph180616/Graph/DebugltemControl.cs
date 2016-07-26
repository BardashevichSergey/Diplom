using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Graph.Items;

namespace Graph
{
    public partial class DebugltemControl : UserControl
    {
        Form f;
        PictureBox pb;
        public DebugltemControl(String title,Image image,List<Label> labelList)
        {
            InitializeComponent();
            
            groupBox1.Text = title;
            pictureBox1.Image = image;
                for (int i = 0; i < labelList.Count; i++)
                {
                    labelList[i].AutoSize = true;
                    tableLayoutPanel1.Controls.Add(labelList[i]);
                }
        }
        public void ChangeData(Node n)
        {
            
            List<Label> labelList = new List<Label>();
            foreach (NodeItem v in n.Items)
            {
                int tag = Convert.ToInt32(v.Tag);
                switch (tag)
                {
                    case 1:
                        Label labelname = new Label();
                        Label labelvalue = new Label();
                        NodeSliderItem nsi = (NodeSliderItem)v;
                        labelvalue.Text = nsi.Value.ToString("#.##");
                        labelname.Text = nsi.Text;
                        labelList.Add(labelname);
                        labelList.Add(labelvalue);
                        break;
                    default: break;
                }
            }
            groupBox1.Text = n.Title;
            pictureBox1.Image = n.Image;
            for (int i = 0; i < labelList.Count; i++)
            {
                Control c = tableLayoutPanel1.Controls[i];
                c.Text = labelList[i].Text;
            }
            this.Refresh();
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                f = new Form();
                f.FormBorderStyle = FormBorderStyle.None;
                f.Size = new Size((int)(pictureBox1.Image.Width * 0.6), (int)(pictureBox1.Image.Height * 0.6));
                //f.DesktopLocation = new Point(Cursor.Position.X+f.Size.Width, Cursor.Position.Y);

                f.StartPosition = FormStartPosition.Manual;
                f.Location = new Point(Cursor.Position.X - f.Size.Width, Cursor.Position.Y);

                pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Image = pictureBox1.Image;
                f.Controls.Add(pb);
                f.Show();
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if(f!=null)
                f.Close();
        }
    }
}
