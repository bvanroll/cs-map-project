﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using GeoJSON.Net.Geometry;
using Polygon = GeoJSON.Net.Geometry.Polygon;

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

        public PolygonPunten(Polygon p, string naam = "")
        {
            Naam = naam;
            Punten = new List<Punt>();
            MaximumX = double.MinValue;
            MaximumY = double.MinValue;
            MinimumX = double.MaxValue;
            MinimumY = double.MaxValue;
            foreach (LineString l in p.Coordinates)
            {
                foreach (Position pos in l.Coordinates)
                {
                    if (pos.Longitude > MaximumX) MaximumX = pos.Longitude;
                    if (pos.Longitude < MinimumX) MinimumX = pos.Longitude;
                    if (pos.Latitude > MaximumY) MaximumY = pos.Latitude;
                    if (pos.Latitude < MinimumY) MinimumY = pos.Latitude;
                    Punten.Add(new Punt(pos.Longitude, pos.Latitude, naam));
                }
            }
        }
    }
}

