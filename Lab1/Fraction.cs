using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI311.Labs
{
    public class Fraction
    {
        public int numerator;
        public int denominator;

        public int Numerator
        {
            get { return numerator; }
            set { numerator = value; }
        }
        
        public int Denominator
        {
            get { return denominator; }
            set { denominator = value; }
        }

        public Fraction(int n = 0, int d = 1)
        {
            numerator = n;
            if (d == 0) d = 1; // avoid dividing by 0
            denominator = d;
        }

        public static Fraction multiply(Fraction a, Fraction b)
        {
            return new Fraction(a.numerator * b.numerator, a.denominator * b.denominator);
        }
    }
}