
using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class DrawableVerticalAxis : DrawableAxis
    {
        public List<DataSeries> DataSeries { get; set; } = new List<DataSeries>();
        public RectangularPatch AxisButton = null;
        public DrawableVerticalAxis()
        { Title = "y axis"; }
    }
    public class DrawableVerticalAxisRight : DrawableVerticalAxis
    {
        public DrawableVerticalAxisRight() : base() { }
        public override MoveOp DefaultResetOp
        {
            get
            {
                return MoveOp.resetScale;
            }
        }
        public override MoveOp DefaultScaleOp
        {
            get
            {
                return MoveOp.yZoom;
            }
        }
        public override void ScaleChanged(SKPoint latestPoint, SKPoint lastPoint)
        {
            float changeG = -(latestPoint.Y - lastPoint.Y);
            float totalShownV = AxisBounds.Height / PPU;
            float changeV = -(changeG) / PPU;
            float newTotalV = totalShownV + changeV;
            if (newTotalV < 0)
                return;
            PPU = AxisBounds.Height / newTotalV;
            OffsetG = (AxisBounds.Height - GAtMouseDown) - VAtMouseDown * PPU;
            NeedsRefresh = true;
        }
        public override void OffsetChanged(SKPoint latestPoint, SKPoint lastPoint)
        {
            OffsetG += -(latestPoint.Y - lastPoint.Y);
        }
        void drawCursorMarker(SKCanvas g, SKFont f, SKColor BackColor)
        {
            if (float.IsNaN(MarkerCursorG.Y))
                return;
            var CursorV = GtoV(MarkerCursorG.Y);
            var num = new DXString { Text = PhysLoggerSharedResources.FormatNumber(CursorV), Color = textColor, DXFont = f };
            var strSz = SKGraphics.MeasureString(num);
            SKGraphics.FillRectangle(g, BackColor, AxisBounds.Left + 8, MarkerCursorG.Y - strSz.Height / 2, strSz.Width, strSz.Height);
            
            SKGraphics.DrawString(g, 
                num,
                AxisBounds.Left + 8,
                MarkerCursorG.Y - strSz.Height / 2);

            SKGraphics.DrawLine(g, new Pen(textColor, 1), PlotBounds.Left, MarkerCursorG.Y, AxisBounds.Left, MarkerCursorG.Y);
        }
        void drawValueMarker(FloatingMarker marker, SKCanvas g)
        {
            if (marker == null)
                return;
            if (!marker.Active)
                return;
            var G = VtoG(marker.Value);
            if (G >= 0 && G < PlotBounds.Width)
                SKGraphics.DrawLine(g, new Pen(marker.VisualState.BorderColor, 2), PlotBounds.Left, G, AxisBounds.Left + 8, G);
        }
        public override void DrawMarkers(SKCanvas g, SKFont f, SKColor BackColor)
        {
            drawCursorMarker(g, f, BackColor);
        }

        protected override void ScrollWithSizeChange(SKSize change)
        {
            if (DefaultScalingMode == AutoscalingMode.Autoscroll)
                OffsetG -= change.Height;
        }
        public override void resetToZero()
        {
            OffsetG = PlotBounds.Height / 2;
        }
        public override void AutoSetScale()
        {
            if (AutoScale)
                _AutoSetScale(PlotBounds.Height);
        }
        double maxY = double.NaN;
        double minY = double.NaN;
        SKFont tickFont;
        public override void DrawAxisAndGrid(SKCanvas g, SKFont _tickFont)
        {
            tickFont = _tickFont;
            var st = DateTime.Now;
            AxisBounds = SKGraphics.MakeRect(AxisBounds.Left, PlotBounds.Top, AxisBounds.Width, PlotBounds.Height);
            float ys = PPU;
            float yog = OffsetG;

            // X Axis
            var axisP = new Pen(axisColor, 1.5F);
            var majLine = new Pen(axisMajorLines, 1F);
            var minLine = new Pen(axisMinorLines, 1F);

            // Y Axis
            double unitY = 1e-6;
            double multF = 5;
            multF = 5;
            int iterations = 0;
            // determine scale first
            while (unitY * ys < tickFont.Size * 1.5F)
            {
                if (double.IsNegativeInfinity(unitY * multF))
                { unitY = double.MinValue; break; }
                if (double.IsPositiveInfinity(unitY * multF))
                { unitY = double.MaxValue; break; }
                unitY *= multF;
                multF = multF == 2 ? 5 : 2;
                if (unitY < 1e-6)
                    break;
            }
            var bm0 = (DateTime.Now - st).TotalMilliseconds;
            //if (unitY < 1e-7 || unitY > 1e7)
            //    return drawingRect;

            if (double.IsNaN(minY))
                minY = (int)Math.Round(-yog / ys) * unitY;
            if (double.IsNaN(maxY))
                maxY = (int)Math.Round((AxisBounds.Height - yog) / ys) * unitY;
            while (minY * ys < -yog)
            {
                if (double.IsPositiveInfinity(minY + unitY))
                { minY = double.MaxValue; break; }
                minY += unitY;
            }
            var bm1 = (DateTime.Now - st).TotalMilliseconds;
            if (bm1 > 1)
                ;
            while (minY * ys + yog > 0)
            {
                if (double.IsNegativeInfinity(minY - unitY))
                { minY = double.MinValue; break; }
                minY -= unitY;
            }

            while (maxY * ys + yog > AxisBounds.Height)
            {
                if (double.IsNegativeInfinity(maxY - unitY))
                { minY = double.MinValue; break; }
                maxY -= unitY;
            }
            var bm2 = (DateTime.Now - st).TotalMilliseconds;

            if (bm2 > 1)
                ;
            while (maxY * ys + yog < AxisBounds.Height)
            {
                if (double.IsPositiveInfinity(maxY + unitY))
                { maxY = double.MaxValue; break; }
                maxY += unitY;
            }

            var bm3 = (DateTime.Now - st).TotalMilliseconds;

            if (bm3 > 1)
                ;
            bool isMinLine = false;
            var ySigFiguresAfterD = 0;
            var totalFigs = (unitY / 2 - Math.Floor(unitY / 2)).ToString().Length - 2;

            while (Math.Round(unitY, ySigFiguresAfterD) == Math.Round(unitY / 2, ySigFiguresAfterD)
                && ySigFiguresAfterD <= totalFigs)
                ySigFiguresAfterD++;
            for (double i = minY; i <= maxY; i += unitY / 2)
            {
                //SKPoint drawableMid = VtoG(new SKPoint(0, i), 1, 1, yog / ys, ys, PlotBounds.Height);
                //drawableMid = new SKPoint(PlotBounds.Width, drawableMid.Y);

                SKPoint drawable1 = new SKPoint(PlotBounds.Left, (float)PlotBounds.Height - (float)(PlotBounds.Top + i * ys + yog));
                SKPoint drawable2 = new SKPoint(PlotBounds.Left + PlotBounds.Width, (float)PlotBounds.Height - (float)(PlotBounds.Top + i * ys + yog));
                if (!isMinLine)
                {
                    drawable2 = new SKPoint(PlotBounds.Left + PlotBounds.Width + 5, (float)PlotBounds.Height - (float)(PlotBounds.Top + i * ys + yog));

                    var s = new DXString
                    {
                        Text = PhysLoggerSharedResources.FormatNumber((float)i),
                        Color = SKColors.Gray,
                        DXFont = tickFont
                    };
                    var xyo = SKGraphics.MeasureString(s);
                    SKPoint drawableStrPos = new SKPoint(AxisBounds.Left + 6, drawable2.Y - xyo.Height / 2);
                    if (drawable2.Y < PlotBounds.Top + PlotBounds.Height && drawable2.Y > PlotBounds.Top)
                    {
                        SKGraphics.DrawLine(g, majLine, drawable1, drawable2);                        
                        SKGraphics.DrawString(g, s, drawableStrPos.X, drawableStrPos.Y);

                    }
                }
                else
                {
                    if (drawable2.Y < PlotBounds.Height && drawable2.Y > 0)
                        SKGraphics.DrawLine(g, minLine, drawable1, drawable2);
                }
                isMinLine = !isMinLine;
            }

            // zero line
            if (yog < AxisBounds.Height && yog > 0)
                SKGraphics.DrawLine(g, axisP, PlotBounds.Left, PlotBounds.Top + PlotBounds.Height - yog, PlotBounds.Left + PlotBounds.Width, PlotBounds.Top + PlotBounds.Height - yog);

            var bm4 = (DateTime.Now - st).TotalMilliseconds;
            if (bm4 > 2)
                ;
            // draw border
            //g.DrawLine(axisP, AxisBounds.X, AxisBounds.Y, AxisBounds.X, AxisBounds.Y + AxisBounds.Height);

            // axis labels are buttons now. Dont draw their strings
            //var unitStr = new DXString(ThemedResources) { Text = Unit, Color = Color.Black, DXFont = Font };
            //var unitSize = g.MeasureString(unitStr);

            //g.DrawString(unitStr, AxisBounds.X + Font.Height * 2.5F, AxisBounds.Y + AxisBounds.Height/2 + unitSize.Width/2, -90);
        }

        public override void goToZero()
        {
            OffsetG = -PlotBounds.Height / 2;
            NeedsRefresh = true;
        }

        public override float VtoG(float v, float offsetG, float ppu)
        {
            return -(v * ppu + offsetG) + AxisBounds.Height;
        }
        public override float GtoV(float vG, float offsetG, float ppu)
        {
            return ((AxisBounds.Height - vG) - offsetG) / ppu;
        }
        public override void SizeChanged(SKRect horizontalAxisBoundsG, SKRect verticalAxisBoundsG, SKRect plotBounds)
        {
            base.SizeChanged(horizontalAxisBoundsG, verticalAxisBoundsG, PlotBounds);
            AxisBounds = verticalAxisBoundsG;
            PlotBounds = plotBounds;
        }
        public override void RegisterMouseDown(SKPoint eG)
        {
            GAtMouseDown = eG.Y;
            VAtMouseDown = GtoV(eG.Y);
        }
        public override RectangularPatch MakeAxisButton(XYPlot aContainer)
        {
            int bw = 150, bh = 20;
            var axisButton = new RectangularPatch(
                "y axis button",
                new VisualState()
                {
                    Left = AxisBounds.Left + AxisBounds.Width - bw / 2 + bh / 2,
                    Top = AxisBounds.Height / 2 - bh / 2,
                    Width = bw,
                    Height = bh,
                    Rotation = 90,
                    FaceColor = SKColors.White,
                    Text = new DXString
                    {
                        Color = SKColors.Black,
                        Text = Title
                    },
                    Radius = -1
                }
                 )
            ;
            return axisButton;
        }
        public event EventHandler OnAxisUnitsChanged;
        public void NotifyAxisUnitChanged()
        { OnAxisUnitsChanged?.Invoke(this, new EventArgs()); }
    }
}
