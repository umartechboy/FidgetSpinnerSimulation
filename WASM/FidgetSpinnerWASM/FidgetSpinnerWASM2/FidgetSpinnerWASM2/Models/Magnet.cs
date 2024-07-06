using SkiaSharp;
using System.Security.Cryptography.X509Certificates;

namespace FidgetSpinnerWASM2.Models
{
    public class Magnet
    {
        public Magnet()
        {
        }
        public double H { get; set; } = 6 / 0.001;
        public float R { get; set; } = (float)(8 / 1000.0F);
        public double Mass { get; set; } = 10 / 1000;
        public bool Polarity { get; set; } = true;
    }
}
