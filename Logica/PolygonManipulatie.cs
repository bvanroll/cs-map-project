
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Datalaag;
using Globals;

namespace Logica
{
    public class PolygonManipulatie
    {
        public JsonReader JsonReader;
        public PolygonManipulatie(JsonReader jsonReader)
        {
            JsonReader = jsonReader; //haal datalaag binnen
        }

        public List<PolygonPunten> GetPolygons()
        {
            return JsonReader._polygons;
        }

        public List<MultiPolygonPunten> GetMultiPolygons()
        {
            return JsonReader._multiPolygons;
        }

        public PolygonPunten GetPolygonByName(string naam)
        {
            return JsonReader._polygons.Find(punten => punten.Naam == naam);
        }

        //oude schaalmethodes
        public PolygonPunten ScalePolygon(PolygonPunten polygon, double scaleX, double scaleY, double offsetX = 0, double offsetY = 0)
        {
            double maxX = polygon.MaximumX;
            double maxY = polygon.MaximumY;
            double minX = polygon.MinimumX;
            double minY = polygon.MinimumY;
            List<Punt> returnWaarde = new List<Punt>();
            foreach (Punt punt in polygon.Punten)
            {
                Punt x = ScalePoint(scaleX, scaleY, punt, maxX, maxY, offsetX, offsetY, minX, minY);

                //double x = punt.X - minX;
                //x /= maxX;
                //x *= scaleX;
                //x += offsetX;
                //double y = punt.Y - minY;
                //y /= maxY;
                //y *= scaleY;
                //y += offsetY;
                //returnWaarde.Add(new Punt(x, y, punt.Naam));
                //x.Naam = punt.Naam;
                returnWaarde.Add(x);
            }
            return new PolygonPunten(returnWaarde, polygon.Naam);
        }

        //lat en long = graden, graden => coords (/360 * scale?
        private static Punt ScalePoint(double scaleX, double scaleY, Punt punt, double maxX = 1, double maxY = 1, double offsetX = 0, double
        offsetY = 0, double minX = 0, double minY = 0)
        {
            double x = punt.X;
            x /= maxX;
            x *= scaleX;
            x += offsetX;
            double y = punt.Y;
            y /= maxY;
            y *= scaleY;
            y += offsetY;
            return new Punt(x, y, punt.Naam);
        }

        public MultiPolygonPunten ScaleMultiPolygon(MultiPolygonPunten multiPolygon, double scaleX, double scaleY, double offsetX = 0, double offsetY = 0)
        {
            double maxX = multiPolygon.MaximumX;
            double maxY = multiPolygon.MaximumY;
            double minX = multiPolygon.MinimumX;
            double minY = multiPolygon.MinimumY;
            List<PolygonPunten> pp = new List<PolygonPunten>();
            foreach (PolygonPunten polygon in multiPolygon.PolygonPunten)
            {
                List<Punt> returnWaarde = new List<Punt>();
                foreach (Punt punt in polygon.Punten)
                {
                    /*
                    double x = punt.X - minX;
                    x /= maxX;
                    x *= scaleX;
                    x += offsetX;
                    double y = punt.Y - minY;
                    y /= maxY;
                    y *= scaleY;
                    y += offsetY;
                    returnWaarde.Add(new Punt(x, y, punt.Naam));*/
                    
                    Punt x = ScalePoint(scaleX, scaleY, punt, maxX, maxY, offsetX, offsetY, minX, minY);
                    returnWaarde.Add(x);

                }
                pp.Add(new PolygonPunten(returnWaarde, polygon.Naam));
            }

            return new MultiPolygonPunten(pp, multiPolygon.Naam); 
        }

