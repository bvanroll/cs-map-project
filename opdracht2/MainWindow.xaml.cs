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
using Datalaag;
using Logica;

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
        ListBox l;
        CheckBox triangulate;
        PolygonManipulatie pm;
        JsonReader j;
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
            l = (ListBox)this.FindName("lb");
            triangulate = (CheckBox)this.FindName("triangulate");
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

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                //TODO remove
                //Tuple<List<Polygon>, List<Ellipse>> t = GeoJsonParser.TriangulateJsonData(File.ReadAllText(openFileDialog.FileName), c.Width, c.Height);
                //f = GeoJsonParser.TriangulateJsonData(File.ReadAllText(openFileDialog.FileName), 200, 200);
                j = new JsonReader(openFileDialog.FileName);
                el = new List<Ellipse>();
                pm = new PolygonManipulatie(j);
            }
            foreach (object o in pm.GetPolygons())
            {
                try { l.Items.Add(o); } catch (Exception) { };

            }
            foreach (object o in pm.GetMultiPolygons())
            {
                try { l.Items.Add(o); } catch (Exception) { };

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
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO zorg ervoor dat als meerdere items geselecteerd worden, we een scaling methode hebben voor meerdere (wss gebruik van list)
            //zodat we een 1 lijst kunnen gebruiken voor alle polygons
            if (lb.SelectedItems.Count >1)
            {
                c.Children.Clear();
                List<MultiPolygonPunten> mpps = new List<MultiPolygonPunten>();
                foreach (Object o in lb.SelectedItems)
                {
                    if (o.GetType().ToString() == "PolygonPunten") {
                        PolygonPunten p = (PolygonPunten)o;
                        mpps.Add(p.ToMultiPolygonPunten());
                    } else
                    {
                        mpps.Add((MultiPolygonPunten)o);
                    }
                }
                //peuker implementatie moet ook nog gebeuren, code is er al, maar wanneer moet deze aangeroepen worden
                mpps = pm.ScaleMultiPolygons(mpps, 100, 100);
                foreach(MultiPolygonPunten mp in mpps)
                {
                    if (triangulate.IsChecked == true) //kan blkbr ook null zijn, raar
                    {
                        foreach(PolygonPunten pp in mp.PolygonPunten)
                        {
                            foreach(PolygonPunten ppd in pm.TriangulatePolygon(pp))
                            {
                                c.Children.Add(getPolygon(ppd));
                            }
                        }
                    }
                    else
                    {
                        foreach (Polygon p in getPolygon(mp))
                        {
                            c.Children.Add(p);
                        }
                    }
                }
            } 
            else
            {
                switch (lb.SelectedItem.GetType().Name)
                {
                    case "MultiPolygonPunten":
                        Debug.WriteLine(lb.SelectedItem.GetType().Name);
                        c.Children.Clear();
                        MultiPolygonPunten mp = pm.ScaleMultiPolygon((MultiPolygonPunten)lb.SelectedItem, 100, 100);
                        //hier ervoor zorgen dat scaling, triangulation etc gebeurt door gebruik van logica layer functies te callen
                        foreach (PolygonPunten pp in mp.PolygonPunten)
                        {
                            foreach (PolygonPunten ppd in pm.TriangulatePolygon(pp))
                            {
                                c.Children.Add(getPolygon(ppd));

                            }
                        }
                        foreach (Polygon po in getPolygon(mp))
                        {
                            //c.Children.Add(po);
                        }
                        break;
                    case "PolygonPunten":
                        Debug.WriteLine(lb.SelectedItem.GetType().Name);
                        c.Children.Clear();
                        PolygonPunten p = pm.ScalePolygon((PolygonPunten)lb.SelectedItem, 100, 100);
                        c.Children.Add(getPolygon(p));
                        break;

                }
            }
            

        }

        private List<Polygon> getPolygon(MultiPolygonPunten mp)
        {
            List<Polygon> lijst = new List<Polygon>();
            foreach(PolygonPunten p in mp.PolygonPunten)
            {
                lijst.Add(getPolygon(p));
            }
            return lijst;
        }
        private Polygon getPolygon(PolygonPunten p)
        {
            Polygon returnWaarde = new Polygon();
            PointCollection puntCollectie = new PointCollection();
            foreach(Punt punt in p.Punten)
            {
                puntCollectie.Add(new Point(punt.X, punt.Y));
            }
            returnWaarde.Points = puntCollectie;
            returnWaarde.StrokeThickness = 1;


            // deze code zorgt ervoor dat de driehoeken duidelijker zijn op de afbeelding door de kleur willekeurig te selecteren
            Random random = new Random();
            Type brushType = typeof(Brushes);
            PropertyInfo[] properties = brushType.GetProperties();
            int randomIndex = random.Next(properties.Length);
            Brush willekeurigeBrush = (Brush)properties[randomIndex].GetValue(null, null);
            if (willekeurigeBrush == Brushes.White) willekeurigeBrush = Brushes.Black;
            returnWaarde.Fill = willekeurigeBrush;
            returnWaarde.Stroke = willekeurigeBrush;
            return returnWaarde;
        }
    }
}

