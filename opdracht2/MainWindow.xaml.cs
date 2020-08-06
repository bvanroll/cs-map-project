using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        ListBox l;
        CheckBox triangulate;
        CheckBox scale;
        CheckBox peuker;
        Slider peukerPercent;
        PolygonManipulatie pm;
        JsonReader j;
        public MainWindow()
        {
            InitializeComponent();
            c = (Canvas)this.FindName("someCanvas"); //vind canvas en link het aan object. zo kan later in functies deze aangeroepen worden
            peukerPercent = (Slider)this.FindName("PeukerSlider"); // vind slider voor peuker percent waarde (wordt later gebruikt in functies)
            l = (ListBox)this.FindName("lb"); //vind listbox waar landen in gaan komen

            //vind checkboxen die verantwoordelijk zijn voor triangulation, peuker en scaling
            triangulate = (CheckBox)this.FindName("Triangulate");
            peuker = (CheckBox)this.FindName("Peuker");
            scale = (CheckBox)this.FindName("Scale");


            Debug.Write("done");
        }


        //zoom algoritme da ooit gebruikt geweest is (zit mss niet in deze versie, te lang geleden om te herinneren) https://stackoverflow.com/a/44593026

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            //openfiledialog om json file in te lezen
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                j = new JsonReader(openFileDialog.FileName); //dit is een datalaag object dat verantwoordelijk is voor geojson om te zetten naar de zelfgemaakte klassen
                pm = new PolygonManipulatie(j); //dit is de logica laag instantie
            }
            foreach (object o in pm.GetPolygons())
            {
                //voeg alle landen toe aan de list
                try { l.Items.Add(o); } catch (Exception) { Debug.WriteLine("error adding item"); };

            }
            foreach (object o in pm.GetMultiPolygons())
            {
                //voeg alle landen toe aan de list deel 2
                try { l.Items.Add(o); } catch (Exception) { Debug.WriteLine("error adding item"); };

            }



            //voor zoom algo
            var st = new ScaleTransform();
            c.RenderTransform = st;
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
            //




            //als een land geselecteerd wordt in lijst runned deze functie



            //voorbereiding van scale waarden.
            
            double scaleX = (c.ActualHeight > c.ActualWidth) ? c.ActualWidth : c.ActualHeight;
            double scaleY = (c.ActualHeight > c.ActualWidth) ? c.ActualWidth : c.ActualHeight;
            double offsetX = 0; //vroeger c.ActualWidth/2
            double offsetY = 0;
            //dit zorgt voor evenredige scaling zonder stretching. als men wel stretching wilt gebruiken:
            // scaleX = c.ActualWidth;
            // scaleY = c.ActualHeight;

            //is er meer dan 1 land geselecteerd 
            if (lb.SelectedItems.Count >1)
            {
                c.Children.Clear(); // haal oude afbeeldingen uit canvas
                List<MultiPolygonPunten> mpps = new List<MultiPolygonPunten>(); //verander polygons in multipolygons met 1 polygon er in (zo kunnen we zelfde bewerking op alle polygons toepassen)
                foreach (Object o in lb.SelectedItems) //loop doorheen geselecteerde items
                {
                    if (o.GetType().Name == "PolygonPunten" ) {
                        PolygonPunten p = (PolygonPunten)o; //als het een polygonpunt object is, zet om naar multipolygon
                        mpps.Add(p.ToMultiPolygonPunten());
                    } else
                    {
                        mpps.Add((MultiPolygonPunten)o); //multipolygons kunenn gewoon in de lijst toegevoegd worden
                    }
                }

                //scale de polygon als de checkbox aan staat
                if (scale.IsChecked == true) mpps = pm.ScaleMultiPolygons(mpps, scaleX, scaleY, offsetX, offsetY);
                
                foreach(MultiPolygonPunten mp in mpps)
                {
                    MultiPolygonPunten mps = null;
                    //peuker (puntvermindering) wordt hier bekeken en uitgevoerd als nodig. percentage gaat van 0 - 100, maar moet gaan van 0 - .1 daarom /1000;
                    if (peuker.IsChecked == true) mps = pm.Peuker(mp, peukerPercent.Value / 1000);
                    else mps = mp; //geen peuker dus gewoon zelfde mp gebruiken (mp = multipolygon)
                    if (triangulate.IsChecked == true) //check of triangulation nodig is. (kan niet gewoon triangulate.IsChecked want dit geeft ook null soms
                    {
                        foreach(PolygonPunten pp in mps.PolygonPunten) //loop doorheen polygons in multipolygon
                        {
                            foreach(PolygonPunten ppd in pm.TriangulatePolygon(pp)) // loop doorheen driehoeken in polygon
                            {
                                c.Children.Add(getPolygon(ppd)); // voeg driehoeken toe aan canvas
                            }
                        }
                    }
                    else //geen triangulation dus voeg gewoon polygons toe aan canvas
                    {
                        foreach (Polygon p in getPolygon(mp))
                        {
                            c.Children.Add(p);
                        }
                    }
                }
            } 
            else if (lb.SelectedItems.Count == 1) //als er maar 1 land geselecteerd is
            {
                switch (lb.SelectedItem.GetType().Name) //check of multipolygon of polygon
                {
                    case "MultiPolygonPunten": // als multipolygon
                        Debug.WriteLine(lb.SelectedItem.GetType().Name); // voor debug redenen schrijf naam naar console
                        c.Children.Clear(); // delete alle vorige afbeeldingen
                        MultiPolygonPunten mp = (MultiPolygonPunten)lb.SelectedItem; //haal multipolygon uit lijst
                        if (scale.IsChecked == true) mp = pm.ScaleMultiPolygon(mp, scaleX, scaleY, offsetX, offsetY); //schaal multipolygon
                        if (peuker.IsChecked == true) mp = pm.Peuker(mp, peukerPercent.Value/1000); // als peuker (puntvermindering) moet gebeuren, doe dit hier
                        foreach (PolygonPunten pp in mp.PolygonPunten) //loop doorheen polygons in multipolygon
                        {
                            if(triangulate.IsChecked == true) //als deze getriangulate moeten worden
                            {
                                foreach (PolygonPunten ppd in pm.TriangulatePolygon(pp)) // loop doorheen driehoeken in triangulated polygon
                                {
                                    c.Children.Add(getPolygon(ppd)); //voeg driehoek toe aan canvas

                                }
                            } else //niet getriangulate
                            {
                                c.Children.Add(getPolygon(pp)); // voeg polygon toe aan canvas
                            }
                            
                        }
                        break;
                    case "PolygonPunten": //in geval polygon
                        Debug.WriteLine(lb.SelectedItem.GetType().Name);
                        c.Children.Clear(); //delete alle vorige afbeeldingen
                        PolygonPunten p = (PolygonPunten)lb.SelectedItem; // haal land uit lijst
                       
                        if (scale.IsChecked == true) p = pm.ScalePolygon(p, scaleX, scaleY, offsetX, offsetY); // schaal polygon
                        if (peuker.IsChecked == true) p = pm.Peuker(p, peukerPercent.Value / 1000); // peuker (puntvermindering)
                        if (triangulate.IsChecked == true) //triangulation check
                        {
                            foreach(PolygonPunten pp in pm.TriangulatePolygon(p)) // loop doorheen driehoeken
                            {
                                c.Children.Add(getPolygon(pp)); //voeg driehoeken toe
                            }

                        } else
                        {
                            c.Children.Add(getPolygon(p)); //voeg polygon toe


                        }
                        break;

                }
            } else
            {
                c.Children.Clear(); // verwijder alles (als niks geselecteerd is)
            }
            Debug.WriteLine("finished");

        }

        private List<Polygon> getPolygon(MultiPolygonPunten mp)
        {
            //zet multipolygon om naar polygon object (kan deze niet buiten deze klasse gebruiken
            List<Polygon> lijst = new List<Polygon>();
            foreach(PolygonPunten p in mp.PolygonPunten)
            {
                lijst.Add(getPolygon(p));
            }
            return lijst;
        }
        private Polygon getPolygon(PolygonPunten p)
        {
            //zet polygon om naar polygon object (kan deze niet buiten deze klasse gebruiken
            Polygon returnWaarde = new Polygon();
            PointCollection puntCollectie = new PointCollection();
            foreach(Punt punt in p.Punten)
            {
                puntCollectie.Add(new Point(punt.X, punt.Y));
            }
            returnWaarde.Points = puntCollectie;
            returnWaarde.StrokeThickness = 1;


            // deze code bepaald kleur van de polygon
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

