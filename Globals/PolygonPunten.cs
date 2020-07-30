using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using GeoJSON.Net.Geometry;
namespace Globals
{
    public class PolygonPunten
    {
        public double MaximumX, MinimumX, MaximumY, MinimumY;
        public List<Punt> Punten;
        public string Naam;
        
        public PolygonPunten(List<Punt> punten, string naam = "")
        {
            Punten = punten;
            Naam = naam;
            MaximumX = punten.Max(punt => punt.X);
            MaximumY = punten.Max(punt => punt.Y);
            MinimumX = punten.Min(punt => punt.X);
            MinimumY = punten.Min(punt => punt.Y);
        }

        public PolygonPunten(GeoJSON.Net.Geometry.Polygon p, string naam = "")
        {
            Naam = naam;
            Punten = new List<Punt>();
            foreach (LineString l in p.Coordinates)
            {
                foreach (Position pos in l.Coordinates)
                {
                    Punten.Add(new Punt(pos.Longitude, pos.Latitude, naam));
                }
            }
            Punten.Reverse();
            MaximumX = Punten.Max(punt => punt.X);
            MaximumY = Punten.Max(punt => punt.Y);
            MinimumX = Punten.Min(punt => punt.X);
            MinimumY = Punten.Min(punt => punt.Y);
        }

        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }

        public MultiPolygonPunten ToMultiPolygonPunten()
        {
            return new MultiPolygonPunten(new List<PolygonPunten>() { this }, this.Naam);
        }
    }
}

