using Microsoft.AspNetCore.Components.Web;
using System.Text;
using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public partial class XYPlot : RectangularPatch
    {
        public event XYPlotEventHandler OnYQuantityAdded;
        public event XYPlotEventHandler OnYQuantityRemoved;
        public event EventHandler OnStateChanged;
        public RectangularPatch MinimumXLimitButton;
        public RectangularPatch HorizontalAveragingFlag;
        public RectangularPatch MaximumXLimitButton;
        public RectangularPatch MinimumYLimitButton;
        public RectangularPatch MaximumYLimitButton;
        SKFont Font = new SKFont(SKTypeface.Default);
        #region Behaviour
        public SKPoint Cursor { get; set; }
        public string CursorStyle { get; set; }
        public DataSeries XSeries { get; set; }
        public DrawableHorizontalAxis XAxis { get; set; }
        public DrawableVerticalAxis YAxis { get; set; }
        List<DrawableAxis> XYAxis
        {
            get
            {
                var range = new List<DrawableAxis>();
                if (XAxis != null) range.Add(XAxis);
                if (YAxis != null) range.Add(YAxis);
                return range;
            }
        }
        int SpaceDivision_XAxisHeight = 30;
        int SpaceDivision_CommonAxisButtonHeight = 15;
        int SpaceDivision_CommonYAxisWidth = 120;
        public int SpaceDivision_XAxisButtonSpace = 20;
        public int SpaceDivision_YAxisButtonSpace = 40;
        public int SpaceDivision_LegendSpace = 200;

        bool _ntrd = true;
        public bool NeedsToRedraw
        {
            get
            {
                foreach (var axis in XYAxis)
                    if (axis.NeedsRefresh) return true;
                return false;
            }
            set
            {
                _ntrd = value;

                foreach (var axis in XYAxis)
                    axis.NeedsRefresh = value;
            }
        }
        RectangularPatch xAxisButton = new ("axis button", new ());
        SKColor axisColor;
        SKColor backColor;


        public XYPlot(string name) : this(name, 0, 0, 10, 10)
        {

        }
        public XYPlot(string name, float left, float top, float width, float height) : base(name, new VisualState(left, top, width, height, 1) { FaceColor = SKColors.White })
        {
            ResetAxisBounds();
            OnMouseMove += AxisContainer_OnMouseMove;
            OnMouseDown += AxisContainer_OnMouseDown;
            OnMouseUp += AxisContainer_OnMouseUp;
            OnAdded += AxisContainer_OnAdded;
            OnClick += AxisContainer_OnClick;
            axisColor = SKColors.Black;
            this.backColor = SKColors.White;


            ResetAxisBounds();
            // populate starting axis
            SetXAxis(new DrawableHorizontalAxis(), true);
            SetYAxis(new DrawableVerticalAxisRight());

            XAxis.PPU = 100;
            XAxis.OffsetG = XAxis.PlotBounds.Width;

            // Y Limit
            MinimumYLimitButton = new RectangularPatch("y min OOB", new VisualState()
            {
                Width = SpaceDivision_CommonYAxisWidth,
                Height = 20,
                Radius = 5,
                FaceColor = SKColors.Red,
                Text = new DXString() { Color = SKColors.Black }
            });
            Controls.Add(MinimumYLimitButton);

            MaximumYLimitButton = new RectangularPatch("y max OOB", new VisualState()
            {
                Width = SpaceDivision_CommonYAxisWidth,
                Height = 20,
                Radius = 5,
                FaceColor = SKColors.White,
                Text = new DXString() { Color = SKColors.Black }
            });
            Controls.Add(MaximumYLimitButton);

            // X Limit
            MinimumXLimitButton = new RectangularPatch("x min OOB", new VisualState()
            {
                Width = 100,
                Height = 20,
                Radius = 5,
                FaceColor = SKColors.White,
                Text = new DXString() { Color = SKColors.Black }
            });
            Controls.Add(MinimumXLimitButton);
            MaximumXLimitButton = new RectangularPatch("x max OOB", new VisualState()
            {
                Width = 100,
                Height = 20,
                Radius = 5,
                FaceColor = SKColors.White,
                Text = new DXString { Color = SKColors.Black }
            });
            Controls.Add(MaximumXLimitButton);
            HorizontalAveragingFlag = new RectangularPatch("horizontal avg OOB", new VisualState()
            {
                Width = 100,
                Height = 20,
                Radius = 5,
                FaceColor = SKColors.White,
            });
            Controls.Add(HorizontalAveragingFlag);

            ResetAxisBounds();
        }

        private void AxisContainer_OnClick(SKPoint position)
        {
        }

        private void AxisContainer_OnAdded(object sender, EventArgs e)
        {
        }
        public void ResetAxisBounds()
        {
            SKSize yAxisCommonSize = new SKSize(SpaceDivision_CommonYAxisWidth, Height - SpaceDivision_XAxisHeight - SpaceDivision_XAxisButtonSpace);
            if (XAxis != null)
            {
                int maxYAxis = 1;

                SKSize plotSize = new SKSize(Width - SpaceDivision_CommonYAxisWidth * maxYAxis - SpaceDivision_LegendSpace - SpaceDivision_YAxisButtonSpace, Height - SpaceDivision_XAxisHeight - SpaceDivision_XAxisButtonSpace);
                SKSize xAxisSize = new SKSize(Width - SpaceDivision_CommonYAxisWidth * maxYAxis - SpaceDivision_LegendSpace - SpaceDivision_YAxisButtonSpace, SpaceDivision_XAxisHeight);

                XAxis.SizeChanged(
                  SKGraphics.MakeRect(new SKPoint(0, Height - SpaceDivision_XAxisHeight - SpaceDivision_XAxisButtonSpace), xAxisSize),
                  SKGraphics.MakeRect(new SKPoint(Width - SpaceDivision_CommonYAxisWidth * maxYAxis - SpaceDivision_LegendSpace - SpaceDivision_YAxisButtonSpace, 0), yAxisCommonSize),
                  SKGraphics.MakeRect(new SKPoint(), plotSize));

                YAxis?.SizeChanged(
                SKGraphics.MakeRect(new SKPoint(0, Height - SpaceDivision_XAxisHeight - SpaceDivision_XAxisButtonSpace), xAxisSize),
                SKGraphics.MakeRect(new SKPoint(Width - SpaceDivision_CommonYAxisWidth * maxYAxis - SpaceDivision_LegendSpace - SpaceDivision_YAxisButtonSpace, 0), yAxisCommonSize),
                SKGraphics.MakeRect(new SKPoint(), plotSize));
                xAxisButton.VisualState.Left =(XAxis.AxisBounds.Width / 2 - xAxisButton.Width / 2);
            }
            if (MinimumYLimitButton != null)
            {
                MinimumYLimitButton.VisualState.Left = (Width - SpaceDivision_LegendSpace - MinimumYLimitButton.Width + 1);
                MinimumYLimitButton.VisualState.Top = (Height - SpaceDivision_XAxisHeight - SpaceDivision_XAxisButtonSpace - MinimumYLimitButton.Height);
            }
            if (MaximumYLimitButton != null)
            {
                MaximumYLimitButton.VisualState.Left = (Width - SpaceDivision_LegendSpace - MaximumYLimitButton.Width + 1);
                MaximumYLimitButton.VisualState.Top = (0);
            }
            if (MinimumXLimitButton != null)
            {
                MinimumXLimitButton.VisualState.Left = (0);
                MinimumXLimitButton.VisualState.Top = (Height - SpaceDivision_XAxisButtonSpace - MinimumYLimitButton.Height);
            }
            if (MaximumXLimitButton != null)
            {
                MaximumXLimitButton.VisualState.Left = (Width - SpaceDivision_LegendSpace - SpaceDivision_CommonYAxisWidth - MaximumYLimitButton.Width + 1);
                MaximumXLimitButton.VisualState.Top = (Height - SpaceDivision_XAxisButtonSpace - MinimumYLimitButton.Height);
            }
            if (HorizontalAveragingFlag != null)
            {
                HorizontalAveragingFlag.VisualState.Left = (Width - SpaceDivision_LegendSpace - SpaceDivision_CommonYAxisWidth - MaximumYLimitButton.Width + 1);
                HorizontalAveragingFlag.VisualState.Top = (MaximumXLimitButton.Bottom + 4);
            }
        }
        #endregion
        #region Data
        internal void Resume()
        {
            //if (XSeries != null)
            //    if (XSeries.BindingQuantity is TerminalTime)
            //        ((TerminalTime)XSeries.BindingQuantity).Resume();
        }


        internal void Pause()
        {
            //if (XSeries != null)
            //    if (XSeries.BindingQuantity is TerminalTime)
            //        ((TerminalTime)XSeries.BindingQuantity).Pause();
        }

        internal void ClearData()
        {
            if (XAxis.DefaultScalingMode == AutoscalingMode.Autoscroll)
                XAxis.resetToZero();
            XSeries?.Reset();
            foreach (var ySeries in YAxis.DataSeries)
                ySeries?.Reset();
            //if (XSeries != null)
            //    if (XSeries.BindingQuantity is TerminalTime)
            //        ((TerminalTime)XSeries.BindingQuantity).Reset();
        }
        public void FeedData(uint id)
        {
            XSeries?.Feed(id);
            foreach (var series in YAxis.DataSeries)
                series.Feed(id);
        }
        public void SetXSource(LoggerTerminalQuantity q)
        {
            //if (q.ClassID == ClassIDs.Defined.ClockTime && !(q is TerminalTime))
            //{
            //    q = new TerminalTime(q);
            //    ((TerminalTime)q).Reset();
            //}
            if (q == null) // remove x
            {
                DataSeries ds = null;
                SetXSource(ds);
                return;
            }
            SetXSource(new DataSeries(q, XAxis));
        }
        public void SetXSource(DataSeries ds)
        {
            if (ds == null)
            // remove
            {
                XSeries = null;
                return;
            }
            var q = ds.BindingQuantity;
            var type = q.Unit.Type;
            if (type != XAxis.UnitOptions.Type) // we need to reset the axis units
                XAxis.UnitOptions = UnitCollection.Create(type, q.Unit.Selected.Symbol);

            // change the axis button text anyways.
            // now, create a new series. The series will have the same units as the axis.

            XSeries = ds;
            XAxis.DataSeries = ds;
            // added ? for portability
            XAxis.UnitOptions = UnitCollection.Create(q.Unit);
            // change the axis button text anyways.
            var axisText = q.Unit.Type.ToString();
            axisText += " (" + q.Unit.Selected.Symbol + ")";
            xAxisButton.VisualState.Text.Text = (axisText);
            if (XAxis.UnitOptions.Type == UnitCollection.UnitTypesEnum.Time)
            {
                XAxis.SetAutoScrollSource(XSeries);
                XAxis.PPU = 50;
                XAxis.OffsetV = -XSeries.BindingQuantity.getValue();
                XAxis.OffsetG += XAxis.PlotBounds.Width - 10;
            }
            else
                XAxis.RemoveAutoScrolling();
            ResetAxisBounds();
            // this doesn't automatically get called in case a q is dropped on the x axis.
            OnStateChanged?.Invoke(this, new EventArgs());
        }

        public void AddYSource(DataSeries ds)
        {
            var q = ds.BindingQuantity;
            if (YAxis.DataSeries.Find(s => s.BindingQuantity == q) != null) // already contained
                return;

            // added ? for portability
            if (YAxis.DataSeries.Count == 0) // new units
            {
                // ?? issue when unit quantity is dropped on a unitless quantity
                if (YAxis.UnitOptions.Type == UnitCollection.UnitTypesEnum.Unitless)
                    YAxis.UnitOptions = UnitCollection.Create(q.Unit);
                // change the axis button text anyways.
                var axisText = YAxis.UnitOptions.Type.ToString() +  " (" + q.Unit.Selected.Symbol + ")";
                YAxis.AxisButton.VisualState.Text.Text = (axisText);
                // add unit selection to the button
                //YAxis.AxisButton.menu
            }
            else // mix units
            {
                var otherTypes = YAxis.DataSeries.Select(s => s.BindingQuantity.Unit.Type).ToList().FindAll(t => t != UnitCollection.UnitTypesEnum.Unitless && t != q.Unit.Type).Distinct().ToList();
                if (otherTypes.Count > 0) // conflict
                {
                    YAxis.UnitOptions = UnitCollection.Create(UnitCollection.UnitTypesEnum.Unitless);
                    // change the axis button text anyways.
                    var axisText = "Conflicting units (" + string.Join(", ", YAxis.DataSeries.Select(s => s.BindingQuantity.Unit.Type)) + ")";
                    YAxis.AxisButton.VisualState.Text.Text = (axisText);
                }
                else
                {
                    if (YAxis.DataSeries.Count > 0) // adapt to existing
                        ds.ChangeUnits(YAxis.UnitOptions.Selected);
                    YAxis.UnitOptions = UnitCollection.Create(q.Unit);
                    foreach (var series in YAxis.DataSeries)
                    {
                        series.BindingQuantity.Unit.Selected = q.Unit.Selected;
                        series.ChangeUnits(q.Unit.Selected);
                    }
                    // change the axis button text anyways.
                    var axisText = q.Unit.Type.ToString();
                    if (q.Unit.Selected == null)
                        q.Unit.Selected = q.Unit.Units[0];
                    axisText += " (" + q.Unit.Selected.Symbol + ")";
                    YAxis.AxisButton.VisualState.Text.Text = (axisText);
                }
            }
            // if axis is null, auto select one
            ResetAxisBounds();
            YAxis.DataSeries.Add(ds);
        }


        internal List<KeyValuePair<DataSeries, DataSeries>> GetSaveTargets()
        {
            List<KeyValuePair<DataSeries, DataSeries>> dsPairs = new List<KeyValuePair<DataSeries, DataSeries>>();
            foreach (var yseries in YAxis.DataSeries)
            {
                if (XSeries != null)
                    dsPairs.Add(new KeyValuePair<DataSeries, DataSeries>(XSeries, yseries));
            }
            return dsPairs;
        }

        public void AddYSource(LoggerTerminalQuantity q)
        {
            if (q == null)
            // Remove
            {
                RemoveYSource(q, YAxis);
                return;
            }
            else
            {
                // make a new series and add it to stack
                var ds = new DataSeries(q, YAxis);
                AddYSource(ds);
            }
        }

        public void RemoveYSource(LoggerTerminalQuantity q, DrawableVerticalAxis axis)
        {
            var ds = axis.DataSeries.Find(ds_ => ds_.BindingQuantity == q);
            if (ds == null) return;
            axis.DataSeries.RemoveAll(ds_ => ds_.BindingQuantity == q);
        }
        public void SetXAxis(DrawableHorizontalAxis axis, bool timeStyle)
        {
            if (timeStyle)
            {
                axis.DefaultScalingMode = AutoscalingMode.Autoscroll;
                //XAxis.OffsetG = -XAxis.PlotBounds.Width;
                axis.NeedsRefresh = true;
            }
            axis.NotifyAddedToContainer(this);
            XAxis = axis;
            ResetAxisBounds();

            if (xAxisButton != null)
                Controls.Remove(xAxisButton);
            ResetAxisBounds();
            xAxisButton = XAxis.MakeAxisButton(this);
            Controls.Add(xAxisButton);
        }
        public void SetYAxis(DrawableVerticalAxis yAxis)
        {
            if (YAxis == yAxis)
                return;
            void YAxis_OnAxisUnitsChanged(object sender, EventArgs e)
            {
                if (YAxis.DataSeries.Count == 0) return;
                if (YAxis.DataSeries.Count == 0) // new units
                {
                    var axisText = YAxis.UnitOptions.ToString() + " (" + YAxis.UnitOptions.Selected + ")";
                    YAxis.AxisButton.VisualState.Text.Text = (axisText);
                    // add unit selection to the button
                    //YAxis.AxisButton.menu
                }
                else // mix units
                {
                    var otherTypes = YAxis.DataSeries.Select(s => s.BindingQuantity.Unit.Type).ToList().FindAll(t => t != UnitCollection.UnitTypesEnum.Unitless).Distinct().ToList();
                    if (otherTypes.Count > 1) // conflict
                    {
                        // change the axis button text anyways.
                        var axisText = "Conflicting units (" + string.Join(", ", YAxis.DataSeries.Select(s => s.BindingQuantity.Unit.Type)) + ")";
                        YAxis.AxisButton.VisualState.Text.Text = (axisText);
                    }
                    else
                    {
                        var title = string.Join(", ", YAxis.DataSeries.Select(ds => ds.BindingQuantity.Unit.Type.ToString()).Distinct());
                        var axisText = title + " (" + YAxis.UnitOptions.Selected.Symbol + ")";
                        YAxis.AxisButton.VisualState.Text.Text = (axisText);

                        foreach (var series in YAxis.DataSeries)
                        {
                            series.ChangeUnits(YAxis.UnitOptions.Selected);
                        }
                    }
                }
            };
            if (YAxis != null)
                ((DrawableVerticalAxisRight)YAxis).OnAxisUnitsChanged -= YAxis_OnAxisUnitsChanged;
            YAxis = yAxis;
            ((DrawableVerticalAxisRight)YAxis).OnAxisUnitsChanged += YAxis_OnAxisUnitsChanged;
            if (yAxis is DrawableVerticalAxis)
                ;
            ResetAxisBounds();

            var b = yAxis.MakeAxisButton(this);
            // Avoid Multiple Buttons
            foreach (var dxc in Controls.FindAll(c => c.Name == b.Name))
                Controls.Remove(dxc);
            Controls.Add(b);
            yAxis.AxisButton = b;


            ResetAxisBounds();
        }

        internal void OnQuantityRemovedFromHW(LoggerTerminalQuantity q)
        {
            if (XAxis != null)
                if (XAxis.DataSeries.BindingQuantity == q)
                    SetXSource((LoggerTerminalQuantity)null);
            if (YAxis != null)
            {
                foreach (var qq in YAxis.DataSeries.Select(ds => ds.BindingQuantity).ToList())
                {
                    if (qq == q)
                        RemoveYSource(q, YAxis);
                }
            }
        }

        internal void NotifyQuantityRemoved(LoggerTerminalQuantity q)
        {
            //if (XSeries != null)
            //{
            //    if (q == XSeries.BindingQuantity)
            //        XSeries = null;
            //    // don't do anything. Keep everything on the screen. scrolling will pause automaticall
            //}
            if (YAxis.DataSeries.Count > 0)
            {
                var ally = YAxis.DataSeries.FindAll(ds => ds.BindingQuantity == q);
                foreach (var yds in ally)
                    RemoveYSource(yds.BindingQuantity, YAxis);
            }
        }

        #endregion

        #region MouseEvents
        SKPoint LastMouse = new SKPoint();
        //protected SKPoint CursorG = new SKPoint();
        SKPoint MouseDownAt = new SKPoint();

        SKPoint ScreenLoopOffset = new SKPoint();


        public void AxisContainer_OnMouseMove(SKPoint position)
        {
            // our original axis container recevied e in local coordinates.

            SKPoint eLocForSaving = new SKPoint(
                (float)position.X,
                (float)position.Y);

            SKPoint eLoc = new SKPoint(
                (float)position.X + (int)ScreenLoopOffset.X,
                (float)position.Y + (int)ScreenLoopOffset.Y);

            MoveOp CurrentMoveOp = MoveOp.None;
            var _axis = XYAxis.Find(axis => axis.CurrentMoveOp != MoveOp.None);
            if (_axis != null)
                CurrentMoveOp = _axis.CurrentMoveOp;
            if (CurrentMoveOp == MoveOp.None)
            {
                MoveOp definedMoveOp = MoveOp.None;
                foreach (var axis in XYAxis)  // check if we have a cursor outside the plot area
                {
                    if (axis.CheckOverShootImageHover(eLoc))
                    {
                        //CursorStyle = "grab";
                        axis.TentativeOp = axis.DefaultResetOp;
                        NeedsToRedraw = true;
                        definedMoveOp = axis.TentativeOp;
                        break;
                    }
                    else if (axis.AxisBounds.Contains(eLoc))
                    {
                        //Cursor = axis.DefaultScaleOp == MoveOp.xZoom ? CursorStyle.SizeWE : Cursors.SizeNS;
                        axis.TentativeOp = MoveOp.Zoom;
                        axis.MarkerCursorG = eLoc;
                        NeedsToRedraw = true;
                        definedMoveOp = axis.TentativeOp;
                        //break;
                    }
                    else
                        axis.TentativeOp = MoveOp.xyPan;

                }  // foreach end
                if (definedMoveOp != MoveOp.None)
                {
                    foreach (var axis in XYAxis)  // check if we have a cursor outside the plot area
                    {
                        if (axis.TentativeOp != definedMoveOp)
                            axis.TentativeOp = MoveOp.None;
                    }
                }
                else
                {
                    //Cursor = Cursors.Default;

                    foreach (var axis in XYAxis)
                    {
                        axis.MarkerCursorG = new SKPoint(float.NaN, float.NaN);
                        axis.CurrentMoveOp = MoveOp.None;
                        axis.TentativeOp = MoveOp.xyPan;
                        axis.NeedsRefresh = true;
                        //if (MenuStripIsShowing == null)
                        //    HoverOver = null;
                    }
                    // Note: We are skipping the hover over series for now.
                    //XAxis.TentativeOp = MoveOp.xyPan;
                    //DataSeries HoverOver = null;
                    //HoverOver = CheckHover(GtoV(eLoc, xOffsetG, XPPU, yOffsetG, YPPU, Height - XLabelHeight), 10 / XPPU, 10 / YPPU);
                    //if (HoverOver != null)
                    //    TentativeOp = MoveOp.selectSeries;
                    //if (HoverOver != bkp)
                    //{
                    //    needsRefresh = true;
                    //    if (HoverOver != null)
                    //    {
                    //        Cursor = Cursors.Help;
                    //    }
                    //}
                }
            } // if CurrentMoveOp.None
            else if (CurrentMoveOp == MoveOp.Zoom)
            {
                foreach (var axis in XYAxis)
                {
                    if (axis.CurrentMoveOp == MoveOp.Zoom)
                        axis.ScaleChanged(eLoc, LastMouse);
                }
            }
            else if (CurrentMoveOp == MoveOp.xyPan || CurrentMoveOp == MoveOp.selectSeries)
            {
                foreach (var axis in XYAxis)
                {
                    axis.OffsetChanged(eLoc, LastMouse);
                    axis.NeedsRefresh = true;
                }
            }
            LastMouse = new SKPoint(eLocForSaving.X + (int)ScreenLoopOffset.X, eLocForSaving.Y + (int)ScreenLoopOffset.Y);
            NeedsToRedraw = true;
        }

        protected void AxisContainer_OnMouseDown(SKPoint position)
        {
            SKPoint eLoc = new SKPoint(
                (float)position.X,
                (float)position.Y);

            foreach (var axis in XYAxis)
            {
                if (axis.TentativeOp != MoveOp.None)
                {
                    axis.CurrentMoveOp = axis.TentativeOp;

                    //if (axis.CurrentMoveOp == MoveOp.xyPan)
                    //    Cursor = Cursors.NoMove2D;
                    //else if (axis.CurrentMoveOp == MoveOp.Zoom)
                    //    Cursor = axis.DefaultScaleOp == MoveOp.xZoom ? Cursors.NoMoveHoriz : Cursors.NoMoveVert;
                    axis.RegisterMouseDown(eLoc);
                    if (axis.CurrentMoveOp == MoveOp.resetScale)
                    {
                        axis.AutoScaleRequested = true;
                        break;
                    }
                    else if (axis.CurrentMoveOp == MoveOp.goToXZero)
                    {
                        axis.goToZero();
                        break;
                    }

                }
            }
            // these coordinates are in local rcs
            MouseDownAt = LastMouse;
        }
        protected void AxisContainer_OnMouseUp(SKPoint position)
        {
            // remember, e is in global. we need local version for original axicContainer functions
            foreach (var axis in XYAxis)
            {
                axis.CurrentMoveOp = MoveOp.None;
                axis.TentativeOp = MoveOp.None;
                axis.NeedsRefresh = true;
                //if (MenuStripIsShowing == null)
                //    HoverOver = null;
            }
            ScreenLoopOffset = new SKPoint();
        }

        public override void OnPaint(SKCanvas g, StringBuilder debugS)
        {
            var st = DateTime.Now;
            //g.Clear(BackColor);  
            // auto set scale     
            if (XYAxis.Count != 2)
                return;
            if (Width < 0 || Height < 0)
                return;
            NeedsToRedraw = false;
            foreach (var axis in XYAxis)
            {
                //g.Clip = new Region(SKGraphics.MakeRect(100, 100, 300, 300));
                if (axis.PlotBounds.Width <= 0 || axis.PlotBounds.Height <= 0)
                    continue;
                axis.AutoSetScale();
                var bm1A = (DateTime.Now - st).TotalMilliseconds;
                //debugS.AppendLine($"XYP 01: {bm1A}");
                axis.DrawAxisAndGrid(g, this.Font);
                var bmB = (DateTime.Now - st).TotalMilliseconds;
                //debugS.AppendLine($"XYP 02: {bmB}");
            }
            var bm1 = (DateTime.Now - st).TotalMilliseconds;
            foreach (var axis in XYAxis)
            {
                axis.DrawLimits(g);
                if (YAxis != null)
                {
                    if (YAxis.HasUndershoot)
                    {
                        MinimumYLimitButton.VisualState.Opacity = (1);
                        MinimumYLimitButton.VisualState.Text.Text = (YAxis.MinDisplayedValue.ToString("F3"));
                    }
                    else MinimumYLimitButton.VisualState.Opacity = (0);

                    if (YAxis.HasOvershoot)
                    {
                        MaximumYLimitButton.VisualState.Opacity = (1);
                        MaximumYLimitButton.VisualState.Text.Text = (YAxis.MaxDisplayedValue.ToString("F3"));
                    }
                    else MaximumYLimitButton.VisualState.Opacity = (0);
                }
                if (XAxis != null && XSeries != null)
                {
                    if (XAxis.HasUndershoot && !XSeries.BindingQuantity.IsTime)
                    {
                        MinimumXLimitButton.VisualState.Opacity = (1);
                        MinimumXLimitButton.VisualState.Text.Text = (XAxis.MinDisplayedValue.ToString("F3"));
                    }
                    else MinimumXLimitButton.VisualState.Opacity = (0);

                    if (XAxis.HasOvershoot)
                    {
                        MaximumXLimitButton.VisualState.Opacity = (1);
                        MaximumXLimitButton.VisualState.Text.Text = (XAxis.MaxDisplayedValue.ToString("F3"));
                    }
                    else MaximumXLimitButton.VisualState.Opacity = (0);
                    if (XAxis.IsDoingPixelAveraging)
                        HorizontalAveragingFlag.VisualState.Opacity = (1);
                    else HorizontalAveragingFlag.VisualState.Opacity = (0);
                }
            }

            var bm2 = (DateTime.Now - st).TotalMilliseconds;
            //debugS.AppendLine($"XYP 03: {bm2}");
            foreach (var axis in XYAxis)
            {
                axis.HasUndershoot = false;
                axis.HasOvershoot = false;
            }
            if (YAxis.DataSeries.Count > 0)
            {
                XAxis.MinDisplayedValue = YAxis.DataSeries[0].MinXValue;
                XAxis.MaxDisplayedValue = YAxis.DataSeries[0].MaxXValue;
            }

            if (YAxis.DataSeries.Count > 0)
            {
                YAxis.MinDisplayedValue = YAxis.DataSeries.Min(ds => ds.MinYValue);
                YAxis.MaxDisplayedValue = YAxis.DataSeries.Max(ds => ds.MaxYValue);
                if (XAxis != null)
                    if (XAxis.DataSeries != null)
                        if (XAxis.DataSeries.BindingQuantity != null)
                            if (XAxis.DataSeries.BindingQuantity.IsTime)
                            {
                                XAxis.MinDisplayedValue = YAxis.DataSeries[0].MinXValue;
                                XAxis.MaxDisplayedValue = YAxis.DataSeries[0].MaxXValue;
                            }
            }
            var bm2b = (DateTime.Now - st).TotalMilliseconds;
            //debugS.AppendLine($"XYP 03: {bm2b}");
            for (int i = 0; i < YAxis.DataSeries.Count; i++)
            {
                var series = YAxis.DataSeries[i];
                g.Save();
                g.ClipRect(YAxis.PlotBounds);
                series.Draw(g, XSeries, XAxis, YAxis, false, debugS);
                g.Restore();
            }
            float r = 9;
            var axisP = new Pen(axisColor, 1F);
                var backP = new Pen(SKColors.White, r);

            if (XAxis != null && YAxis != null)
            {
                SKGraphics.DrawRoundedRectangle(g, backP, 0, 0, XAxis.AxisBounds.Width, YAxis.AxisBounds.Height, r);
                SKGraphics.DrawRoundedRectangle(g, axisP, 0, 0, XAxis.AxisBounds.Width, YAxis.AxisBounds.Height, r);
            }
            var bm3 = (DateTime.Now - st).TotalMilliseconds;
            //debugS.AppendLine($"XYP 04: {bm3}");
            foreach (var axis in XYAxis)
            {
                axis.DrawMarkers(g, this.Font, backColor);
                var bmC = (DateTime.Now - st).TotalMilliseconds;
                if (bmC > 10)
                    ;
            }
        }
        #endregion
    }

}
