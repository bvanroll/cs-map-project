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
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            if (string.Equals(Naam, "", StringComparison.Ordinal)) return "UNKNOWN";
            else return Naam;
        }

        public Point GetPoint()
        {
            return new Point(X, Y);
        }

    }
}
