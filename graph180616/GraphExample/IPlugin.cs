using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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

namespace GraphNodes
{
    public interface IPlugin
    {
        void Load(ListView listview);
    }
    public class PluginLoader
    {
        [ImportMany(typeof(IPlugin))]
        private IEnumerable<IPlugin> _plugins;

        public PluginLoader(string extentionsDirectory)
        {
            var catalog = new DirectoryCatalog(String.IsNullOrEmpty(extentionsDirectory) ? "." : extentionsDirectory);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);

        }
        public void LoadPlugins(ListView listview)
        {
            foreach (var sender in _plugins)
                sender.Load(listview);
        }
    }
}
