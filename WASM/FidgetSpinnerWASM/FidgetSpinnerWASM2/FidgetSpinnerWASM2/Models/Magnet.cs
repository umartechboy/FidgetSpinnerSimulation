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
        public void SetRmm(float rmm)
        {
            Rmm = rmm;
            OnRequestToDraw?.Invoke(this, EventArgs.Empty);
        }
        public void SetHmm(float hmm)
        {
            Hmm = hmm;
            OnRequestToDraw?.Invoke(this, EventArgs.Empty);
        }
        public void SetMassg(float massg)
        {
            Massg = massg;
            OnRequestToDraw?.Invoke(this, EventArgs.Empty);
        }
        public float H { get; set; } = 6 / 1000.0F;
        public float R { get; set; } = (float)(8 / 1000.0F);
        public float Mass { get; set; } = 10 / 1000.0F;
        public float Hmm { get => H * 1000.0F; set => H = value / 1000.0F ; }
        public float Rmm { get => R * 1000.0F; set => R = value / 1000.0F; }
        public float Massg { get => Mass * 1000.0F; set => Mass = value / 1000.0F; }
        public bool Polarity { get; set; } = true;
    }
}
