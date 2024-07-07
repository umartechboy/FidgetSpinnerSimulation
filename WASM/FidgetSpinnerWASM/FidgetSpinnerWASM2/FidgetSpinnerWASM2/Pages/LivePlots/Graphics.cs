using CSharpMath.SkiaSharp;
using SkiaSharp;
using System.Drawing;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class DXString
    {
        public string Text { get; set; } = "";
        public SKColor Color { get; set; }
        public SKFont DXFont { get; set; }
    }
    public class SKGraphics
    {
        public static SKRect MeasureString(DXString str)
        {
            return MeasureString(str.Text, str.DXFont.Typeface.FamilyName, str.DXFont.Size);
        }
        public static SKRect MeasureString(string str, string fontFamily, float height)
        {
            if ((str.StartsWith("$") && str.EndsWith("$") && str.Length >= 2) || str.StartsWith("$$")) // latex
            {
                var painter = new CSharpMath.SkiaSharp.MathPainter();
                painter.FontSize = height * 0.8F;
                painter.LaTeX = str;
                if (str.StartsWith("$") && str.EndsWith("$"))
                    painter.LaTeX = str.Substring(1, str.Length - 2);
                else
                    painter.LaTeX = str.Substring(2, str.Length - 2);
                var r = painter.Measure();
                return MakeRect(r.Left, r.Top, r.Width, r.Height);
            }
            var tf = SKTypeface.FromFamilyName(fontFamily);
            var headerPaint = new SKPaint
            {
                //Color = new SKColor(str.Color.R, str.Color.G, str.Color.B, str.Color.A),
                TextSize = height * 0.9F,
                Typeface = SKTypeface.FromFamilyName(fontFamily),
                IsAntialias = true,
            };
            if (str.Contains("\n"))
            {
                var lines = str.Replace("\r", "").Split('\n').ToList();
                var wid = lines.Max(line => headerPaint.MeasureText(line));

                return MakeRect(0, 0, wid, height * lines.Count);
            }
            else
            {
                var wid = headerPaint.MeasureText(str);
                return MakeRect(0, 0, wid, height);
            }
        }
        public static SKRect MakeRect(float left, float top, float width, float height)
        {
            return new SKRect(left, top, left + width, top + height);
        }
        public static SKRect MakeRect(SKPoint point, SKSize size)
        {
            return new SKRect(point.X, point.Y, point.X + size.Width, point.Y + size.Height);
        }

        public static void DrawRoundedRectangle(SKCanvas canvas, Pen pen, RectangleF rectangleF, int radius)
        {
            var sRect = new SKRect(rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom);
            var rect = new SKRoundRect(sRect, radius);
            var backColor = pen.Color;
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = pen.Width,
                Color = pen.Color,
                IsAntialias = true,
            };
            canvas.DrawRoundRect(rect, skPaint);
        }
        public static void DrawRoundedRectangle(SKCanvas canvas, Pen p, float x, float y, float width, float height, float radius)
        {
            if (radius <= 0)
                DrawRectangle(canvas, p, x, y, width, height);

            var ssRect = new RectangleF(x, y, width, height);
            var sRect = new SKRect(ssRect.Left, ssRect.Top, ssRect.Right, ssRect.Bottom);
            var rect = new SKRoundRect(sRect, radius);
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = p.Color,
                StrokeWidth = p.Width,
                IsAntialias = true,
            };
            canvas.DrawRoundRect(rect, skPaint);
        }
        public static void DrawRectangle(SKCanvas canvas, Pen p, float x, float y, float width, float height)
        {
            DrawRectangle(canvas, p, MakeRect(x, y, width, height));
        }
        public static void FillRectangle(SKCanvas canvas, SKColor backColor, float x, float y, float width, float height)
        {
            var sRect = MakeRect(x, y, width, height);
            var rect = new SKRect(sRect.Left, sRect.Top, sRect.Right, sRect.Bottom);
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = backColor,
                IsAntialias = true,
            };
            canvas.DrawRect(rect, skPaint);
        }
        public static void DrawString(SKCanvas canvas, DXString str, float x, float y, float rotation = 0)
        {
            if (str.Text == null || str.Text == "")
                return;
            DrawStringGLFW(canvas, str, x, y, rotation);
        }
        static void DrawStringGLFW(SKCanvas canvas, DXString str, float x, float y, float rotation)
        {
            if (string.IsNullOrEmpty(str.Text))
                return;

            canvas.Save();
            canvas.Translate(x, y);
            canvas.RotateDegrees(rotation);
            if (
                (str.Text.StartsWith("$") && str.Text.EndsWith("$") && str.Text.Length >= 2)
                 || str.Text.StartsWith("$$"))// latex
            {
                DrawLatexString(canvas, str.Text, str.DXFont.Size, str.Color, 0, 0);
            }
            else
            {
                var textPaint = new SKPaint(str.DXFont)
                {
                    Color = str.Color,
                    IsAntialias = true
                };
                var lines = str.Text.Replace("\r", "").Split("\n");
                int li = 0;
                foreach (var line in lines)
                {
                    //Canvas.DrawText(line, 0, str.DXFont.Height * li + str.DXFont.Height * 5 / 6, textPaint);
                    canvas.DrawText(line, 0, str.DXFont.Size * li + str.DXFont.Size * 5 / 6, textPaint);
                    li++;
                }
            }
            canvas.RotateDegrees(-rotation);
            canvas.Translate(-x, -y);
        }
        public static void DrawLatexString(SKCanvas canvas, string str, float height, SKColor color, float x, float y)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var painter = new CSharpMath.SkiaSharp.MathPainter();
            painter.FontSize = height * 0.8F;
            painter.LaTeX = str;
            if (str.StartsWith("$") && str.EndsWith("$") && str.Length >= 2)
                painter.LaTeX = str.Substring(1, str.Length - 2);
            else if (str.StartsWith("$$"))
                painter.LaTeX = str.Substring(2, str.Length - 2);
            var stream = painter.DrawAsStream(format: SKEncodedImageFormat.Png);
            var img = SKImage.FromEncodedData(stream);
            var paint = new SKPaint()
            {

                Color = color
            };
            canvas.DrawImage(img, x, y, paint);
        }


        public static void DrawRectangle(SKCanvas canvas, Pen p, SKRect drawingRect)
        {
            var rect = new SKRect(drawingRect.Left, drawingRect.Top, drawingRect.Right, drawingRect.Bottom);
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = p.Color,
                StrokeWidth = p.Width,
                IsAntialias = true,
            };
            canvas.DrawRect(rect, skPaint);
        }
        public static void DrawLines(SKCanvas canvas, Pen p, SKPoint[] list)
        {
            if (list.Length < 2)
                return;
            if (list.Length == 2)
            {
                DrawLine(canvas, p, list[0], list[1]);
                return;
            }

            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = p.Color,
                StrokeWidth = p.Width,
                IsAntialias = true,
            };
            SKPath path = new SKPath();
            path.MoveTo(list[0].X, list[0].Y);

            for (int i = 1; i < list.Length; i++)
                path.LineTo(list[i].X, list[i].Y);
            canvas.DrawPath(path, skPaint);
        }
        public static void DrawLine(SKCanvas canvas, Pen p, SKPoint p1, SKPoint p2)
        {
            DrawLine(canvas, p, p1.X, p1.Y, p2.X, p2.Y);
        }
        public static void DrawLine(SKCanvas canvas, Pen p, float x1, float y1, float x2, float y2)
        {
            SKPaint skPaint = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                Color = p.Color,
                StrokeWidth = p.Width,
                IsAntialias = true,
            };
            canvas.DrawLine(new SKPoint(x1, y1), new SKPoint(x2, y2), skPaint);
        }
    }

    public static class GraphicsUtils
    {
        public static SKColor GetContrast(this SKColor source, bool preserveOpacity = true)
        {
            SKColor inputColor = source;
            //if RGB values are close to each other by a diff less than 10%, then if RGB values are lighter side, decrease the blue by 50% (eventually it will increase in conversion below), if RBB values are on darker side, decrease yellow by about 50% (it will increase in conversion)
            byte avgColorValue = (byte)((source.Red + source.Green + source.Blue) / 3);
            if (avgColorValue < 110 || avgColorValue > 144) //The color is a shade of gray
            {
                if (avgColorValue < 123) //color is dark
                {
                    inputColor = new SKColor(221, 57, 138);
                }
                else
                {
                    inputColor = new SKColor(34, 198, 117);
                }
            }
            byte sourceAlphaValue = source.Alpha;
            if (!preserveOpacity)
            {
                sourceAlphaValue = Math.Max(source.Alpha, (byte)127); //We don't want contrast color to be more than 50% transparent ever.
            }
            RGB rgb = new RGB { R = inputColor.Red, G = inputColor.Green, B = inputColor.Blue };
            HSB hsb = ConvertToHSB(rgb);
            hsb.H = hsb.H < 180 ? hsb.H + 180 : hsb.H - 180;
            //hsb.B = isColorDark ? 240 : 50; //Added to create dark on light, and light on dark
            rgb = ConvertToRGB(hsb);
            return new SKColor(sourceAlphaValue, (byte)rgb.R, (byte)rgb.G, (byte)rgb.B); ;
        }
        internal static RGB ConvertToRGB(HSB hsb)
        {
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double chroma = hsb.S * hsb.B;
            double hue2 = hsb.H / 60;
            double x = chroma * (1 - Math.Abs(hue2 % 2 - 1));
            double r1 = 0d;
            double g1 = 0d;
            double b1 = 0d;
            if (hue2 >= 0 && hue2 < 1)
            {
                r1 = chroma;
                g1 = x;
            }
            else if (hue2 >= 1 && hue2 < 2)
            {
                r1 = x;
                g1 = chroma;
            }
            else if (hue2 >= 2 && hue2 < 3)
            {
                g1 = chroma;
                b1 = x;
            }
            else if (hue2 >= 3 && hue2 < 4)
            {
                g1 = x;
                b1 = chroma;
            }
            else if (hue2 >= 4 && hue2 < 5)
            {
                r1 = x;
                b1 = chroma;
            }
            else if (hue2 >= 5 && hue2 <= 6)
            {
                r1 = chroma;
                b1 = x;
            }
            double m = hsb.B - chroma;
            return new RGB()
            {
                R = r1 + m,
                G = g1 + m,
                B = b1 + m
            };
        }
        internal static HSB ConvertToHSB(RGB rgb)
        {
            // By: <a href="http://blogs.msdn.com/b/codefx/archive/2012/02/09/create-a-color-picker-for-windows-phone.aspx" title="MSDN" target="_blank">Yi-Lun Luo</a>
            double r = rgb.R;
            double g = rgb.G;
            double b = rgb.B;

            double max = Max(r, g, b);
            double min = Min(r, g, b);
            double chroma = max - min;
            double hue2 = 0d;
            if (chroma != 0)
            {
                if (max == r)
                {
                    hue2 = (g - b) / chroma;
                }
                else if (max == g)
                {
                    hue2 = (b - r) / chroma + 2;
                }
                else
                {
                    hue2 = (r - g) / chroma + 4;
                }
            }
            double hue = hue2 * 60;
            if (hue < 0)
            {
                hue += 360;
            }
            double brightness = max;
            double saturation = 0;
            if (chroma != 0)
            {
                saturation = chroma / brightness;
            }
            return new HSB()
            {
                H = hue,
                S = saturation,
                B = brightness
            };
        }
        private static double Max(double d1, double d2, double d3)
        {
            if (d1 > d2)
            {
                return Math.Max(d1, d3);
            }
            return Math.Max(d2, d3);
        }
        private static double Min(double d1, double d2, double d3)
        {
            if (d1 < d2)
            {
                return Math.Min(d1, d3);
            }
            return Math.Min(d2, d3);
        }
        internal struct RGB
        {
            internal double R;
            internal double G;
            internal double B;
        }
        internal struct HSB
        {
            internal double H;
            internal double S;
            internal double B;
        }
        public static SKColor[] CommonBrightColors()
        {
            return new UInt32[] {
                0xF2D7D5,
                0xEBDEF0,
                0xD4E6F1,
                0xD1F2EB,
                0xFCF3CF,
                0xFAE5D3,
                0xEAEDED,
                0xD6DBDF,
                0xB2EBF2,
                0xC5CAE9,
                0xFFF3E0,
                0xD5F5E3,
                0xD7CCC8,
                0xE9F7EF,
                0xFADBD8,
                0xFBE9E7,
                0xD7BDE2,
                0xECEFF1,
                0xEBF5FB,
                0xD6DBDF,
 }.Select(v => new SKColor((byte)(v % 256), (byte)((v >> 8) % 256), (byte)((v >> 16) % 256))).ToArray();
        }
        public static SKColor[] CommonColors()
        {
            return new UInt32[] {
                0xB71C1C,
                0x4A148C,
                0x880E4F,
                0x1A237E,
                0x006064,
                0x0D47A1,
                0x1B5E20,
                0x827717,
                0xFF6F00,
                0xBF360C,
                0x3E2723,
                0x212121,
                0x37474F,
                0xFDD835,
                0x4CAF50,
                0x0033FF,
                0xFF00FF,
                0x336699,
                0xFF0033,
                0x9C27B0,
                }.Select(v => new SKColor((byte)(v % 256), (byte)((v >> 8) % 256), (byte)((v >> 16) % 256))).ToArray();
        }
    }
}
