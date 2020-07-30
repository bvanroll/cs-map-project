using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Globals
{
    public class Punt 
    {
        public double X, Y;
        public string Naam;
        public Punt(double x, double y, string naam = "")
        {
            Naam = naam;
            X = ConvertToRadians(x);
            Y = ConvertToRadians(y);
        }

        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }
        
        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        
        public Point ToPoint()
        {
            return new Point(X, Y);
        }

    }
}
