using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Polygon = System.Windows.Shapes.Polygon;

namespace opdracht2
{
    public static class GeoJsonParser
    {
        
        public static double maximumXWaarde,
            maximumYWaarde,
            minimumXWaarde,
            minimumYWaarde,
            schaalXWaarde,
            schaalYWaarde;
        public static double epsilon;

        public static List<System.Windows.Shapes.Polygon> TriangulateJsonData(string json,
            double x, double y)
        {
            epsilon = .003;
            schaalXWaarde = x;
            schaalYWaarde = y;
            maximumXWaarde = double.MinValue;
            minimumXWaarde = double.MaxValue;
            maximumYWaarde = double.MinValue;
            minimumYWaarde = double.MaxValue;
            List<System.Windows.Shapes.Polygon> returnWaarde = new List<System.Windows.Shapes.Polygon>();
            foreach(JObject v in JObject.Parse(json)["features"])
            {
                try
                {
                    List<Point> EnkelePolygonLijst = MaakPolygonLijn(MaakPolygonLijst(JsonConvert.DeserializeObject<MultiPolygon>(v["geometry"].ToString())));
                    returnWaarde.AddRange(maakDriehoeken(EnkelePolygonLijst));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("couldn't parse " + v["properties"]["name"]);
                }
            }
            return NormalizePolygon(returnWaarde);
        }

        private static List<Ellipse> maakPunten(List<Point> enkelePolygonLijst)
        {
            List<Ellipse> returnWaarde = new List<Ellipse>();
            foreach (Point c in enkelePolygonLijst)
            {
                Ellipse dot = new Ellipse();
                dot.Stroke = Brushes.Black;
                dot.StrokeThickness = 1;
                dot.Height = 5;
                dot.Width = 5;
                dot.Fill = new SolidColorBrush(Colors.Black);
                dot.Margin = new Thickness(c.X, c.Y, 0, 0);
                returnWaarde.Add(dot);
            }

            return returnWaarde;
        }

        private static List<System.Windows.Shapes.Polygon> maakDriehoeken(List<Point> polygonLijst)
        {
            List<System.Windows.Shapes.Polygon> returnWaarde = new List<System.Windows.Shapes.Polygon>();
            int i = 0;
            while (true)
            {

                if (i >= polygonLijst.Count)
                {
                    i = 0;
                }

                int punt1Index = i;
                int punt2Index = i + 1;
                if (punt2Index >= polygonLijst.Count) punt2Index -= polygonLijst.Count;
                int punt3Index = i + 2;
                if (punt3Index >= polygonLijst.Count) punt3Index -= polygonLijst.Count;

                if (polygonLijst.Count == 3)
                {
                    returnWaarde.Add(CreateNewPolygon(polygonLijst[punt1Index], polygonLijst[punt2Index],
                           polygonLijst[punt3Index]));
                    break;
                }
                // TODO HIER MOET DE FOUT ZITTEN, KAN NIE ANDERS!!!!
                double hoek = GetAngle(polygonLijst[punt1Index], polygonLijst[punt2Index], polygonLijst[punt3Index]);
                if (hoek < 180)
                {
                    returnWaarde.Add(CreateNewPolygon(polygonLijst[punt1Index], polygonLijst[punt2Index],
                           polygonLijst[punt3Index]));
                    polygonLijst.RemoveAt(punt2Index);

                }
                
                


                i++;
            }

            return returnWaarde;
        }

        //https://stackoverflow.com/a/31334882
        private static double GetAngle(Point p1, Point p2, Point p3)
        {
            return Math.Atan2(p3.Y - p1.Y, p3.X - p1.X) - Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }



        private static System.Windows.Shapes.Polygon CreateNewPolygon(Point punt1, Point punt2, Point punt3)
        {
            System.Windows.Shapes.Polygon returnWaarde = new System.Windows.Shapes.Polygon();
            PointCollection puntCollectie = new PointCollection();
            puntCollectie.Add(punt1);
            puntCollectie.Add(punt2);
            puntCollectie.Add(punt3);
            returnWaarde.Points = puntCollectie;
            returnWaarde.StrokeThickness = 1;


            // deze code zorgt ervoor dat de driehoeken duidelijker zijn op de afbeelding door de kleur willekeurig te selecteren
            Random random = new Random();
            Type brushType = typeof(Brushes);
            PropertyInfo[] properties = brushType.GetProperties();
            int randomIndex = random.Next(properties.Length);
            Brush willekeurigeBrush = (Brush) properties[randomIndex].GetValue(null, null);
            returnWaarde.Fill = willekeurigeBrush;
            returnWaarde.Stroke = willekeurigeBrush;

            return returnWaarde;
        }

        private static List<List<Point>> MaakPolygonLijst(MultiPolygon multiPolygon)
        {
            List<List<Point>> polygonAlsPuntenLijst = new List<List<Point>>();
            foreach (GeoJSON.Net.Geometry.Polygon geojsonPolygon in multiPolygon.Coordinates)
            {
                List<Point> puntenLijst = new List<Point>();
                foreach (LineString lineString in geojsonPolygon.Coordinates)
                {
                    foreach (Position positie in lineString.Coordinates)
                    {
                        if (positie.Longitude < minimumXWaarde)
                        {
                            minimumXWaarde = positie.Longitude;
                        }

                        if (positie.Longitude > maximumXWaarde)
                        {
                            maximumXWaarde = positie.Longitude;
                        }

                        if (positie.Latitude < minimumYWaarde)
                        {
                            minimumYWaarde = positie.Latitude;
                        }

                        if (positie.Latitude > maximumYWaarde)
                        {
                            maximumYWaarde = positie.Latitude;
                        }
                        Point punt = new Point(positie.Longitude, positie.Latitude);
                        if (!puntenLijst.Contains(punt)) puntenLijst.Add(new Point( positie.Longitude, positie.Latitude));
                    }
                }

                puntenLijst = douglasPeuker(puntenLijst);
                polygonAlsPuntenLijst.Add(puntenLijst);
                /* TODO waarom is dit hier???
                if (polygonAlsPuntenLijst.Count > 1)
                {
                    break;
                }*/
            }

            return polygonAlsPuntenLijst;
        }

