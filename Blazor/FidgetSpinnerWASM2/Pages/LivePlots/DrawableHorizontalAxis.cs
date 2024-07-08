using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
        public class DrawableHorizontalAxis : DrawableAxis
    {
        public DataSeries DataSeries { get; set; }
        public DrawableHorizontalAxis () : base()
        {
            Title = "x axis";
            //MinimumLimitImage = new RectangularPatch("max", themedResources, new VisualState()
            //{
            //    Left = AxisBounds.Right - 25,
            //    Top = AxisBounds.Bottom - 15,
            //    Width = 25,
            //    Height = 15,
            //    Image = themedResources.GetDXImage("leftlimit")
            //})
            //{
            //    Cursor = Cursors.Hand,
            //};
            //MinimumLimitImage.OnClick += (s, e) => { OffsetG = };
        }
        public List<XYPlot> Containers = new List<XYPlot>();
        public override MoveOp DefaultResetOp
        {
            get
            {
                return MoveOp.goToXZero;
            }
        }
        public override MoveOp DefaultScaleOp
        {
            get
            {
                return MoveOp.xZoom;
            }
        }

        public void NotifyAddedToContainer(XYPlot container)
        {
            if (!Containers.Contains(container))
                Containers.Add(container);
            //if (MinimumLimitImage == null)
            //{
            //    var minImg = ThemedDXImage.Create(ThemedResources, "leftlimit");
            //    MinimumLimitImage = new RectangularPatch("minimum limit", ThemedResources, new VisualState()
            //    {
            //        Left = 5,
            //        Top = AxisBounds.Y + 5,
            //        Width = minImg.Width - 4,
            //        Height = minImg.Height - 4,
            //        Image = minImg
            //    });
            //}
        }
        public void NotifyRemovedFromContainer(XYPlot container)
        {
            if (Containers.Contains(container))
                Containers.Remove(container);
        }
        public override void ScaleChanged(SKPoint latestPoint, SKPoint lastPoint)
        {
            float changeG = latestPoint.X - lastPoint.X;
            float totalShownV = AxisBounds.Width / PPU;
            float changeV = -(changeG) / PPU;
            float newTotalV = totalShownV + changeV;
            if (newTotalV < 0)
                return;
            PPU = AxisBounds.Width / newTotalV;
            OffsetG = GAtMouseDown - VAtMouseDown * PPU;
            NeedsRefresh = true;
        }
        public override void OffsetChanged(SKPoint latestPoint, SKPoint lastPoint)
        {
            OffsetG += (latestPoint.X - lastPoint.X);
        }
        void drawCursorMarker(SKCanvas g, SKFont f, SKColor BackColor)
        {

            if (float.IsNaN(MarkerCursorG.X))
                return;
            var CursorV = GtoV(MarkerCursorG.X);
            DXString num = new DXString { Color = textColor, DXFont = f, Text = PhysLoggerSharedResources.FormatNumber(CursorV) };
            var strSz = SKGraphics.MeasureString(num);
            SKGraphics.FillRectangle(g, BackColor, MarkerCursorG.X - strSz.Width / 2, PlotBounds.Height + 1, strSz.Width, strSz.Height + 5);
            
            SKGraphics.DrawString(g, 
                num,
                MarkerCursorG.X - strSz.Width / 2,
                AxisBounds.Top + 5);

            SKGraphics.DrawLine(g, new Pen(textColor, 1), MarkerCursorG.X, PlotBounds.Top, MarkerCursorG.X, AxisBounds.Top);

        }
        void drawValueMarker(FloatingMarker marker, SKCanvas g)
        {
            if (marker == null)
                return;
            if (!marker.Active)
                return;
            var G = VtoG(marker.Value);
            if (G >= 0 && G < PlotBounds.Width)
                SKGraphics.DrawLine(g, new Pen(marker.VisualState.BorderColor, 2), G, PlotBounds.Top, G, AxisBounds.Top + 8);
        }
        public override void DrawMarkers(SKCanvas g, SKFont f, SKColor BackColor)
        {
            drawCursorMarker(g, f, BackColor);
        }
        public override void SizeChanged(SKRect horizontalAxisBoundsG, SKRect verticalAxisBoundsG, SKRect plotBounds)
        {
            base.SizeChanged(horizontalAxisBoundsG, verticalAxisBoundsG, PlotBounds);
            AxisBounds = horizontalAxisBoundsG;
            PlotBounds = plotBounds;
        }
        protected override void ScrollWithSizeChange(SKSize change)
        {
            if (DefaultScalingMode == AutoscalingMode.Autoscroll)
                OffsetG += change.Width;
        }
        public override void resetToZero()
        {
            OffsetG = PlotBounds.Width;
            totalScrollV = 0;
        }
        public override void AutoSetScale()
        {
            if (AutoScale)
                _AutoSetScale(PlotBounds.Width);
        }

        double minX = double.NaN;
        double maxX = double.NaN;
        SKFont tickFont;
        public override void DrawAxisAndGrid(SKCanvas g, SKFont _tickFont)
        {
            tickFont = _tickFont;
            AxisBounds = SKGraphics.MakeRect(PlotBounds.Left, AxisBounds.Top, PlotBounds.Width, AxisBounds.Height); 
            float xs = PPU;
            float xog = OffsetG;
            // X Axis
            var axisP = new Pen(axisColor, 1.5F);
            var majLine = new Pen(axisMajorLines, 1F);
            var minLine = new Pen(axisMinorLines, 1F);

            double unitX = 1 / Math.Pow(10, PhysLoggerSharedResources.SignificantFiguresAfterDecimal);
            double multF = 5;
            // determine scale first
            float testValue = 123.123456F;
            PhysLoggerSharedResources.FormatNumber(testValue);
            var testS = new DXString
            {
                Text = PhysLoggerSharedResources.FormatNumber(testValue),
                DXFont = tickFont
            };
            var testSz = SKGraphics.MeasureString(testS);
            while (unitX * xs < testSz.Width * 1.5F)
            {
                unitX *= multF;
                multF = multF == 2 ? 5 : 2;
            }

            if (double.IsNaN(minX))
                minX = 0;
            if (double.IsNaN(maxX))
                maxX = 0;
            while (minX * xs < -xog)
            {
                if (double.IsPositiveInfinity(minX + unitX))
                { minX = float.MaxValue; break; }
                minX += unitX;
            }
            while (minX * xs > -xog)
            {
                if (double.IsNegativeInfinity(minX - unitX))
                { minX = double.MinValue; break; }
                minX -= unitX;
            }

            while (maxX * xs > PlotBounds.Width - xog)
            {
                if (double.IsNegativeInfinity(maxX - unitX))
                { minX = double.MinValue; break; }
                maxX -= unitX;
            }
            while (maxX * xs < PlotBounds.Width - xog)
            {
                if (double.IsPositiveInfinity(maxX + unitX))
                { maxX = float.MaxValue; break; }
                maxX += unitX;
            }



            float xaHei = (tickFont.Size * 15 / 10);
            bool isMinLine = false;

            var xSigFiguresAfterD = 0;
            var totalFigs = (unitX / 2 - Math.Floor(unitX / 2)).ToString().Length - 2;

            while (Math.Round(unitX, xSigFiguresAfterD) == Math.Round(unitX / 2, xSigFiguresAfterD)
                && xSigFiguresAfterD <= totalFigs)
                xSigFiguresAfterD++;

            for (double i = minX; i <= maxX; i += unitX / 2)
            {
                //SKPoint drawableMid = VtoG(new SKPoint(i, 0), xog / xs, xs, 1, 0);
                //drawableMid = new SKPoint(drawableMid.X, h);

                SKPoint drawable1 = new SKPoint((float)i * xs + xog + PlotBounds.Left, PlotBounds.Top);
                SKPoint drawable2 = new SKPoint((float)i * xs + xog + PlotBounds.Left, PlotBounds.Top + PlotBounds.Height);
                //if (grid)
                //drawable1 = new SKPoint(drawable1.X, 0);
                //if (grid)
                //drawable2 = new SKPoint(drawable2.X, h - xaHei);
                var s = new DXString
                {
                    Text = PhysLoggerSharedResources.FormatNumber((float)i),
                    Color = SKColors.Gray,
                    DXFont = tickFont
                };
                var xyo = SKGraphics.MeasureString(s);
                SKPoint drawableStrPos = new SKPoint(drawable2.X - xyo.Width / 2, AxisBounds.Top + 8);
                if (!isMinLine)
                {
                    drawable2 = new SKPoint((float)i * xs + xog, PlotBounds.Top + PlotBounds.Height + 5);
                    if (drawable1.X < PlotBounds.Width && drawable1.X >= 0)
                    {
                        SKGraphics.DrawLine(g, majLine, drawable1, drawable2);                        
                        SKGraphics.DrawString(g, s, drawableStrPos.X, drawableStrPos.Y);
                    }
                }
                else
                {
                    if (drawable1.X < PlotBounds.Width && drawable1.X > 0)
                    {
                        SKGraphics.DrawLine(g, minLine, drawable1, drawable2);
                    }
                }
                isMinLine = !isMinLine;
            }
            if (xog < PlotBounds.Width && xog > 0)
                SKGraphics.DrawLine(g, axisP, xog, 0, xog, PlotBounds.Height);

            //g.DrawLine(axisP, AxisBounds.X, AxisBounds.Y, AxisBounds.X + AxisBounds.Width, AxisBounds.Y);

            //// axis labels are buttons now. Dont draw their strings
            //var unitStr = new DXString(ThemedResources) { Text = Unit, Color = Color.Black, DXFont = tickFont };
            //var unitSize = g.MeasureString(unitStr);
            //g.DrawString(unitStr, AxisBounds.X + AxisBounds.Width / 2 - unitSize.Width / 2, AxisBounds.Y + tickFont.Height * 0.9F);
        }

        internal void ExpandToMaximum()
        {
            RequestToRemoveHorizontalAveraging = true;
        }
        internal void ScrollToEnd()
        {
            OffsetV = -MaxDisplayedValue;
            OffsetG += PlotBounds.Width - 100;
            NeedsRefresh = true;
        }

        public override void goToZero()
        {
            OffsetG = -PlotBounds.Width;
            NeedsRefresh = true;
        }
        public override float VtoG(float v, float offsetG, float ppu)
        {
            return v * ppu + offsetG;
        }
        public override float GtoV(float vG, float offsetG, float ppu)
        {
            return (vG - offsetG) / ppu;
        }
        public override void RegisterMouseDown(SKPoint eG)
        {
            GAtMouseDown = eG.X;
            VAtMouseDown = GtoV(eG.X);
        }
        public event EventHandler OnAxisUnitsChanged;
        public override RectangularPatch MakeAxisButton(XYPlot aContainer)
        {
            int bw = 200, bh = 20;
            var axisButton = new RectangularPatch(
                "x axis button",
                new VisualState()
                {
                    Left = AxisBounds.Width / 2 - bw / 2,
                    Top = aContainer.Height - aContainer.SpaceDivision_XAxisButtonSpace,
                    Width = bw,
                    Height = bh,
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
    }
}
