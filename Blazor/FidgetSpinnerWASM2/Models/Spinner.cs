using static MudBlazor.CategoryTypes;
using System.Runtime.CompilerServices;
using System;
using SkiaSharp;
using System.Numerics;
using static MudBlazor.Colors;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Typography.OpenFont.Tables;

namespace FidgetSpinnerWASM2.Models
{
    [DataContract]
    public class Spinner
    {
        public SpinnerSimResult SimResult { get; set; } = new();
        [DataMember]
        public List<Magnet> Magnets { get; set; } = new List<Magnet>();
        public event EventHandler OnRequestToDraw;
        public Spinner() { }
        public int N { get => Magnets.Count; }
        static int spinnerID = 1;
        [DataMember]
        public int ID { get; private set; }
        public Spinner(int nMagnets, float r, Vector3 position, params bool [] polarity)
        {
            ID = spinnerID++;
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
                AddMagnet(new Magnet() { Polarity = p });
            
            Position = position;
        }
        [DataMember]
        public double B { get; set; } = 0.002; // random Friction
        public float BNmms { get => (float)(B * 1000); set { B = value / 1000.0F; } }
        [DataMember]
        public Vector3 Position { get; set; } // 3D position
        [DataMember]
        public float RPM { get => (float)(w / 2 / Math.PI * 60); set => w = value / 60.0F * 2 * Math.PI; }
       
        public float Xmm { get => Position.X * 1000; set { Position = new Vector3(value / 1000.0F, Position.Y, Position.Z); } }
        public float Ymm { get => Position.Y * 1000; set { Position = new Vector3(Position.X, value / 1000.0F, Position.Z); } }
        public float Rmm { 
            get => R * 1000; 
            set => R = value / 1000.0F; }
        [DataMember]
        public float R { get; set; } = 40 / 1000.0F; // Radius.Also used to initialize magnets
        [DataMember] 
        public double tau { get; set; } = 0; // torque, for reference only
        [DataMember] 
        public double a { get; set; } = 0; // rotational acceleration, for reference only
        [DataMember] 
        public double I { get; set; } = 0.00002;
        [DataMember] 
        public double w { get; set; } = 0; // rotational velocity
        [DataMember] 
        public double th { get; set; } = 0; // rotational position
        [DataMember] 
        public bool IsPowered { get; set; } = false;
        public void AddMagnet(Magnet magnet)
        {
            this.Magnets.Add(magnet);
            magnet.OnRequestToDraw += (s, e) => OnRequestToDraw?.Invoke(s, e);
        }
        public void RemoveMagnet(Magnet magnet)
        {
            this.Magnets.Remove(magnet);
            magnet.OnRequestToDraw += (s, e) => OnRequestToDraw?.Invoke(s, e);
        }
        public float TotalI()
        {
            return (float)(I + Magnets.Sum(m => 0.5F * m.Mass * R * R));
        }
        public void Draw(SKCanvas canvas, bool showLabels)
        {
            var thD = 2 * Math.PI / Magnets.Count;
            if (showLabels)
            {
                canvas.Save();
                canvas.Scale(1, -1);
                canvas.DrawText(ID.ToString(), Position.X, -Position.Y, new SKPaint(new SKFont()
                {
                    Size = 0.02F
                })
                {
                    Color = SKColors.Blue.WithAlpha(150),
                    IsStroke = false,
                    IsAntialias = true
                });
                canvas.Restore();
            }
            for (int ii = 0; ii < Magnets.Count; ii++) {

                var thI = th + thD * ii;
                var cx = (float)(R * (float)Math.Cos(thI) + Position.X);
                var cy = (float)(R * (float)Math.Sin(thI) + Position.Y);
                SKColor colS = SKColors.Black;
                SKColor colN = SKColors.Red;
                canvas.Save();
                canvas.Translate(cx, cy);
                if (Magnets[ii].IsRadial)
                {
                    var paintN = new SKPaint()
                    {
                        IsStroke = true,
                        Color = colN,
                        StrokeWidth = 0.002F,
                    };
                    var paintS = new SKPaint()
                    {
                        IsStroke = true,
                        Color = colS,
                        StrokeWidth = 0.002F,
                    };
                    var m = Magnets[ii];
                    SKRect rectS = new SKRect(-m.H / 2, -m.R, 0, m.R);
                    SKRect rectN = new SKRect(0, -m.R, m.H / 2, m.R);
                    canvas.RotateRadians((float)(Magnets[ii].RadialTh + thI), 0, 0);

                    if (!Magnets[ii].Polarity)
                    {
                        var t = rectS;
                        rectS = rectN;
                        rectN = t;
                    }
                    //canvas.DrawRect(rectS, paintS);
                    paintS.IsStroke = false;
                    paintS.Color = paintS.Color.WithAlpha(150);
                    canvas.DrawRect(rectS, paintS);

                    //canvas.DrawRect(rectN, paintN);
                    paintN.IsStroke = false;
                    paintN.Color = paintN.Color.WithAlpha(150);
                    canvas.DrawRect(rectN, paintN);
                }
                else
                {
                    var paint = new SKPaint()
                    {
                        IsStroke = true,
                        Color = colS,
                        StrokeWidth = 0.002F,
                    };
                    if (Magnets[ii].Polarity)
                        paint.Color = colN;

                    canvas.DrawCircle(0, 0, Magnets[ii].R, paint);
                    paint.IsStroke = false;
                    paint.Color = paint.Color.WithAlpha(150);
                    canvas.DrawCircle(0, 0, Magnets[ii].R, paint);
                }
                canvas.Restore();
                if (showLabels)
                {
                    // text will be inverted. transform the canvas
                    canvas.Save();
                    canvas.Scale(1, -1);
                    canvas.DrawText((ii + 1).ToString(), cx, -cy - Magnets[ii].R, new SKPaint(new SKFont()
                    {
                        Size = 0.02F
                    })
                    {
                        Color = SKColors.Blue.WithAlpha(150),
                        IsStroke = false,
                        IsAntialias = true
                    });
                    canvas.Restore();
                }
                var paintL = new SKPaint()
                {
                    IsStroke = true,
                    Color = colS,
                    StrokeWidth = 0.002F,
                };
                if (IsPowered)
                    paintL.Color = colN;
                canvas.DrawLine(
                    cx, cy, Position.X, Position.Y, paintL);
            }
        }
    }
}
