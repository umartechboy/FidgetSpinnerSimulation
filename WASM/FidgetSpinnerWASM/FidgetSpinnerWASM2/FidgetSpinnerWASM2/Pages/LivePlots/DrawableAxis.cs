using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class DrawableAxis
    {
        // Graphics
        // Note: this element doesn't get to know the exact locations on the container. The container reports and expects positions in axis' local FOR.
        //public DXControl MinimumLimitImage;
        //public DXControl MaximumLimitImage;
        public MoveOp CurrentMoveOp = MoveOp.None;
        public MoveOp TentativeOp = MoveOp.None;
        public string Title { get; set; }
        public int DebugID { get; set; } = 0;

        // used by the overlay objects for a continuous scaling after mouse has been pressed once.
        public bool InScale = false, AutoScaleRequested = false;
        /// <summary>
        /// The bounds of the drawing area
        /// </summary>
        public SKRect AxisBounds = SKGraphics.MakeRect(0, 100, 200, 10);
        public SKRect PlotBounds = SKGraphics.MakeRect(0, 0, 200, 100);
        public UnitCollection UnitOptions { get; set; } = UnitCollection.Create(UnitCollection.UnitTypesEnum.Unitless);
        public float PPU { get; set; } = 1;
        public float OffsetG { get; set; } = 0;
        public float OffsetV { get { return OffsetG / PPU; } set { OffsetG = value * PPU; } }

        public bool NeedsRefresh { get; set; } = true;
        public void SetAutoScrollSource(DataSeries ds)
        {
            ds.SubscribeAutoScroll(this);
            DefaultScalingMode = AutoscalingMode.Autoscroll;
        }
        public bool RequestToRemoveHorizontalAveraging { get; set; } = false;
        public void RemoveAutoScrolling()
        {
            DefaultScalingMode = AutoscalingMode.Autoscale;
            AutoScale = true;
        }
        // called by the series
        protected double totalScrollV = 0;
        UInt32 lastScrollID = 0;
        public void NotifyScroll(float dv, UInt32 scrollID)
        {
            if (lastScrollID == scrollID)
                return;
            lastScrollID = scrollID;
            totalScrollV += dv;
        }

        //UI                          
        protected float GAtMouseDown = 0;
        protected float VAtMouseDown = 0;
        /// <summary>
        /// called by the axis containers. The children must override it.
        /// </summary>
        public virtual void ScaleChanged(SKPoint latestPoint, SKPoint lastPoint)
        { throw new NotImplementedException(); }
        /// <summary>
        /// called by the axis containers. The children must override it.
        /// </summary>
        public virtual void OffsetChanged(SKPoint latestPoint, SKPoint lastPoint)
        { throw new NotImplementedException(); }
        public virtual MoveOp DefaultResetOp { get { return MoveOp.None; } }
        public virtual MoveOp DefaultScaleOp { get { return MoveOp.None; } }


        public AutoscalingMode DefaultScalingMode { get; set; } = AutoscalingMode.Autoscale;

        //Data                                       
        bool _ascale = true;
        public bool AutoScale
        {
            get { return _ascale; }
            set
            {
                _ascale = value;
                if (value) AutoSetScale(); NeedsRefresh = true;
            }
        }
        public virtual float VtoG(float v, float offsetG, float ppu)
        {
            throw new NotImplementedException();
        }
        public float VtoG(float v)
        {
            return VtoG(v, OffsetG, PPU);
        }
        public virtual float GtoV(float vG, float offsetG, float ppu)
        {
            throw new NotImplementedException();
        }
        public float GtoV(float vG)
        {
            return GtoV(vG, OffsetG, PPU);
        }
        public virtual void AutoSetScale()
        { throw new NotImplementedException(); }
        public bool IsDoingPixelAveraging { get; set; } = false;
        public float MinDisplayedValue { get; set; } = 0;
        public float MaxDisplayedValue { get; set; } = 0;
        public bool HasOvershoot { get; set; } = false;
        public bool HasUndershoot { get; set; } = false;
        protected List<float> AverageMax = new List<float>();
        protected List<float> AverageMin = new List<float>();
        float maxDisplayedValue_Avg = 0, minDisplayedValue_Avg = 0;
        async Task AutoScaleLoop()
        {
            while (true)
            {
                await Task.Delay(30);
                if (!float.IsNaN(MaxDisplayedValue) && !float.IsInfinity(MaxDisplayedValue))
                {
                    if (MaxDisplayedValue > float.MinValue / 2)
                    {
                        AverageMax.Add(MaxDisplayedValue);
                        if (AverageMax.Count > 30)
                            AverageMax.RemoveAt(0);
                    }
                }
                if (!float.IsNaN(MinDisplayedValue) && !float.IsInfinity(MinDisplayedValue))
                {
                    if (MinDisplayedValue < float.MaxValue / 2)
                    {
                        AverageMin.Add(MinDisplayedValue);
                        if (AverageMin.Count > 30)
                            AverageMin.RemoveAt(0);
                    }
                    else
                    { }
                }
                if (AverageMax.Count > 0)
                    maxDisplayedValue_Avg = AverageMax.Average();
                if (AverageMin.Count > 0)
                    minDisplayedValue_Avg = AverageMin.Average();
                // smooth auto scroll
                if (DefaultScalingMode == AutoscalingMode.Autoscroll)
                {
                    if (AutoScale)
                    {
                        float thisV = 0.1F * (float)totalScrollV;
                        if (thisV > 1e-30)
                        {
                            OffsetV -= thisV;
                            totalScrollV -= thisV;
                            NeedsRefresh = true;
                        }
                    }
                }
            }
        }
        protected void _AutoSetScale(float maxWidthG)
        {
            if (this is DrawableVerticalAxisRight)
            {
                //if (((DrawableVerticalAxisRight)this).DataSeries[0].BindingQuantity.Title.Contains("Flux"))
                //{
                //    if (maxDisplayedValue_Avg < 5 || minDisplayedValue_Avg < 5)
                //        ;

                //}
            }
            if (DefaultScalingMode != AutoscalingMode.Autoscale)
                return;
            if (AverageMax.Count == 0)
                return;
            if (AverageMin.Count == 0)
                return;
            float currentMax = maxDisplayedValue_Avg;
            float currentMin = minDisplayedValue_Avg;
            if (float.IsInfinity(currentMin) || float.IsInfinity(currentMax))
                return;
            if (currentMin == currentMax)
            {
                float inc = currentMin * 0.1F;
                if (inc == 0)
                    inc = 0.001F;
                currentMin -= inc;
                currentMax += inc;
            }
            float fac = 0.9F;
            float cover = 0.4F;
            float idealXPPU = maxWidthG / (float)(currentMax - currentMin) * cover;
            if (idealXPPU > Math.Pow(10, PhysLoggerSharedResources.SignificantFiguresAfterDecimal) * 5000)
                idealXPPU = (float)Math.Pow(10, PhysLoggerSharedResources.SignificantFiguresAfterDecimal) * 5000;
            if (currentMax - currentMin > 0)
                PPU = PPU * fac + idealXPPU * (1 - fac);

            fac = 0.9F;
            float idealOffsetG = -(currentMax + currentMin) / 2 * idealXPPU + maxWidthG / 2;
            OffsetG = OffsetG * fac + idealOffsetG * (1 - fac);
        }
        //float LabelMaxHeight = 0;

        // Default constructor
        public SKColor axisColor, axisMajorLines, axisMinorLines, backColor, textColor;
        public DrawableAxis()
        {
            axisColor = SKColors.Black;
            axisMajorLines = SKColors.DarkGray;
            axisMinorLines = SKColors.LightGray;
            backColor = SKColors.White;
            textColor = SKColors.Black;
            new Task(async () => await AutoScaleLoop()).Start();
        }

        public virtual void goToZero()
        {
            throw new NotImplementedException();
        }

        public virtual void resetToZero()
        {
            throw new NotImplementedException();
        }
        public void DrawLimits(SKCanvas g)
        {
            InScale = true;
            if (HasUndershoot)
            {
                //MinimumLimitImage.Draw(g, TentativeOp == MoveOp.resetScale);
                InScale = false;
            }
            if (HasOvershoot)
            {
                //MaximumLimitImage.Draw(g, TentativeOp == MoveOp.resetScale);
                InScale = false;
            }
            if (!InScale && AutoScaleRequested)
                AutoSetScale();
        }
        public virtual bool CheckOverShootImageHover(SKPoint cursorG)
        {
            return false;
            throw new NotImplementedException();
        }
        SKSize lastSize = new SKSize(0, 0);
        public SKPoint MarkerCursorG = new SKPoint();
        protected virtual void ScrollWithSizeChange(SKSize change)
        { throw new NotImplementedException("Aren't we using Left or Right vertical axis?"); }
        public virtual void SizeChanged(SKRect horizontalAxisBoundsG, SKRect verticalAxisBoundsG, SKRect plotBoundsG)
        {
            PlotBounds = plotBoundsG;
            ScrollWithSizeChange(new SKSize(plotBoundsG.Width - lastSize.Width, plotBoundsG.Height - lastSize.Height));
            lastSize = new SKSize(plotBoundsG.Size.Width, plotBoundsG.Height);
            //if (!DontScrollPlotOnReSize)
            //    resetToZero();
            NeedsRefresh = true;
        }
        public virtual void RegisterMouseDown(SKPoint eG)
        { throw new NotImplementedException(); }

        public virtual void DrawMarkers(SKCanvas g, SKFont f, SKColor BackColor)
        { throw new NotImplementedException(); }
        public virtual void DrawAxisAndGrid(SKCanvas g, SKFont f)
        {
            throw new NotImplementedException();
        }
        public virtual RectangularPatch MakeAxisButton(XYPlot aContainer)
        { throw new NotImplementedException(); }
        public override string ToString()
        {
            return UnitOptions.Type + " (" + UnitOptions.Selected.Symbol + ")";
        }
    }
    public class FloatingMarker : RectangularPatch
    {
        public FloatingMarker(string name, VisualState vs) : base(name, vs)
        {
            Active = false; // hide
        }
        float v = 0;
        public float Value
        {
            get { return Active ? v : 0; }
            set
            {
                v = value;
            }
        }
        bool ac = false;
        public bool Active
        {
            get
            {
                return ac;
            }
            internal set
            {
                ac = value;
                this.VisualState.Opacity = value ? 1 : 0;
            }
        }
    }
    public class XYPlotEventArgs : EventArgs
    {
        public LoggerTerminalQuantity Quantity { get; set; }
        public int PlotIndex { get; set; } = -1;
        public int SeriesIndex { get; internal set; } = -1;
    }
    public delegate void XYPlotEventHandler(object sender, XYPlotEventArgs e);
}
