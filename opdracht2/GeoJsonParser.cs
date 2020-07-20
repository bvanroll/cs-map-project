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
                    List<Point> EnkelePolygonLijst = maakPolygonLijn(maakPolygonLijst(JsonConvert.DeserializeObject<MultiPolygon>(v["geometry"].ToString())));
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
                    returnWaarde.Add(createNewPolygon(polygonLijst[punt1Index], polygonLijst[punt2Index],
                           polygonLijst[punt3Index]));
                    break;
                }
                double hoek = getAngle(polygonLijst[punt1Index], polygonLijst[punt2Index], polygonLijst[punt3Index]);
                if (hoek < 180)
                {
                    returnWaarde.Add(createNewPolygon(polygonLijst[punt1Index], polygonLijst[punt2Index],
                           polygonLijst[punt3Index]));
                    polygonLijst.RemoveAt(punt2Index);

                }
                
                


                i++;
            }

            return returnWaarde;
        }

        //https://stackoverflow.com/a/31334882
        private static double getAngle(Point p1, Point p2, Point p3)
        {
            return Math.Atan2(p3.Y - p1.Y, p3.X - p1.X) - Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }



        private static System.Windows.Shapes.Polygon createNewPolygon(Point punt1, Point punt2, Point punt3)
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

        private static List<List<Point>> maakPolygonLijst(MultiPolygon multiPolygon)
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

            maximumXWaarde -= minimumXWaarde;
            maximumYWaarde -= minimumYWaarde;
            return polygonAlsPuntenLijst;
        }

        private static List<Point> maakPolygonLijn(List<List<Point>> multiPolygon)
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

                        /*if (Vector2.Distance(punt, normalizedPoly[0]) < Vector2.Distance(dichtsbijzijndePunt, normalizedPoly[0]))
                        {
                            dichtsbijzijndePunt = punt;
                        }*/
                    }

                    //normalizedPoly = orderList(normalizedPoly, dichtsbijzijndeNieuwePunt);
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

        private static List<Vector2> orderList(List<Vector2> normalizedPoly, Vector2 dichtsbijzijndeNieuwePunt)
        {
            int i = normalizedPoly.IndexOf(dichtsbijzijndeNieuwePunt);
            int check = i;
            List<Vector2> returnWaarde = new List<Vector2>();
            do
            {
                if (i >= normalizedPoly.Count) i -= normalizedPoly.Count;
                returnWaarde.Add(normalizedPoly[i]);
                i++;
            } while (i != check);

            return returnWaarde;
        }

        //TODO refactor dit zodat het alle polygons normalized aan het einde van de formule.
        private static List<System.Windows.Shapes.Polygon> NormalizePolygon(List<System.Windows.Shapes.Polygon> polygons)
        {
            //mss deze fn hermaken zodat deze ook ramer douglas peucker
            List<System.Windows.Shapes.Polygon> returnWaarde = new List<System.Windows.Shapes.Polygon>();
            foreach (System.Windows.Shapes.Polygon p in polygons)
            {
                PointCollection puntCollectie = new PointCollection();
                foreach (Point punt in p.Points)
                {
                    double x = (punt.X - minimumXWaarde);
                    x = (x / maximumXWaarde);
                    x = (x * 200);
                    double y = (punt.Y - minimumYWaarde);
                    y = (y / maximumYWaarde);
                    y =  (y * 200);
                    puntCollectie.Add(new Point(x, y));
                }
                Polygon pol = new Polygon();
                pol.Points = puntCollectie;
                returnWaarde.Add(pol);
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
                else
                {
                    Debug.WriteLine("distance between points = " + distance);
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

        public static Double PerpendicularDistance
    (Vector2 Point1, Vector2 Point2, Vector2 Point)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            Double area = Math.Abs(.5 * (Point1.X * Point2.Y + Point2.X *
            Point.Y + Point.X * Point1.Y - Point2.X * Point1.Y - Point.X *
            Point2.Y - Point1.X * Point.Y));
            Double bottom = Math.Sqrt(Math.Pow(Point1.X - Point2.X, 2) +
            Math.Pow(Point1.Y - Point2.Y, 2));
            Double height = area / bottom * 2;

            return height;

            //Another option
            //Double A = Point.X - Point1.X;
            //Double B = Point.Y - Point1.Y;
            //Double C = Point2.X - Point1.X;
            //Double D = Point2.Y - Point1.Y;

            //Double dot = A * C + B * D;
            //Double len_sq = C * C + D * D;
            //Double param = dot / len_sq;

            //Double xx, yy;

            //if (param < 0)
            //{
            //    xx = Point1.X;
            //    yy = Point1.Y;
            //}
            //else if (param > 1)
            //{
            //    xx = Point2.X;
            //    yy = Point2.Y;
            //}
            //else
            //{
            //    xx = Point1.X + param * C;
            //    yy = Point1.Y + param * D;
            //}

            //Double d = DistanceBetweenOn2DPlane(Point, new Point(xx, yy));
        }
    }
}