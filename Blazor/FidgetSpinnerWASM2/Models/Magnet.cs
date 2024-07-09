using SkiaSharp;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace FidgetSpinnerWASM2.Models
{
    [DataContract]
    public class Magnet
    {
        public event EventHandler OnRequestToDraw;
        public Magnet()
        {
            Id = Guid.NewGuid();
        }
        // For magnet removal from menu
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        ForceCalculation ForceCalculation { get; set; } = ForceCalculation.Characteristics;
        [DataMember]
        public float R { get; set; } = (float)(8 / 1000.0F);
        [DataMember]
        public float H { get; set; } = (float)(6 / 1000.0F);
        [DataMember]
        public float RadialTh { get; set; } = (float)(0);
        [DataMember]
        public float Mass { get; set; } = 10 / 1000.0F;
        bool _pol = true, _isRad = false;

        [DataMember]
        float _mu { get; set; } = 1.366F;
        public float moment { get => _mu; set { _mu = value; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float Rmm { get => R * 1000.0F; set { R = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float Hmm { get => H * 1000.0F; set { H = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float Massg { get => Mass * 1000.0F; set { Mass = value / 1000.0F; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
        public float RadialThDeg{ get => (float)(RadialTh * 180 / Math.PI); set { RadialTh = value / 180.0F * (float)Math.PI; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }

        [DataMember]
        public bool Polarity { get { return _pol; } set { _pol = value; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }

        [DataMember]
        public bool IsRadial { get { return _isRad; } set { _isRad = value; OnRequestToDraw?.Invoke(this, EventArgs.Empty); } }
    }
}