        //todo (offset var voor center op canvas, vree ambetant)
        public List<MultiPolygonPunten> ScaleMultiPolygons(List<MultiPolygonPunten> multiPolygons, double scaleX, double scaleY, double offsetX = 0, double offsetY = 0)
        {
            //TODO onderdeel versnelen door manuele for each loop
            double maxX = multiPolygons.Max(m => m.MaximumX);
            double maxY = multiPolygons.Max(m => m.MaximumY);
            double minX = multiPolygons.Min(m => m.MinimumX);
            double minY = multiPolygons.Min(m => m.MinimumY);
            
            List<MultiPolygonPunten> mpps = new List<MultiPolygonPunten>();
            foreach (MultiPolygonPunten mp in multiPolygons)
            {
                List<PolygonPunten> pp = new List<PolygonPunten>();
                foreach (PolygonPunten poly in mp.PolygonPunten)
                {
                    List<Punt> punten = new List<Punt>();
                    foreach (Punt punt in poly.Punten)
                    {
                        
                        Punt x = ScalePoint(scaleX, scaleY,punt, maxX, maxY, offsetX, offsetY, minX, minY);
                        x.Naam = punt.Naam;
                        punten.Add(x);
                        /*double x = punt.X - minX;
                        x /= maxX;
                        x *= scaleX;
                        x += offsetX;
                        double y = punt.Y - minY;
                        y /= maxY;
                        y *= scaleY;
                        y += offsetY;
                        punten.Add(new Punt(x, y, punt.Naam));*/
                    }
                    pp.Add(new PolygonPunten(punten, poly.Naam));
                }
                mpps.Add(new MultiPolygonPunten(pp, mp.Naam));
            }
            return mpps;

        }
        public List<PolygonPunten> GetAllPolygons()
        {
            List<PolygonPunten> lijst = new List<PolygonPunten>();
            lijst.AddRange(GetPolygons());
            foreach (MultiPolygonPunten multiPolygonPunten in GetMultiPolygons())
            {
                lijst.AddRange(multiPolygonPunten.PolygonPunten);
            }
            return lijst;
        }

        public List<PolygonPunten> TriangulatePolygon(List<Punt> punten)
        {
            List<PolygonPunten> returnWaarde = new List<PolygonPunten>();
            int i = 0;
            int BACKUP = 0;
            int BACKBACKUP = punten.Count;
            while (true)
            {
                if (i >= punten.Count)
                {
                    i = 0;
                    if (punten.Count == BACKBACKUP)
                    {
                        BACKUP++;
                    }

                    BACKBACKUP = punten.Count;
                }
                int punt1Index = i;
                int punt2Index = i + 1;
                if (punt2Index >= punten.Count) punt2Index -= punten.Count;
                int punt3Index = i + 2;
                if (punt3Index >= punten.Count) punt3Index -= punten.Count;
                if (punten.Count < 3)
                {
                    break;
                }
                double hoek = VindHoek(punten[punt2Index], punten[punt1Index], punten[punt3Index]);
                if (hoek < 180)
                {
                    returnWaarde.Add(MaakNieuweDriehoek(punten[punt2Index], punten[punt3Index],
                        punten[punt1Index], punten[punt1Index].Naam));
                    punten.RemoveAt(punt2Index);
                    Debug.WriteLine("added a triangle, polygonLijst count " + punten.Count);
                    i = punt1Index;
                    BACKUP = 0;
                    continue;
                }
                Debug.WriteLine(hoek);
                i++;
                if (BACKUP >= punten.Count)
                {
                    Debug.WriteLine("FUCK, couldnt parse " + punten.Count + " points");
                    break;
                }
            }
            return returnWaarde;
        }

        public List<PolygonPunten> TriangulatePolygon(PolygonPunten polygon)
        {
            List<Punt> punten = polygon.Punten;
            List<PolygonPunten> returnWaarde = new List<PolygonPunten>();
            int i = 0;
            int BACKUP = 0;
            int BACKBACKUP = punten.Count;
            while (true)
            {
                if (i >= punten.Count)
                {
                    i = 0;
                    if (punten.Count == BACKBACKUP)
                    {
                        BACKUP++;
                    }

                    BACKBACKUP = punten.Count;
                }
                int punt1Index = i;
                int punt2Index = i + 1;
                if (punt2Index >= punten.Count) punt2Index -= punten.Count;
                int punt3Index = i + 2;
                if (punt3Index >= punten.Count) punt3Index -= punten.Count;
                if (punten.Count < 3)
                {
                    break;
                }
                double hoek = VindHoek(punten[punt2Index], punten[punt1Index], punten[punt3Index]);
                if (hoek < 180)
                {
                    returnWaarde.Add(MaakNieuweDriehoek(punten[punt2Index], punten[punt3Index],
                        punten[punt1Index], punten[punt1Index].Naam));
                    punten.RemoveAt(punt2Index);
                    Debug.WriteLine("added a triangle, polygonLijst count " + punten.Count);
                    i = punt1Index;
                    BACKUP = 0;
                    continue;
                }
                Debug.WriteLine(hoek);
                i++;
                if (BACKUP >= punten.Count)
                {
                    Debug.WriteLine("FUCK, couldnt parse " + punten.Count + " points");
                    break;
                }
            }
            return returnWaarde;
        }

        

