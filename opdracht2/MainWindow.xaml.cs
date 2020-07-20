using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace opdracht2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Canvas c;
        List<Polygon> f;
        List<Ellipse> el;
        List<Polygon> buffer;
        public MainWindow()
        {
            f = new List<Polygon>();
            el = new List<Ellipse>();
            buffer = new List<Polygon>();
            InitializeComponent();
            c = (Canvas)this.FindName("someCanvas");
            CompositionTarget.Rendering += DoUpdates;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                //TODO remove
                //Tuple<List<Polygon>, List<Ellipse>> t = GeoJsonParser.TriangulateJsonData(File.ReadAllText(openFileDialog.FileName), c.Width, c.Height);
                f = GeoJsonParser.TriangulateJsonData(File.ReadAllText(openFileDialog.FileName), c.Width, c.Height);
                el = new List<Ellipse>();
            }
            var st = new ScaleTransform();
            c.RenderTransform = st;
            foreach (Ellipse eli in el)
            {
                c.Children.Add(eli);
            }
            c.MouseWheel += (sender, e) =>
            {
                if (f.Count > 0)
                {
                    buffer.Add(f[0]);
                    f.RemoveAt(0);
                }
                if (e.Delta > 0)
                {
                    st.ScaleX *= 2;
                    st.ScaleY *= 2;
                }
                else
                {
                    st.ScaleX /= 2;
                    st.ScaleY /= 2;
                }
            };
            
            Debug.Write("done");
        }

        private void DoUpdates(object sender, EventArgs e)
        {
            if (f.Count > 0)
            {
                c.Children.Add(f[0]);
                f.RemoveAt(0);
                Thread.Sleep(50);
            }
        }


        //zoom https://stackoverflow.com/a/44593026
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (f.Count > 0)
            {
                buffer.Add(f[0]);
                f.RemoveAt(0);
            }
            
        }
    }
}

