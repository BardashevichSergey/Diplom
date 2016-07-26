using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Graph;
using System.Drawing.Drawing2D;
using Graph.Compatibility;
using Graph.Items;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace GraphNodes
{
   
    public partial class ExampleForm : Form
	{
        public List<Node> ShowInDebug = new List<Node>();
		public ExampleForm()
		{
			InitializeComponent();
            var ms = new PluginLoader(string.Empty);
            ms.LoadPlugins(listView1);

            graphControl.CompatibilityStrategy = new TagTypeCompatibility();

            
            /*var someNode2 = new Node("Цвет");
            someNode2.Location = new Point(650, 150);
            someNode2.AddItem(new NodeCheckboxItem("Включить", true, true) { Tag = 42f });

            var imageItem2 = new NodeImageItem(null, 64, 64, true, true) { Tag = 1000f };
            imageItem2.Clicked += new EventHandler<NodeItemEventArgs>(OnImgClicked);
            someNode2.AddItem(imageItem2);
            someNode2.AddProcess(Process2);
            graphControl.AddNode(someNode2);*/

            
            graphControl.ConnectionAdded	+= new EventHandler<AcceptNodeConnectionEventArgs>(OnConnectionAdded);
			graphControl.ConnectionRemoved += new EventHandler<NodeConnectionEventArgs>(OnConnectionRemoved);
			graphControl.ShowElementMenu	+= new EventHandler<AcceptElementLocationEventArgs>(OnShowElementMenu);
            graphControl.NodeAdded += new EventHandler<AcceptNodeEventArgs>(OnNodeAdded); 
            graphControl.NodeRemoving += new EventHandler<AcceptNodeEventArgs>(OnNodeRemoving);
            
        }

        private void OnNodeRemoving(object sender, AcceptNodeEventArgs e)
        {
            e.Node.debug.Checked = false;
            AddInDebug(e.Node, null);
        }

        private void AddInDebug(object sender, ElementEventArgs e)
        {
            Node n = (Node)sender;
            if (n.debug.Checked)
            {
                ShowInDebug.Add(n);
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
                            labelvalue.Text = nsi.Value.ToString();
                            labelname.Text = nsi.Text;
                            labelList.Add(labelname);
                            labelList.Add(labelvalue);
                            break;
                        default: break;
                    }
                }
                DebugltemControl dicon = new  DebugltemControl(n.Title, n.Image,labelList);
                n.debugitem = dicon;
                Control c = splitContainer1.Panel2.GetChildAtPoint(new Point(1, 1));
                if(c!=null)
                    c.Controls.Add(dicon);
            }
            else
            {
                int index = ShowInDebug.IndexOf(n);
                if (index != -1)
                {
                    ShowInDebug.RemoveAt(index);
                    Control c = splitContainer1.Panel2.GetChildAtPoint(new Point(1, 1));
                    if (c != null)
                        c.Controls.RemoveAt(index);

                }
            }

        }
        private void OnNodeAdded(object sender, AcceptNodeEventArgs e)
        {
            e.Node.AddNodeInDebug += new EventHandler<ElementEventArgs>(AddInDebug);
            e.Node.UpdateNodeInDebug += new EventHandler<ElementEventArgs>(UpdateNodeInDebug);
        }

        private void UpdateNodeInDebug(object sender, ElementEventArgs e)
        {
            Node n = (Node)sender;
            if (n.debugitem != null)
                n.debugitem.ChangeData(n);
        }

        void Process2(Node n)
        {
            NodeConnection ncon = n.Connections.First();
            Image img = ncon.From.Node.Image;

            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(img));
            Image<Gray, byte> blur = image.Convert<Gray, byte>();

            n.Image = blur.ToBitmap();
        }
        
        void OnImgClicked(object sender, NodeItemEventArgs e)
		{
            ImageForm f = new ImageForm();
            Image<Bgr, Byte> image = new Image<Bgr, Byte>(new Bitmap(e.Item.Node.Image));
            f.SetImage(image);
            f.Show();
        }

		void OnConnectionRemoved(object sender, NodeConnectionEventArgs e)
		{
            e.To.Node.DoAction();
        }

		void OnShowElementMenu(object sender, AcceptElementLocationEventArgs e)
		{
			if (e.Element == null)
			{
				// Show a test menu for when you click on nothing
				testMenuItem.Text = "(clicked on nothing)";
				nodeMenu.Show(e.Position);
                
				e.Cancel = false;
			} else
			if (e.Element is Node)
			{
				// Show a test menu for a node
				testMenuItem.Text = ((Node)e.Element).Title;
				nodeMenu.Show(e.Position);
                nodeMenu.Tag = e.Element;
                e.Cancel = false;
			} else
			if (e.Element is NodeItem)
			{
				// Show a test menu for a nodeItem
				testMenuItem.Text = e.Element.GetType().Name;
				nodeMenu.Show(e.Position);
				e.Cancel = false;
			} else
			{
				// if you don't want to show a menu for this item (but perhaps show a menu for something more higher up) 
				// then you can cancel the event
				e.Cancel = true;
			}
		}

		static int counter = 1;
		void OnConnectionAdded(object sender, AcceptNodeConnectionEventArgs e)
		{
			e.Connection.Name = "Connection " + counter ++;
            e.Connection.To.Node.DoAction();
		}
        		            
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IElement el = (IElement)nodeMenu.Tag;
            if(el is Node)
            {
                Node n = (Node)el;
                graphControl.RemoveNode(n);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            this.Close();
        }

        private void ExampleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) //если нажали на кнопку "Закрыть"
            {
                if (DialogResult.No == MessageBox.Show("Вы действительно хотите закрыть приложение ?", "Завершение программы", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
                    e.Cancel = true;
            }
        }

        
    }
    
}