        private static List<Point> MaakPolygonLijn(List<List<Point>> multiPolygon)
        {
            //zoek dichtsbijzijnde punt wanneer toevoegen volgende vector en voeg pas vanaf dat punt toe.
            bool richting = true; //richting van inlezen polygon (met klok mee, tegen in klok)
            List<Point> returnWaarde = new List<Point>();
            foreach (List<Point> polygon in multiPolygon)
            {
                if (richting)
                {
                    polygon.Reverse();
                }

                List<Point> normalizedPoly = polygon; //geen nederlandse naam gevonden voor dit
                if (returnWaarde.Count > 0)
                {
                    Point dichtsbijzijndePunt = returnWaarde[0];
                    Point dichtsbijzijndeNieuwePunt = normalizedPoly[0];
                    
                    foreach (Point punt in returnWaarde)
                    {
                        //ik dacht dat dit beter ging zijn, maar blijkbaar geen echt effect, behalve shit vertragen.
                        foreach (Point punt2 in normalizedPoly)
                        {
                            if (Vector2.Distance(new Vector2((float)punt.X, (float)punt.Y), new Vector2((float)punt2.X, (float)punt2.Y)) <
                                Vector2.Distance(new Vector2((float) dichtsbijzijndePunt.X,(float) dichtsbijzijndePunt.Y), 
                                new Vector2((float)dichtsbijzijndeNieuwePunt.X, (float)dichtsbijzijndeNieuwePunt.Y)))
                            {
                                dichtsbijzijndePunt = punt;
                                dichtsbijzijndeNieuwePunt = punt2;
                            }
                        }
                    }

                    normalizedPoly = orderList(normalizedPoly, dichtsbijzijndeNieuwePunt);
                    returnWaarde.InsertRange(returnWaarde.IndexOf(dichtsbijzijndePunt), normalizedPoly);
                }
                else
                {
                    returnWaarde.AddRange(normalizedPoly);
                }

                richting = !richting;
            }

            return returnWaarde;
        }
        
        private static List<Point> orderList(List<Point> normalizedPoly, Point dichtsbijzijndeNieuwePunt)
        {
            int i = normalizedPoly.IndexOf(dichtsbijzijndeNieuwePunt);
            int check = i;
            List<Point> returnWaarde = new List<Point>();
            do
            {
                if (i >= normalizedPoly.Count) i -= normalizedPoly.Count;
                returnWaarde.Add(normalizedPoly[i]);
                
                i++;
            } while (normalizedPoly.Count != returnWaarde.Count);

            return returnWaarde;
        }

        //TODO refactor dit zodat het alle polygons normalized aan het einde van de formule.
        private static List<System.Windows.Shapes.Polygon> NormalizePolygon(List<System.Windows.Shapes.Polygon> polygons)
        {
            maximumXWaarde -= minimumXWaarde;
            maximumYWaarde -= minimumYWaarde;
            //mss deze fn hermaken zodat deze ook ramer douglas peucker
            List<System.Windows.Shapes.Polygon> returnWaarde = new List<System.Windows.Shapes.Polygon>();
            foreach (System.Windows.Shapes.Polygon p in polygons)
            {
                PointCollection puntCollectie = new PointCollection();
                foreach (Point punt in p.Points)
                {
                    double x = (punt.X - minimumXWaarde);
                    x = (x / maximumXWaarde);
                    x = (x * schaalXWaarde);
                    double y = (punt.Y - minimumYWaarde);
                    y = (y / maximumYWaarde);
                    y =  (y * schaalYWaarde);
                    puntCollectie.Add(new Point(x, y));
                }
                returnWaarde.Add(CreateNewPolygon(puntCollectie[0], puntCollectie[1], puntCollectie[2]));
            }
            //List<Point> iets = DP(returnWaarde);
            return returnWaarde;
        }
        
        private static List<Point> douglasPeuker(List<Point> punten)
        {
            double dmax = -1;
            int index = 0;
            int end = punten.Count;

            for (int i = 1; i < end-1; i++)
            {
                double distance = PerpendicularDistance2(punten[i], punten[0], punten[end-1]);
                if (distance > dmax)
                {
                    index = i;
                    dmax = distance;
                }
            }


            List<Point> returnWaarde = new List<Point>();

            if (dmax > epsilon)
            {
                List<Point> recResults1 = douglasPeuker(punten.GetRange(0, index));
                List<Point> recResults2 = douglasPeuker(punten.GetRange(index, end-1-index));
                

                returnWaarde.AddRange(recResults1);
                returnWaarde.AddRange(recResults2);
            }
            else
            {
                returnWaarde = new List<Point>() { punten[0], punten[punten.Count-1] };
            }

            return returnWaarde;
        }
         
        private static double PerpendicularDistance2(Point point, Point l1, Point l2)
        {
            return Math.Abs((l2.X - l1.X) * (l1.Y - point.Y) - (l1.X - point.X) * (l2.Y - l1.Y)) /
                    Math.Sqrt(Math.Pow(l2.X - l1.X, 2) + Math.Pow(l2.Y - l1.Y, 2));

        }

    }
}