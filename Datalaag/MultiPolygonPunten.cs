using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;

namespace Datalaag
{
    public class MultiPolygonPunten
    {
        public List<PolygonPunten> PolygonPunten;

        public string Naam;


        public MultiPolygonPunten(MultiPolygon multiPolygon, string naam = "")
        {
            PolygonPunten = new List<PolygonPunten>();
            Naam = naam;
            foreach (Polygon polygon in multiPolygon.Coordinates)
            {
                PolygonPunten.Add(new PolygonPunten(polygon, naam));
            }
        }


        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }
    }
}