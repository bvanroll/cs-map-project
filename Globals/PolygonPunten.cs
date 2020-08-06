using System;
using System.Collections.Generic;
using System.Linq;
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
            MaximumX = Double.MinValue;
            MaximumY = Double.MinValue;
            MinimumX = Double.MaxValue;
            MinimumY = Double.MaxValue;

            UpdateMaxEnMinPunt();
        }

        public PolygonPunten(Polygon polygon, string naam = "", bool reverse = true)
        {
            Naam = naam;
            Punten = new List<Punt>();
            MaximumX = Double.MinValue;
            MaximumY = Double.MinValue;
            MinimumX = Double.MaxValue;
            MinimumY = Double.MaxValue;
            
            foreach (LineString linestring in polygon.Coordinates)
            {
                if (reverse)
                {
                    foreach (Position positie in linestring.Coordinates.Reverse()) //sneller om eerst te reversen dan tegen einde deze bewerking te doen
                    {
                        Punt punt = new Punt(positie.Longitude, positie.Latitude, naam);
                    
                        if (!Punten.Contains(punt)) {
                            Punten.Add(punt); //dit vertraagd programma enorm, maar zorgt ervoor dat peuker beter werkt denk ik
                            UpdateMaxEnMinPunt(punt); //sneller dan eindigen met punten.max en punten.min
                        }
                        //de vertraging komt vooral door de .Contains methode, deze mag weggelaten worden voor snelheid maar peuker zal niet meer zo goed werken 
                    }
                }
                else
                {
                    foreach (Position positie in linestring.Coordinates) 
                    {
                        Punt punt = new Punt(positie.Longitude, positie.Latitude, naam);
                    
                        if (!Punten.Contains(punt)) {
                            Punten.Add(punt); //dit vertraagd programma enorm, maar zorgt ervoor dat peuker beter werkt denk ik
                            UpdateMaxEnMinPunt(punt); //sneller dan eindigen met punten.max en punten.min
                        }
                        //de vertraging komt vooral door de .Contains methode, deze mag weggelaten worden voor snelheid maar peuker zal niet meer zo goed werken 
                    }
                }
                
            }
        }

        private void UpdateMaxEnMinPunt(Punt punt)
        {
            if (punt.X > MaximumX) MaximumX = punt.X;
            else if (punt.X < MinimumX) MinimumX = punt.X;
            if (punt.Y > MaximumY) MaximumY = punt.Y;
            else if (punt.Y < MinimumY) MinimumY = punt.Y;
        }

        public void UpdateMaxEnMinPunt()
        {
            foreach (Punt punt in Punten)
            {
                UpdateMaxEnMinPunt(punt);
            }
        }


        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            return Naam;
        }

        public MultiPolygonPunten ToMultiPolygonPunten()
        {
            return new MultiPolygonPunten(new List<PolygonPunten>() { this }, this.Naam);
        }
    }
}

