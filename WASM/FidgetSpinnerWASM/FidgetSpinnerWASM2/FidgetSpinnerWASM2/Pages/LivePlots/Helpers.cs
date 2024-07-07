using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public enum AutoscalingMode
    {
        Autoscale,
        Autoscroll
    }
    public enum MoveOp
    {
        None,
        xZoom,
        yZoom,
        xyPan,
        Zoom,
        resetScale,
        goToXZero,
        ValueUpDown,
        ValueUpDownFine,
        selectSeries

    }
    public class Pen
    {
        public Pen() { }
        public Pen(SKColor color, float width) 
        {
            this.Color = color;
            this.Width = width;
        }
        public SKColor Color { get; set; }
        public float Width { get; set; }
    }
}