        private PolygonPunten MaakNieuweDriehoek(Punt punt, Punt punt1, Punt punt2, string naam = "")
        {
            return new PolygonPunten(new List<Punt>() { punt, punt1, punt2 }, naam);
        }

        private double VindHoek(Punt p1, Punt p2, Punt p3)
        {
            double hoek = (Math.Atan2(p3.Y - p1.Y, p3.X - p1.X) - Math.Atan2(p2.Y - p1.Y, p2.X - p1.X)) * (180 / Math.PI);
            if (hoek < 0)
            {
                hoek += 360;
            }
            return hoek;
        }

        public PolygonPunten Peuker(PolygonPunten polygon, double epsilon)
        {
            double nX = Math.Abs(Math.Abs(polygon.MaximumX) - Math.Abs(polygon.MinimumX));
            double nY = Math.Abs(Math.Abs(polygon.MaximumY) - Math.Abs(polygon.MinimumY));
            epsilon = ((nX + nY) / 2) * epsilon;
            polygon.Punten = Peuker(polygon.Punten, epsilon);
            return polygon;

        }

        public MultiPolygonPunten Peuker(MultiPolygonPunten polygons, double epsilon)
        {
            foreach (PolygonPunten polygon in polygons.PolygonPunten)
            {
                double nX = Math.Abs(Math.Abs(polygon.MaximumX) - Math.Abs(polygon.MinimumX));
                double nY = Math.Abs(Math.Abs(polygon.MaximumY) - Math.Abs(polygon.MinimumY));
                epsilon = ((nX + nY) / 2) * epsilon;

                polygon.Punten = Peuker(polygon.Punten, epsilon);
            }
            return polygons;

        }


        //
        /*
         * De reden dat peuker enkel werkte op bepaalde landen was wegens de grote van deze landen/ polygons in deze landen. 
         * Sommige landen waren zo klein dat het double datatype een limiet bereikt had van precisie (bv 0.00000000005 0.00000000007 zouden worden herleid naar 0.0000000001)
         * De afstands methode die een afstand probeert te berekenen tussen 2 punten doet op een bepaald moment een aftrekking en als de punten gelijk waren komt in die var een 0
         * daarna wordt deze var gebruikt voor een deling, en een deling door 0 geeft geen error, maar een NaN type in het double datatype
         * Daardoor ging peuker niet op kleinere landen zoals lichtenstein
         * D
         *
         *De oplossing voor dit probleem was door het gebruik van de contains methode in de polygonpunten klasse
         * elk punt dat toegevoegd wordt wordt gechecked of het al in de lijst zit. Dit vertraagd het inladen van de data enorm, maar zorgt ervoor dat alle landen puntvermindering kunnen krijgen
         * 
         * 
         * 
         */
        private List<Punt> Peuker(List<Punt> punten, double epsilon)
        {
            double dmax = -1;
            int index = 0;
            int end = punten.Count;

            for (int i = 1; i < end - 1; i++)
            {
                double distance = PerpendicularDistance2(punten[i], punten[0], punten[end - 1]);
                if (distance > dmax)
                {
                    index = i;
                    dmax = distance;
                } 
                else
                {
                    Debug.WriteLine("hmm");
                }
            }


            List<Punt> returnWaarde = new List<Punt>();

            if (dmax > epsilon)
            {
                List<Punt> recResults1 = Peuker(punten.GetRange(0, index), epsilon);
                List<Punt> recResults2 = Peuker(punten.GetRange(index, end - 1 - index), epsilon);


                returnWaarde.AddRange(recResults1);
                returnWaarde.AddRange(recResults2);
            }
            else
            {
                returnWaarde = new List<Punt>() { punten[0], punten[punten.Count - 1] };
            }

            return returnWaarde;
        }

        private double PerpendicularDistance2(Punt point, Punt l1, Punt l2)
        {
            double temp = Math.Abs((l2.X - l1.X) * (l1.Y - point.Y) - (l1.X - point.X) * (l2.Y - l1.Y)) /
                   Math.Sqrt(Math.Pow(l2.X - l1.X, 2) + Math.Pow(l2.Y - l1.Y, 2));
            if (double.IsNaN(temp))
            {
                Debug.WriteLine("wtf fam");
                return 0;
            }
            return temp;
        }
    }
}
