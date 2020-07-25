using System;

namespace Datalaag
{
    public class Punt
    {
        public double X, Y;
        public string Naam;
        public Punt(double x, double y, string naam = "")
        {
            Naam = naam;
            X = x;
            Y = y;
        }
        
        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }
    }
}