using System.Text;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class PhysLoggerSharedResources
    {
        public static int SignificantFiguresAfterDecimal { get; set; } = 3;
        public static int SignificantFiguresAfterDecimalForFiles { get; set; }
        public static NumberFormatting NumberFormat { get; set; } = NumberFormatting.None;
        public static NumberFormatting NumberFormatForFiles { get; set; } = NumberFormatting.None;
        public enum NumberFormatting { Exponents, None, Suffix };
        public static string FormatNumber(float N)
        {
            return FormatNumber(N, SignificantFiguresAfterDecimal, NumberFormat);
        }
        internal static float ParseNumber(string v)
        {
            try { return float.Parse(v); }
            catch
            {
                try
                {
                    for (int i = 0; i < prefixes.Length; i++)
                    {
                        if (prefixes[i] == ' ')
                            continue;
                        if (v.Contains(prefixes[i]))
                        {
                            var f = float.Parse(v.Replace(prefixes[i].ToString(), ""));
                            return (float)(Math.Pow(10, 3 * (i - 5)) * f);
                        }
                    }
                }
                catch { return 0; }
            }
            return 0;
        }
        public static string FormatNumber(float N, int significantFigure, NumberFormatting format)
        {
            if (format == NumberFormatting.Exponents)
            {
                string comp = N.ToString("E" + significantFigure);
                var num = comp.Split(new char[] { 'E' })[0];
                var exp = comp.Split(new char[] { 'E' })[1];
                if (int.Parse(exp) != 0)
                    return num + "e" + int.Parse(exp).ToString();
                else return num;
            }
            else if (format == NumberFormatting.None)
            {
                string s = N.ToString("F" + significantFigure);
                if (s.Contains("."))
                {
                    s = s.TrimEnd(new char[] { '0' });
                    s = s.TrimEnd(new char[] { '.' });
                }
                if (s.Length == 0)
                    s = "0";
                return s;
            }
            else
                return roundedFrac(N, significantFigure);
        }

        static string prefixes = "afpum kMTPA";
        static string roundedFrac(float number, int significantFigures)
        {
            var isNeg = number < 0;
            if (isNeg)
                number = -number;

            if (number == 0)
                return "0";
            int multi = 5;
            while (number < 1 && multi > 0)
            { number *= 1000; multi--; }
            while (number >= 1000 && multi < 10)
            { number /= 1000; multi++; }
            if (isNeg)
                number *= -1;
            string ns = number.ToString("F" + significantFigures);
            if (ns.Contains('.'))
            {
                var mant = ns.Substring(ns.IndexOf('.')).TrimEnd(new char[] { '0', '.' });
                ns = ns.Substring(0, ns.IndexOf('.')) + mant;
            }
            if (multi != 5)
                ns += prefixes[multi];
            return ns;
        }
    }
}
