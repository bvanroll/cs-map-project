﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Datalaag;

namespace Logica
{
    public class PolygonManipulatie
    {
        public JsonReader JsonReader;
        public PolygonManipulatie(JsonReader jsonReader)
        {
            JsonReader = jsonReader;
        }

        public List<PolygonPunten> getPolygons()
        {
            return JsonReader._polygons;
        }

        public List<MultiPolygonPunten> getMultiPolygons()
        {
            return JsonReader._multiPolygons;
        }

        public List<PolygonPunten> getAllPolygons()
        {
            List<PolygonPunten> lijst = new List<PolygonPunten>();
            lijst.AddRange(getPolygons());
            foreach (MultiPolygonPunten multiPolygonPunten in getMultiPolygons())
            {
                lijst.AddRange(multiPolygonPunten.PolygonPunten);
            }
            return lijst;
        }

        public List<PolygonPunten> triangulatePolygon(List<Punt> punten)
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
                        punten[punt1Index]));
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
            return new PolygonPunten(new List<Punt>(){punt, punt1, punt2}, naam);
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

        private List<Punt> peuker(List<Punt> punten, double epsilon)
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


            List<Punt> returnWaarde = new List<Punt>();

            if (dmax > epsilon)
            {
                List<Punt> recResults1 = peuker(punten.GetRange(0, index), epsilon);
                List<Punt> recResults2 = peuker(punten.GetRange(index, end-1-index), epsilon);
                

                returnWaarde.AddRange(recResults1);
                returnWaarde.AddRange(recResults2);
            }
            else
            {
                returnWaarde = new List<Punt>() { punten[0], punten[punten.Count-1] };
            }

            return returnWaarde;
        }

        private double PerpendicularDistance2(Punt point, Punt l1, Punt l2)
        {
            return Math.Abs((l2.X - l1.X) * (l1.Y - point.Y) - (l1.X - point.X) * (l2.Y - l1.Y)) /
                   Math.Sqrt(Math.Pow(l2.X - l1.X, 2) + Math.Pow(l2.Y - l1.Y, 2));
        }
    }
}
