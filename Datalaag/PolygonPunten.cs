using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;

namespace Datalaag
{
    public class PolygonPunten
    {
        public double MinimumXWaarde, MinimumYWaarde, MaximumXWaarde, MaximumYWaarde;
        public List<Punt> Punten;
        public string Naam;


        public PolygonPunten(Polygon polygon, string naam = "")
        {
            Naam = naam;
            MinimumXWaarde = double.MaxValue;
            MinimumYWaarde = double.MaxValue;
            MaximumXWaarde = double.MinValue;
            MaximumYWaarde = double.MinValue;
            LeesPuntenVanPolygon(polygon);

        }

        public PolygonPunten(List<Punt> polygon, string naam = "")
        {
            Naam = naam;
            Punten = polygon;
            MaximumXWaarde = Punten.Max(p => p.X);
            MinimumXWaarde = Punten.Min(p => p.X);
            MaximumYWaarde = Punten.Max(p => p.Y);
            MinimumYWaarde = Punten.Min(p => p.Y);
        }




        private void LeesPuntenVanPolygon(Polygon polygon)
        {
            foreach (LineString lijn in polygon.Coordinates)
            {
                foreach (Position positie in lijn.Coordinates)
                {
                    CheckMinMaxWaarden(positie);
                    Punten.Add(new Punt(positie.Longitude, positie.Latitude, Naam));
                }
            }
        }

        private void CheckMinMaxWaarden(Position positie)
        {
            if (positie.Longitude > MaximumXWaarde) MaximumXWaarde = positie.Longitude;
            if (positie.Longitude < MinimumXWaarde) MinimumXWaarde = positie.Longitude;
            if (positie.Latitude > MaximumYWaarde) MaximumYWaarde = positie.Latitude;
            if (positie.Latitude < MinimumYWaarde) MinimumYWaarde = positie.Latitude;
        }
        
        
        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }
    }
}