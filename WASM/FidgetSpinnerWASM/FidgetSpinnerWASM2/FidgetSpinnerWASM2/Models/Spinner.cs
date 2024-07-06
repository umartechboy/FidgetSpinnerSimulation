using static MudBlazor.CategoryTypes;
using System.Runtime.CompilerServices;
using System;
using SkiaSharp;
using System.Numerics;
using static MudBlazor.Colors;
using System.Threading.Tasks;

namespace FidgetSpinnerWASM2.Models
{
    public class Spinner
    {
        public List<Magnet> Magnets { get; set; } = new List<Magnet>();

        public Spinner() { }
        public int N { get => Magnets.Count; }
        public Spinner(int nMagnets, float r, Vector3 position, params bool [] polarity)
        {
            if (polarity == null)
                polarity = new bool [] { true};
            while (polarity.Length < nMagnets)
            {
                var pp = new List<bool>(polarity)
                {
                    polarity[0]
                };
                polarity = pp.ToArray();
            }
            foreach(var p in polarity)
                Magnets.Add(new Magnet() { Polarity = p });
            Position = position;
        }
        public double B { get; set; } = 0.002; // random Friction
        public Vector3 Position { get; set; } // 3D position
        public float R { get; set; } = 40 / 1000.0F; // Radius.Also used to initialize magnets
        public double tau { get; set; } = 0; // torque, for reference only
        public double a { get; set; } = 0; // rotational acceleration, for reference only
        public double I { get; set; } = 0.00002;
        public double w { get; set; } = 0; // rotational velocity
        public double th { get; set; } = 0; // rotational position
        public bool IsPowered { get; set; } = false;
        public void AddMagnet(Magnet magnet)
        {
            this.Magnets.Add(magnet);
        }
        public void Draw(SKCanvas canvas)
        {
            var thD = 2 * Math.PI / Magnets.Count;
            for (int ii = 0; ii < Magnets.Count; ii++) {

                var thI = th + thD * (ii - 1);
                var cx = (float)(R * (float)Math.Cos(thI) + Position.X);
                var cy = (float)(R * (float)Math.Sin(thI) + Position.Y);
                SKColor col = SKColors.Black;
                if (Magnets[ii].Polarity)
                    col = SKColors.Red;
                var paint = new SKPaint()
                {
                    IsStroke = true,
                    Color = col,
                    StrokeWidth = 0.002F,
                };
                canvas.DrawCircle(cx, cy, Magnets[ii].R, paint);
                paint.IsStroke = false;
                paint.Color = paint.Color.WithAlpha(150);
                canvas.DrawCircle(cx, cy, Magnets[ii].R, paint);
                if (IsPowered)
                    col = SKColors.Red;
                else
                    col = SKColors.Black;
                paint.Color = col;
                canvas.DrawLine(
                    cx, cy, Position.X, Position.Y, paint);
            }
        }
    }
}
