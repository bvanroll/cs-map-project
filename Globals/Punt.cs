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
            if (Graden)
            {
                X = ConvertToRadians(x) * 100;
                Y = ConvertToRadians(y) * 100;
            }
            else
            {
                X = x;
                Y = y;
            }
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

        public bool Graden = true;

        public override bool Equals(object obj)
        {
            try
            {
                Punt p = (Punt)obj;
                return (p.X == this.X && p.Y == this.Y); // && this.Name = p.Name
                
            } catch (Exception)
            {
                return false;
            }
        }
    }
}
