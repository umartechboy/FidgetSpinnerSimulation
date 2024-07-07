using Microsoft.AspNetCore.Components.Web;
using SkiaSharp;
using System.Drawing;
using System.Text;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class RectangularPatch
    {
        public string Name { get; set; }
        public VisualState VisualState { get; set; }
        public RectangularPatch(string name, VisualState st) 
        {
            Name = name;
            VisualState = st;
        }
        public float Height { get => VisualState.Height; set => VisualState.Height = value; }
        public float Width { get => VisualState.Width; set => VisualState.Width = value; }
        public float Bottom { get => VisualState.Top + VisualState.Height; }
        public float Left { get => VisualState.Left; set => VisualState.Left = value; }
        public float Top { get => VisualState.Top; set => VisualState.Top = value; }
        public float Right { get => VisualState.Left + VisualState.Width; }
        public SKRect Bounds { get => SKGraphics.MakeRect(Left, Top, Width, Height); }
        public List<RectangularPatch> Controls { get; set; } = new();
        public virtual void OnPaint(SKCanvas g, StringBuilder debugS)
        {

        }

        public event EventHandler OnAdded;
        public delegate void MouseEventHandler(SKPoint e);
        public event MouseEventHandler OnMouseMove;
        public event MouseEventHandler OnMouseEnter;
        public event MouseEventHandler OnMouseLeave;
        public event MouseEventHandler OnClick;
        public event MouseEventHandler OnMouseWheel;
        public event MouseEventHandler OnMouseDown;
        public event MouseEventHandler OnMouseUp;
        public virtual bool ProcessMouseMove(SKPoint position)
        {
            foreach(var control in Controls)
            {
                var pos2 = new SKPoint(position.X - control.Left, position.Y - control.Top);
                if (control.ProcessMouseMove(pos2))
                    return true;
            }
            if (Bounds.Contains(position))
            {
                OnMouseMove?.Invoke(position);
                return true;
            }
            return false;
        }
        public virtual bool ProcessMouseClick(SKPoint position)
        {
            foreach (var control in Controls)
            {
                var pos2 = new SKPoint(position.X - control.Left, position.Y - control.Top);
                if (control.ProcessMouseClick(pos2))
                    return true;
            }
            if (Bounds.Contains(position))
            {
                OnClick?.Invoke(position);
                return true;
            }
            return false;
        }
        public virtual bool ProcessMouseDown(SKPoint position)
        {
            foreach (var control in Controls)
            {
                var pos2 = new SKPoint(position.X - control.Left, position.Y - control.Top);
                if (control.ProcessMouseDown(pos2))
                    return true;
            }
            if (Bounds.Contains(position))
            {
                OnMouseDown?.Invoke(position);
                return true;
            }
            return false;
        }


        public virtual bool ProcessMouseUp(SKPoint position)
        {
            foreach (var control in Controls)
            {
                var pos2 = new SKPoint(position.X - control.Left, position.Y - control.Top);
                if (control.ProcessMouseUp(pos2))
                    return true;
            }
            if (Bounds.Contains(position))
            {
                OnMouseUp?.Invoke(position);
                return true;
            }
            return false;
        }
    }
    public class VisualState
    {
        public VisualState()
        {
            
        }
        public VisualState(float left, float top, float width, float height, float opacity)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Opacity = opacity;
        }

        public float Left { get; set; } = 0;
        public float Top {get;set;} = 0;
        public float Width {get;set;} = 100;
        public float Height {get;set;} = 25;
        public float Rotation {get;set;} = 0;
        public float Radius {get;set;} = 0;
        public float Opacity {get;set;} = 1;
        public SKColor BorderColor {get;set;}
        public float BorderThickness {get;set;} = 2;
        public SKColor FaceColor {get;set;}
        public DXString Text { get; set; } =new DXString();
    }
}
