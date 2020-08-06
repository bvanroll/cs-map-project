using System;
using System.Collections.Generic;
using GeoJSON.Net.Geometry;

namespace Globals
{
    public class MultiPolygonPunten
    {
        public double MaximumX, MinimumX, MaximumY, MinimumY;
        public List<PolygonPunten> PolygonPunten;
        public string Naam;

        public MultiPolygonPunten(List<PolygonPunten> polygonPunten, string naam = "")
        {
            PolygonPunten = polygonPunten;
            Naam = naam;
            UpdateMaximumEnMinimum();
        }

        public MultiPolygonPunten(MultiPolygon multiPolygon, string naam = "")
        {
            Naam = naam;
            PolygonPunten = new List<PolygonPunten>();
            bool reverse = true;
            foreach (Polygon polygon in multiPolygon.Coordinates)
            {
                PolygonPunten p = new PolygonPunten(polygon, naam, reverse);
                PolygonPunten.Add(p);
                reverse = !reverse; //reverse parameter voor polygonpunten is sneler dan 
                UpdateMaximumEnMinimum(p);
            }
        }

        private void UpdateMaximumEnMinimum(PolygonPunten polygon)
        {
            if (polygon.MaximumX > MaximumX) MaximumX = polygon.MaximumX;
            else if (polygon.MinimumX < MinimumX) MinimumX = polygon.MinimumX;
            if (polygon.MaximumY > MaximumY) MaximumY = polygon.MaximumY;
            else if (polygon.MinimumY < MinimumY) MinimumY = polygon.MinimumY;
        }

        public void UpdateMaximumEnMinimum()
        {
            foreach (PolygonPunten polygon in PolygonPunten)
            {
                UpdateMaximumEnMinimum(polygon);
            }
        }

        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }
    }
}