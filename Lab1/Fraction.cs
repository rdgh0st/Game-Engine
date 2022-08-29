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
            Simplify();
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            return new Fraction((a.numerator * b.denominator) + (b.numerator * a.denominator), (a.denominator * b.denominator));
        }
        
        public static Fraction operator -(Fraction a, Fraction b)
        {
            return new Fraction((a.numerator * b.denominator) - (b.numerator * a.denominator), (a.denominator * b.denominator));
        }

        public static Fraction operator *(Fraction a, Fraction b)
        {
            return new Fraction(a.numerator * b.numerator, a.denominator * b.denominator);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            return new Fraction(a.numerator * b.denominator, a.denominator * b.numerator);
        }

        public override string ToString()
        {
            return numerator + "/" + denominator;
        }

        private void Simplify()
        {
            if (denominator < 0)
            {
                denominator *= -1;
                numerator *= -1;
            }
            int gcd = GCD(numerator, denominator);
            numerator /= gcd;
            denominator /= gcd;
        }

        public static int GCD(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            if (a == 0)
            {
                return b;
            }

            if (b == 0)
            {
                return a;
            }

            if (b > a)
            {
                return GCD(a, b % a);
            }
            else
            {
                return GCD(a % b, b);
            }
        }
        
    }
}