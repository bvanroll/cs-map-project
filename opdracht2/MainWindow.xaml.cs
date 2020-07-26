using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Globals;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

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
            //todo logica toevoegen als object
            //todo datalaag toevoegen als object en passen naar logica

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
                f = GeoJsonParser.TriangulateJsonData(File.ReadAllText(openFileDialog.FileName), 800, 800);
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
                Debug.WriteLine("--- ADDING NEW POLYGON FROM LIST (COUNT: " + f.Count + ")");
                Debug.WriteLine(f[0].ToString());
                c.Children.Add(f[0]);
                Debug.WriteLine(("ADDED POLYGON"));
                f.RemoveAt(0);
                Debug.WriteLine(("REMOVED POLYGON"));
                //Thread.Sleep(80);
            }
        }

        private Polygon makePolygon(PolygonPunten polygonPunten)
        {
            PointCollection punten = new PointCollection();
            foreach (Punt p in polygonPunten.Punten)
            {
                punten.Add(p.ToPoint());
            }
            Polygon polygon = new Polygon();
            polygon.Points = punten;
            polygon.StrokeThickness = 1;


            // deze code zorgt ervoor dat de driehoeken duidelijker zijn op de afbeelding door de kleur willekeurig te selecteren
            Random random = new Random();
            Type brushType = typeof(Brushes);
            PropertyInfo[] properties = brushType.GetProperties();
            int randomIndex = random.Next(properties.Length);
            Brush willekeurigeBrush = (Brush) properties[randomIndex].GetValue(null, null);
            polygon.Fill = willekeurigeBrush;
            polygon.Stroke = willekeurigeBrush;
            return polygon;
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

