using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphNodes;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using Graph;
using Graph.Items;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Plugins
{
    [Export(typeof(IPlugin))]
    public class NodeSourse : IPlugin
    {
        #region IStringSender Members

        ListView selection;
        Node textureNode;
        static int Number = 1;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Узел-источник");
            selection.Items.Add(it);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if(lvi != null)
            {
                if (lvi.Text.Equals("Узел-источник"))
                {
                    textureNode = new Node("Image"+Number++);
                    textureNode.RemoveItem(textureNode.Items.ElementAt(1));
                    textureNode.AddItem(new NodeCheckboxItem("Изображение", false, true) { Tag = 42f });
                    var imageItem = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    textureNode.AddItem(imageItem);
                    var btn = new NodeButtonItem("Выбрать источник", false, false);
                    btn.Clicked += new EventHandler<NodeItemEventArgs>(OnBtnChooseSrcClicked);
                    textureNode.AddItem(btn);


                    selection.DoDragDrop(textureNode, DragDropEffects.Copy);
                    selection.SelectedItems.Clear();
                }
            }
        }
        void OnBtnChooseSrcClicked(object sender, NodeItemEventArgs e)
        {
            Node n = ((NodeItem)sender).Node;
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                n.Image = Image.FromFile(open_dialog.FileName);
                n.DoAction();
            }
        }
        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }

        #endregion
    }


    [Export(typeof(IPlugin))]
    public class NodeProcesserGaussFlur : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node gauss;
        static int Number = 1;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Размытие Гаусса");

            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Размытие Гаусса"))
                {
                    gauss = new Node("Размытие Гауcса" + Number++);
                    var widthItem = new NodeNumericSliderItem("По ширине", 70, 30, 1, 50, 10, false, false) { Tag = 1f };
                    widthItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    gauss.AddItem(widthItem);
                    var heightItem = new NodeNumericSliderItem("По высоте", 70, 30, 1, 50, 10, false, false) { Tag = 1f };
                    heightItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnHeigthChanget); ;
                    gauss.AddItem(heightItem);

                    var sigmaXItem = new NodeNumericSliderItem("sigmaX", 70, 30, 1, 50, 34.5f, false, false) { Tag = 1f };
                    heightItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnHeigthChanget); ;
                    gauss.AddItem(sigmaXItem);

                    var sigmaYItem = new NodeNumericSliderItem("sigmaY", 70, 30, 1, 50, 34.5f, false, false) { Tag = 1f };
                    heightItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnHeigthChanget); ;
                    gauss.AddItem(sigmaYItem);

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    gauss.AddItem(imageItem1);
                    gauss.AddProcess(ProcessGauss);

                    selection.DoDragDrop(gauss, DragDropEffects.Copy);
                    lvi.Selected = false;
                }
            }
        }

        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if(!connection.To.Node.Equals(n))
                    connection.To.Node.DoAction();
        }

        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {
            
            e.Item.Node.DoAction();
        }
        void OnHeigthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessGauss(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {
                    NodeNumericSliderItem w = null, h = null,sigmaX = null, sigmaY = null;
                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("По ширине"))
                                w = ns;
                            else if (ns.Text.Equals("По высоте"))
                                h = ns;
                            else if (ns.Text.Equals("sigmaX"))
                                sigmaX = ns;
                            else if (ns.Text.Equals("sigmaY"))
                                sigmaY = ns;
                        }
                    }
                    Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));
                    int kernelW = (int)(w.Value);
                    if (kernelW % 2 == 0)
                        kernelW += 1;
                    int kernelH = (int)(h.Value);
                    if (kernelH % 2 == 0)
                        kernelH += 1;
                    double sx = sigmaX.Value;
                    Image<Bgr, byte> blur = image.SmoothGaussian(kernelW, kernelH, sigmaX.Value, sigmaY.Value);
                    n.Image = blur.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }

    [Export(typeof(IPlugin))]
    public class NodeProcesSmoothBlur : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node src;
        static int Number = 1;

        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Размытие");
            selection.Items.Add(it);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Размытие"))
                {
                    src = new Node("Размытие" + Number++);
                    var widthItem = new NodeNumericSliderItem("По ширине", 70, 30, 1, 50, 10, false, false) { Tag = 1f };
                    widthItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    src.AddItem(widthItem);
                    var heightItem = new NodeNumericSliderItem("По высоте", 70, 30, 1, 50, 10, false, false) { Tag = 1f };
                    heightItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnHeigthChanget); ;
                    src.AddItem(heightItem);

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    src.AddItem(imageItem1);
                    src.AddProcess(ProcessSmoothBlur);
                    selection.DoDragDrop(src, DragDropEffects.Copy);
                    lvi.Selected = true;
                }
            }
        }

        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {

            e.Item.Node.DoAction();
        }
        void OnHeigthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                {
                    if (n.Image == null)
                        connection.To.Node.Image = null;
                    connection.To.Node.DoAction();
                }
        }


        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessSmoothBlur(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {
                    NodeNumericSliderItem w = null, h = null, sigma1 = null, sigma2 = null;
                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("По ширине"))
                                w = ns;
                            else if (ns.Text.Equals("По высоте"))
                                h = ns;
                        }
                    }
                    Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));
                    Image<Bgr, byte> blur = image.SmoothBlur(Convert.ToInt32(w.Value), Convert.ToInt32(h.Value));
                    n.Image = blur.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;

           
        }
        #endregion
    }

    [Export(typeof(IPlugin))]
    public class NodeProcesSmoothMedian : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node src;
        static int Number = 1;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Медианное размытие");
            selection.Items.Add(it);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Медианное размытие"))
                {
                    src = new Node("Медианное размытие" + Number++);
                    var widthItem = new NodeNumericSliderItem("Размер", 70, 30, 1, 50, 10, false, false) { Tag = 1f };
                    widthItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    src.AddItem(widthItem);

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    src.AddItem(imageItem1);
                    src.AddProcess(ProcessSmoothMedian);
                    selection.DoDragDrop(src, DragDropEffects.Copy);
                    lvi.Selected = true;
                }
            }
        }

        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {

            e.Item.Node.DoAction();
        }
        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                {
                    if (n.Image == null)
                        connection.To.Node.Image = null;
                    connection.To.Node.DoAction();
                }
        }


        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessSmoothMedian(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {
                    NodeNumericSliderItem size = null;
                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("Размер"))
                                size = ns;
                            
                        }
                    }
                    int size1 = (int)(size.Value);
                    if (size1 % 2 == 0)
                        size1 += 1;
                    Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));
                    Image<Bgr, byte> blur = image.SmoothMedian(size1);
                    n.Image = blur.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;


        }
        #endregion
    }

    [Export(typeof(IPlugin))]
    public class NodeProcesserCannyDetector : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node canny;
        static int Number = 1;

        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Детектор границ Кенни");
            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Детектор границ Кенни"))
                {
                    canny = new Node("Детектор границ Кенни(Canny)"+Number++);
                    var thresh = new NodeNumericSliderItem("Начальный Порог", 70, 30, 1, 100, 10, false, false) { Tag = 1f };
                    thresh.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    canny.AddItem(thresh);
                    var threshLinking = new NodeNumericSliderItem("Порог связывания границ", 70, 30, 1, 100, 10, false, false) { Tag = 1f };
                    threshLinking.ValueChanged += new EventHandler<NodeItemEventArgs>(OnHeigthChanget); ;
                    canny.AddItem(threshLinking);

                    var imageItem3 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem3.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    canny.AddItem(imageItem3);
                    canny.AddProcess(ProcessCanny);

                    selection.DoDragDrop(canny, DragDropEffects.Copy);
                    selection.SelectedItems.Clear();
                }
            }
        }
        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                    connection.To.Node.DoAction();
        }
        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        void OnHeigthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessCanny(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {
                    Image<Gray, Byte> image = new Image<Gray, Byte>(new Bitmap(img));
                    //Image<Gray, byte> gray_image = image.Convert<Gray, byte>();

                    NodeNumericSliderItem thresh = null, threshLinking = null;

                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("Начальный Порог"))
                                thresh = ns;
                            else if (ns.Text.Equals("Порог связывания границ"))
                                threshLinking = ns;
                        }
                    }

                    Image<Gray, byte> blur = image.Canny(new Gray(Convert.ToDouble(thresh.Value)), new Gray(Convert.ToDouble(threshLinking.Value)));// Convert<Gray, byte>();
                    Image<Bgr, byte> b = blur.Convert<Bgr, byte>();
                    n.Image = b.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }

    [Export(typeof(IPlugin))]
    public class NodeProcesserCircleDetector : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node circle;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Детектор окружностей");
            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Детектор окружностей"))
                {
                    circle = new Node("Детектор окружностей");

                    var CannyThreshold = new NodeLabelItem("Порог Кенни") { Tag = 4f };
                    circle.AddItem(CannyThreshold);
                    var CannyThresholdValue = new NodeTextBoxItem("180", false, false) { Tag = 3f };
                    CannyThresholdValue.Name = "Порог Кенни";
                    CannyThresholdValue.TextChanged += new EventHandler<AcceptNodeTextChangedEventArgs>(OnTextBoxChanget);
                    circle.AddItem(CannyThresholdValue);

                    var accumulatorThreshold = new NodeLabelItem("Накопитель порога") { Tag = 4f };
                    circle.AddItem(accumulatorThreshold);
                    var accumulatorThresholdValue = new NodeTextBoxItem("120", false, false) { Tag = 3f };
                    accumulatorThresholdValue.Name = "Накопитель порога";
                    circle.AddItem(accumulatorThresholdValue);

                    var dp = new NodeNumericSliderItem("Разрешение аккумулятора", 70, 30, 1, 10, 5, false, false) { Tag = 1f };
                    dp.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    circle.AddItem(dp);

                    var minDist = new NodeNumericSliderItem("Минимальное расстояние между центрами", 70, 30, 1, 10, 5, false, false) { Tag = 1f };
                    minDist.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    circle.AddItem(minDist);

                    var minRadius = new NodeNumericSliderItem("Минимальный радиус", 70, 30, 1, 10, 5, false, false) { Tag = 1f };
                    minRadius.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    circle.AddItem(minRadius);

                    var maxRadius = new NodeNumericSliderItem("Максимальный радиус", 70, 30, 0, 10, 5, false, false) { Tag = 1f };
                    maxRadius.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    circle.AddItem(maxRadius);

                    var imageItem3 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem3.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    circle.AddItem(imageItem3);
                    circle.AddProcess(ProcessCircleDetection);


                    selection.DoDragDrop(circle, DragDropEffects.Copy);
                    selection.SelectedItems.Clear();
                }
            }
        }

        private void OnTextBoxChanget(object sender, AcceptNodeTextChangedEventArgs e)
        {
            ((NodeTextBoxItem)sender).Node.DoAction();
        }

        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                    connection.To.Node.DoAction();
        }

        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessCircleDetection(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {


                    Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));

                    NodeTextBoxItem CannyThresholdValue = null, accumulatorThresholdValue = null;

                    NodeNumericSliderItem dp = null, minDist = null, minRadius = null, maxRadius = null;

                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("Разрешение аккумулятора"))
                                dp = ns;
                            else if (ns.Text.Equals("Минимальное расстояние между центрами"))
                                minDist = ns;
                            else if (ns.Text.Equals("Минимальный радиус"))
                                minRadius = ns;
                            else if (ns.Text.Equals("Максимальный радиус"))
                                maxRadius = ns;

                        }
                        else if ((float)ni.Tag == 3f)
                        {
                            NodeTextBoxItem ns = (NodeTextBoxItem)ni;
                            if (ns.Name.Equals("Накопитель порога"))
                                accumulatorThresholdValue = ns;
                            else if (ns.Name.Equals("Порог Кенни"))
                                CannyThresholdValue = ns;
                        }
                    }

                    Image<Gray, Byte> gray = image.Convert<Gray, Byte>().PyrDown().PyrUp();

                    Gray cannyThreshold = new Gray(Convert.ToInt32(CannyThresholdValue.Text));
                    //Gray cannyThresholdLinking = new Gray(120);
                    Gray circleAccumulatorThreshold = new Gray(Convert.ToInt32(accumulatorThresholdValue.Text));

                    CircleF[] circles = gray.HoughCircles(
                        cannyThreshold,
                        circleAccumulatorThreshold,
                        dp.Value, //Resolution of the accumulator used to detect centers of the circles
                        minDist.Value, //min distance 
                        Convert.ToInt32(minRadius.Value), //min radius
                        Convert.ToInt32(maxRadius.Value) //max radius
                        )[0]; //Get the circles from the first channel
                              /*CircleF[] circles = gray.HoughCircles(
                                          cannyThreshold,
                                          circleAccumulatorThreshold,
                                          5.0, //Resolution of the accumulator used to detect centers of the circles
                                          10.0, //min distance 
                                          5, //min radius
                                          0 //max radius
                                          )[0]; //Get the circles from the first channel*/
                    #region draw circles
                    Image<Bgr, Byte> circleImage = image.CopyBlank();
                    foreach (CircleF circle in circles)
                        circleImage.Draw(circle, new Bgr(Color.Brown), 2);
                    n.Image = circleImage.ToBitmap();
                    #endregion

                    //Image<Bgr, byte> blur = image.Dilate(Convert.ToInt32(thresh.Value));// Canny(new Gray(Convert.ToDouble(thresh.Value)), new Gray(Convert.ToDouble(threshLinking.Value)));// Convert<Gray, byte>();
                    //Image<Bgr, byte> b = blur.Convert<Bgr, byte>();
                    //n.Image = b.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }
    [Export(typeof(IPlugin))]
    public class NodeProcesserRotate : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node gauss;
        static int Number = 1;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Поворот изображения");
            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Поворот изображения"))
                {
                    gauss = new Node("Поворот изображения"+Number++);
                    var widthItem = new NodeNumericSliderItem("Угол", 70, 30, 0, 360, 0, false, false) { Tag = 1f };
                    widthItem.ValueChanged += new EventHandler<NodeItemEventArgs>(OnWidthChanget); ;
                    gauss.AddItem(widthItem);

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    gauss.AddItem(imageItem1);
                    gauss.AddProcess(ProcessMedianSmooth);
                    selection.DoDragDrop(gauss, DragDropEffects.Copy);
                    lvi.Selected = true;
                }
            }
        }

        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                {
                    if (n.Image == null)
                        connection.To.Node.Image = null;
                    connection.To.Node.DoAction();
                }
        }

        void OnWidthChanget(object sender, NodeItemEventArgs e)
        {
            e.Item.Node.DoAction();
        }
        
        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessMedianSmooth(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null)
                {
                    NodeNumericSliderItem w = null, h = null;
                    foreach (var ni in n.Items)
                    {
                        if ((float)ni.Tag == 1f)
                        {
                            NodeNumericSliderItem ns = (NodeNumericSliderItem)ni;
                            if (ns.Text.Equals("Угол"))
                                w = ns;
                        }
                    }
                    Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));
                    Image<Bgr, byte> blur = image.Rotate(w.Value, new Bgr(255, 0, 255));
                    n.Image = blur.ToBitmap();
                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }
    [Export(typeof(IPlugin))]
    public class NodeProces : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node src;
        static int Number = 1;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Сложение изображений");
            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Сложение изображений"))
                {
                    src = new Node("Сложение изображений" + Number++);
                    var img = new NodeCheckboxItem("Изображение2", true, false) { Tag = 42f };
                    src.AddItem(img);

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    src.AddItem(imageItem1);
                    src.AddProcess(ProcessMedianSmooth);
                    selection.DoDragDrop(src, DragDropEffects.Copy);
                    lvi.Selected = true;
                }
            }
        }

        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                {
                    if (n.Image == null)
                        connection.To.Node.Image = null;
                    connection.To.Node.DoAction();
                }
        }


        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessMedianSmooth(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection && n.Items.ElementAt(2).Input.HasConnection)
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                Image img2q = n.Items.ElementAt(2).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null && img2q != null)
                {
                    
                    Image<Bgr, Byte> img1 = new Image<Bgr, Byte>(new Bitmap(img));
                    Image<Bgr, Byte> img2 =  new Image<Bgr, byte>(new Bitmap(img2q));
                    Image<Bgr, Byte> img3,img4;
                    if (img1.Size != img2.Size)
                    {
                        int maxheight = 0, minheight = 0, maxwidth = 0, minwidth = 0;
                        //Find Maximum/Minimum width and minimum/height
                        if (img1.Width > img2.Width)
                        {
                            maxwidth = img1.Width;
                            minwidth = img2.Width;
                        }
                        else
                        {
                            maxwidth = img2.Width;
                            minwidth = img1.Width;
                        }

                        if (img1.Height > img2.Height)
                        {
                            maxheight = img1.Height;
                            minheight = img2.Height;
                        }
                        else
                        {
                            maxheight = img2.Height;
                            minheight = img1.Height;
                        }

                        //cvCopy function respect ROI so you could adjust the ROI of img3/img4 so that the images are in the centre if required
                        //but the ROIs must match that of the image being copied
                        img3 = new Image<Bgr, byte>(maxwidth, maxheight);  //create the image
                        img3.ROI = new Rectangle(0, 0, img1.Width, img1.Height);   //Set the ROI to match img to be copied
                        CvInvoke.cvCopy(img1, img3, IntPtr.Zero);      //Copy images over
                        //or
                        img1.CopyTo(img3);
                        //img3.ROI = new Rectangle(0, 0, maxwidth, maxheight);      //Reset the ROI
                        img3.ROI = new Rectangle(0, 0, minwidth, minheight);

                        img4 = new Image<Bgr, byte>(maxwidth, maxheight);
                        img4.ROI = new Rectangle(0, 0, img2.Width, img2.Height);
                        CvInvoke.cvCopy(img2, img4, IntPtr.Zero);
                        //img4.ROI = new Rectangle(0, 0, maxwidth, maxheight); 
                        img4.ROI = new Rectangle(0, 0, minwidth, minheight);

                        n.Image = img3.Add(img4).ToBitmap();
                    }
                    else n.Image = img1.Add(img2).ToBitmap();

                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }

    [Export(typeof(IPlugin))]
    public class NodeProcessColored : IPlugin
    {
        #region IStringSender Members
        ListView selection;
        Node src;
        public void Load(ListView listview)
        {
            selection = listview;
            selection.MouseDown += new System.Windows.Forms.MouseEventHandler(listView1_MouseDown);
            ListViewItem it = new ListViewItem("Раскраска");
            selection.Items.Add(it);
        }
        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = selection.GetItemAt(e.X, e.Y);
            if (lvi != null)
            {

                if (lvi.Text.Equals("Раскраска"))
                {
                    src = new Node("Раскраска");

                    var imageItem1 = new NodeImageItem(null, 90, 90, false, false) { Tag = 1000f };
                    imageItem1.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
                    src.AddItem(imageItem1);
                    src.AddProcess(ProcessMedianSmooth);
                    selection.DoDragDrop(src, DragDropEffects.Copy);
                    lvi.Selected = true;
                }
            }
        }

        private void NodeChanged(object sender, NodeEventArgs e)
        {
            Node n = (Node)sender;
            foreach (var connection in n.Connections)
                if (!connection.To.Node.Equals(n))
                {
                    if (n.Image == null)
                        connection.To.Node.Image = null;
                    connection.To.Node.DoAction();
                }
        }


        void OnImgClicked(object sender, NodeItemEventArgs e)
        {
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }
        void ProcessMedianSmooth(Node n)
        {
            if (n.Items.ElementAt(1).Input.HasConnection )
            {
                Image img = n.Items.ElementAt(1).Input.Connectors.ElementAt(0).From.Node.Image;
                if (img != null )
                {

                    Image<Bgr, Byte> frame = new Image<Bgr, Byte>(new Bitmap(img));

                    /*Image<Gray, Byte> frame = img1.InRange(new Gray(Color.Orange.GetHue() - 10),
                                     new Gray(Color.Orange.GetHue() + 10));*/
                    Image<Gray, Byte> grayscale = frame.Convert<Gray, Byte>();
                    grayscale = grayscale.Canny(new Gray(0), new Gray(255)).Not(); //invert with Not()
                    frame = frame.And(grayscale.Convert<Bgr, Byte>(), grayscale); //And function in action
                    //imageBox1.Image = frame;


                    n.Image = frame.ToBitmap();

                }
                else
                    n.Image = null;
            }
            else
                n.Image = null;
        }
        #endregion
    }
}
