using SkiaSharp;
using System.Security.Cryptography.X509Certificates;

namespace FidgetSpinnerWASM2.Models
{
    public class Magnet
    {
        public event EventHandler OnRequestToDraw;
        public Magnet()
        {
        }
        public float H { get; set; } = 6 / 1000.0F;
        public float R { get; set; } = (float)(8 / 1000.0F);
        public float Mass { get; set; } = 10 / 1000.0F;
        bool _pol = true;
        public float Hmm { get => H * 1000.0F; set { H = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float Rmm { get => R * 1000.0F; set { R = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float Massg { get => Mass * 1000.0F; set { Mass = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public bool Polarity { get { return _pol; } set { _pol = value; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
    }
}
