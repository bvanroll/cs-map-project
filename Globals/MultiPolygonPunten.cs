﻿using System.Collections.Generic;
using System.Linq;
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
            VindMaximumEnMinimum(polygonPunten);
        }

        public MultiPolygonPunten(MultiPolygon multiPolygon, string naam = "")
        {
            Naam = naam;
            PolygonPunten = new List<PolygonPunten>();
            foreach (Polygon polygon in multiPolygon.Coordinates)
            {
                PolygonPunten.Add(new PolygonPunten(polygon, naam));
            }
            VindMaximumEnMinimum(PolygonPunten);
        }

        private void VindMaximumEnMinimum(List<PolygonPunten> polygonPunten)
        {
            MaximumX = polygonPunten.Max(p => p.MaximumX);
            MaximumY = polygonPunten.Max(p => p.MaximumY);
            MinimumX = polygonPunten.Max(p => p.MinimumX);
            MinimumY = polygonPunten.Max(p => p.MinimumY);
        }
    }
}