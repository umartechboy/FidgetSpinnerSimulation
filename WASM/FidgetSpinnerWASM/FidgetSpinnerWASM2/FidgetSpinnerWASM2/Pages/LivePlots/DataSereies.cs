using SkiaSharp;
using System.Text;
using static FidgetSpinnerWASM2.Pages.LivePlots.UnitCollection;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class DataSeries
    {
        public DrawableAxis AxisAssociation { get; private set; } = null;
        
        public LoggerTerminalQuantity BindingQuantity { get; protected set; }
        public UnitCollection.IUnit CurrentUnit { get; private set; }
        public List<float> Data { get; private set; } = new List<float>();
        public UInt32 IDOffset { get; private set; } = 0;
        UInt32 NextID = 0;
        // this can be used to adjust offset in virtual series having lesser datapoints than the source.
        int MissingStartPoints = 0;

        List<DrawableAxis> AutoscrollSubscriptions = new List<DrawableAxis>();
        Task ParallelCalculationsThread;
        public DataSeries(LoggerTerminalQuantity binding, DrawableAxis axis)
        {
            BindingQuantity = binding;
            AxisAssociation = axis;
            CurrentUnit = binding.Unit.Selected;

            ParallelCalculationsThread = new Task(async () =>
            {
                try
                {
                    await parallelCalcLoop();
                }
                catch { }
            });
            ParallelCalculationsThread.Start();
        }
        float lastV = float.NaN;
        public void Feed(UInt32 id)
        {
            if (id < NextID)
                return;
            NextID = id + 1;
            if (Data.Count == 0)
                IDOffset = id;
            var v = BindingQuantity.getValue();

            Data.Add(v);
            if (float.IsNaN(lastV))
                lastV = v;
            foreach (var axis in AutoscrollSubscriptions)
                axis.NotifyScroll(v - lastV, id);
            AxisAssociation.NeedsRefresh = true;
            lastV = v;
        }
        public float this[int index]
        {
            get
            {
                try
                {
                    return Data[index + MissingStartPoints];
                }
                catch
                {
                    return 0;
                }
            }
        }
        public int Count { get { return Data.Count - MissingStartPoints; } }

        List<LineCalculatorState> tasks = new List<LineCalculatorState>();
        // this class represents an instance of this series on an individual axis, plotted against a single x series.
        public class LineCalculatorState
        {
            public DataSeries xSeries;
            public DrawableAxis xAxis;
            public DrawableAxis yAxis;
            public bool needsToRecalculate = false;
            public SKPoint[] Result = new SKPoint[0];
        }
        bool parallelCalcTasksAvailable = false;
        double rate = 0, time = 0;
        bool parallelTaskOnGoing = false;
        bool resetSeriesRequest = false;
        public float MinXValue { get; protected set; } = 0;
        public float MinYValue { get; protected set; } = 0;
        public float MaxXValue { get; protected set; } = 0;
        public float MaxYValue { get; protected set; } = 0;
        async Task parallelCalcLoop()
        {
            while (true)
            {
                if (resetSeriesRequest)
                    Data.Clear();
                resetSeriesRequest = false;
                parallelTaskOnGoing = false;
                while (parallelCalcTasksAvailable == false)
                    await Task.Delay(5);
                parallelCalcTasksAvailable = false;
                parallelTaskOnGoing = true;
                for (int j = 0; j < tasks.Count; j++)
                {
                    var st = DateTime.Now;
                    if (tasks[j] == null)
                        continue;
                    if (!tasks[j].needsToRecalculate)
                        continue;
                    var task = tasks[j];
                    tasks[j].needsToRecalculate = false;
                    var xSeries = task.xSeries;
                    var ySeries = this;
                    int xso = 0;
                    int yso = 0;
                    if (xSeries == null)
                    { await Task.Delay(30); continue; }
                    if (xSeries.IDOffset > IDOffset) // x has lesser points.
                        yso = (int)(xSeries.IDOffset - IDOffset);
                    else if (xSeries.IDOffset < IDOffset) // y has lesser points.
                        xso = (int)(IDOffset - xSeries.IDOffset);
                    var yAxis = task.yAxis;
                    var xAxis = task.xAxis;
                    var Count = ySeries.Count - yso;
                    if (xSeries.Count - xso < Count)
                        Count = ySeries.Count - yso;
                    List<SKPoint> ps = new List<SKPoint>();

                    if (Count > 2)
                    {
                        float sumOfValues = 0;
                        float sumOfTimes = 0;
                        float summedValues = 0;
                        float lastAddedXInF = xSeries[xso] * xAxis.PPU;
                        int lastAddedX = (int)Math.Round(lastAddedXInF) - 1;
                        float xAxisOffsetG = xAxis.OffsetG;
                        float xAxisPPU = xAxis.PPU;
                        float yAxisOffsetG = yAxis.OffsetG;
                        float yAxisPPU = yAxis.PPU;
                        int skipping = 1;
                        int startI = 0;
                        var bm1 = DateTime.Now - st;
                        if (xAxis.DefaultScalingMode == AutoscalingMode.Autoscroll)
                        {
                            var firstXV = xAxis.GtoV(0, xAxisOffsetG, xAxisPPU);
                            var lastXV = xAxis.GtoV(xAxis.PlotBounds.Width, xAxisOffsetG, xAxisPPU);
                            double dx = xSeries[1 + xso] - xSeries[0 + xso];
                            var valuesPerPixel = (lastXV - firstXV) / dx / xAxis.PlotBounds.Width;
                            if (valuesPerPixel > 40)
                                skipping = (int)(valuesPerPixel / 20 + 1);

                            //skip whats before xG = 0;
                            int currentInd = 0;
                            int inc = Count / 2;

                            while (true)
                            {
                                if (inc < 2)
                                    break;
                                if (currentInd + inc >= Count - 2)
                                    // crossed zero. too much inc
                                    inc /= 2;
                                if (currentInd + inc >= Count)
                                {
                                    inc /= 2;
                                    continue;
                                }
                                if (xAxis.VtoG(xSeries[currentInd + inc + xso], xAxisOffsetG, xAxisPPU) > 0)
                                // crossed zero. go back
                                {
                                    inc /= 2;
                                    continue;
                                }
                                // safe to come this far
                                currentInd += inc;
                            }
                            while (startI + skipping < currentInd)
                                startI += skipping;
                        }
                        else // skipping for auto scale, like phase portraits
                        {
                            skipping = Count / 100000 + 1;
                        }
                        float xAxis_MinDisplayedValue = float.MaxValue;
                        float xAxis_MaxDisplayedValue = float.MinValue;
                        float yAxis_MinDisplayedValue = float.MaxValue;
                        float yAxis_MaxDisplayedValue = float.MinValue;
                        xAxis.IsDoingPixelAveraging = false;

                        var bm2 = DateTime.Now - st;
                        if (xSeries.Data.Count == 0)
                            break;
                        if (xAxis.VtoG(xSeries.Data.Last(), xAxisOffsetG, xAxisPPU) > xAxis.PlotBounds.Width)
                        {
                            xAxis.HasOvershoot = true;
                            xAxis_MaxDisplayedValue = xSeries.Data.Last();
                        }
                        for (int i = startI; i < Count; i += skipping)
                        {
                            float xToAddInF = xSeries[i + xso] * xAxisPPU;
                            var xToAdd = (int)Math.Round(xToAddInF);
                            summedValues++;
                            sumOfValues += ySeries[i + yso];
                            sumOfTimes += xSeries[i + xso];
                            if (xToAdd != lastAddedX)
                            {
                                //averaged = Math.Round((i - lastIAdded) * 0.01 + averaged * 0.99);
                                //lastIAdded = i;
                                float xV = sumOfTimes / summedValues;
                                float yV = sumOfValues / summedValues;

                                float xG = xAxis.VtoG(xV, xAxisOffsetG, xAxisPPU);
                                float yG = yAxis.VtoG(yV, yAxisOffsetG, yAxisPPU);
                                if (xG > xAxis.PlotBounds.Width)
                                {
                                    // check if its a time scale, skip whats after xG = 0;
                                    if (xAxis.DefaultScalingMode == AutoscalingMode.Autoscroll)
                                        break;
                                }
                                //if (xG >= 0 )
                                {
                                    if (xG < 0)
                                        xAxis.HasUndershoot = true;
                                    if (xG > xAxis.PlotBounds.Width)
                                        xAxis.HasOvershoot = true;

                                    if (xV < xAxis_MinDisplayedValue || float.IsInfinity(xAxis_MinDisplayedValue))
                                        xAxis_MinDisplayedValue = xV;
                                    if (xV > xAxis_MaxDisplayedValue || float.IsInfinity(xAxis_MaxDisplayedValue))
                                        xAxis_MaxDisplayedValue = xV;

                                    if (yG < 0)
                                        yAxis.HasOvershoot = true;
                                    if (yG > yAxis.PlotBounds.Height)
                                        yAxis.HasUndershoot = true;
                                    if (yV < yAxis_MinDisplayedValue || float.IsInfinity(yAxis_MinDisplayedValue))
                                        yAxis_MinDisplayedValue = yV;
                                    if (yV > yAxis_MaxDisplayedValue || float.IsInfinity(yAxis_MaxDisplayedValue))
                                        yAxis_MaxDisplayedValue = yV;

                                    ps.Add(new SKPoint(xG, yG));
                                }

                                summedValues = 0;
                                sumOfTimes = 0;
                                sumOfValues = 0;
                                lastAddedX = xToAdd;
                            }
                            else
                            {
                                xAxis.IsDoingPixelAveraging = true;
                            }
                        }
                        var bm3 = DateTime.Now - st;
                        time = (DateTime.Now - st).TotalMilliseconds;
                        rate = rate * 0.99 + 0.01 * time / (double)Count * 1e6;

                        if (xAxis_MinDisplayedValue!= float.MaxValue)
                            MinXValue = xAxis_MinDisplayedValue;
                        if (xAxis_MaxDisplayedValue != float.MinValue)
                            MaxXValue = xAxis_MaxDisplayedValue;
                        if (yAxis_MinDisplayedValue != float.MaxValue)
                            MinYValue = yAxis_MinDisplayedValue;
                        if (yAxis_MaxDisplayedValue != float.MinValue)
                            MaxYValue = yAxis_MaxDisplayedValue;

                        if (xAxis.RequestToRemoveHorizontalAveraging)
                        {
                            if (xAxis.IsDoingPixelAveraging)
                            {
                                xAxis.PPU *= 1.5F;
                                ((DrawableHorizontalAxis)xAxis).ScrollToEnd();
                            }
                            else
                                xAxis.RequestToRemoveHorizontalAveraging = false;
                        }
                        var bm4 = DateTime.Now - st;
                    }


                    //AxisAssociation.NeedsRefresh = true;
                    // lets add this to the result
                    task.Result = ps.ToArray();
                }
            }
        }

        //public void SaveAgainst(string root, params DataSeries[] ySeriesList)
        //{
        //    if (!themedResources.IsValidDataDir(root))
        //        return;
        //    if (ySeriesList == null)
        //        return;
        //    if (ySeriesList.Length == 0)
        //        return;
        //    int xo = 0;
        //    float xVAdjust = 0;
        //    if (this.AxisAssociation.UnitOptions.Type == UnitTypesEnum.Time)
        //    {
        //        xo = (int)(ySeriesList.ToList().Min(vy => vy.IDOffset) - this.IDOffset);
        //        xVAdjust = this[xo];
        //    }
        //    var fileSeed = this.BindingQuantity.Title.ActiveString + "-" +
        //                string.Join(";", ySeriesList.Select(ys =>
        //                MultilingualText.Parse(themedResources, ys.BindingQuantity.Title).ActiveString));

        //    themedResources.GetAutoFileName(root, fileSeed, ".csv",
        //        (completeFileName) =>
        //        {
        //            var fs = System.IO.File.OpenWrite(completeFileName);
        //            StreamWriter sb = new StreamWriter(fs);
        //            var str = "Sr. No.," +
        //                MultilingualText.Parse(themedResources, this.BindingQuantity.Title).ActiveString.Replace(",", "_") + "_" + this.BindingQuantity.Unit.Selected.Symbol + "," +
        //                string.Join(",", ySeriesList.Select(ys =>
        //                MultilingualText.Parse(themedResources, ys.BindingQuantity.Title).ActiveString.Replace(",", "_") + "_" + ys.BindingQuantity.Unit.Selected.Symbol));
        //            sb.WriteLine(str);
        //            int crsr = 1;
        //            for (int i = xo; i < Data.Count; i++)
        //            {
        //                sb.Write(crsr + ", "); crsr++;
        //                int xi = i;
        //                string vx = "";
        //                if (xi >= 0)
        //                    vx = SharedResources.FormatNumber((this[xi] - xVAdjust), themedResources.SignificantFiguresAfterDecimalForFiles, themedResources.NumberFormatForFiles);
        //                sb.Write(vx + ", ");
        //                for (int y = 0; y < ySeriesList.Length; y++)
        //                {
        //                    var xSeries = this;
        //                    var ySeries = ySeriesList[y];
        //                    int yi = xi + (int)((int)this.IDOffset - (int)ySeries.IDOffset);
        //                    string vy = "";
        //                    if (yi >= 0 && yi < ySeries.Count)
        //                        vy = SharedResources.FormatNumber(ySeries[yi], themedResources.SignificantFiguresAfterDecimalForFiles, themedResources.NumberFormatForFiles);
        //                    sb.Write(vy + ((y + 1 < ySeriesList.Length) ? ", " : ""));
        //                }
        //                sb.WriteLine();
        //            }
        //            sb.Flush();
        //            fs.Flush();
        //            fs.Close();
        //            DXMessageBox.Build(themedResources, DXMessageBox.Preset.DataSaved, completeFileName)
        //            .ShowNotification();
        //        });
        //}

        internal void ChangeUnits(IUnit selected)
        {
            if (CurrentUnit == null)
                return;
            if (selected == null)
                return;
            if (CurrentUnit.Symbol == selected.Symbol)
                return;
            for (int i =0; i < Data.Count;i++)
                Data[i] = selected.TF(CurrentUnit.ITF(Data[i]));
            CurrentUnit = selected;
            BindingQuantity.Unit.Selected = CurrentUnit;
        }

        internal void NotifyRemoved()
        {
            // ParallelCalculationsThread?.Interrupt();
        }

        internal void Reset()
        {
            if (parallelTaskOnGoing)
                resetSeriesRequest = true;
            else
                Data.Clear();
            lastV = float.NaN;
        }

        public void Draw(SKCanvas g, DataSeries xSeries, DrawableHorizontalAxis xAxis, DrawableVerticalAxis yAxis, 
            bool showHighlighted,
            StringBuilder debugS

            )
        {
            var ySeries = this;

            if (Enabled)
            {
                try
                {
                    var state = tasks.Find(ts =>
                    ts.xAxis == xAxis &&
                    ts.yAxis == yAxis &&
                    ts.xSeries == xSeries);
                    if (xSeries == null || ySeries == null)
                        return;
                    if (xSeries.Count < 2 || ySeries.Count < 2)
                        return;
                    if (state == null)
                    {
                        tasks.Add(new LineCalculatorState()
                        {
                            xAxis = xAxis,
                            yAxis = yAxis,
                            xSeries = xSeries
                        });
                        return;
                    }
                    state.needsToRecalculate = true;
                    parallelCalcTasksAvailable = true;

                    SKPoint[] ps = new SKPoint[0];
                    ps = state.Result;
                    SKColor c = LineColor.WithAlpha((byte)(LineOpacity * 255));
                    Pen PlotP = new Pen(c, LineThickness);
                    if (ps.Length > 1)
                    {
                        if (showHighlighted)
                        {
                            SKGraphics.DrawLines(g, new Pen(GraphicsUtils.GetContrast(PlotP.Color), PlotP.Width + 3), ps.ToArray());
                            SKGraphics.DrawLines(g, new Pen(PlotP.Color.WithAlpha(255), LineThickness), ps.ToArray());
                        }
                        else
                        {
                            SKGraphics.DrawLines(g, PlotP, ps.ToArray());
                        }
                    }
                    if (LastPointHighlight && ps.Length > 0)
                    {
                        var p = ps[ps.Length - 1];
                        SKGraphics.DrawRectangle(g, Pen, (int)(p.X - LineThickness * 2), (int)(p.Y - LineThickness * 2), LineThickness * 4, LineThickness * 4);
                    }
                    //debugS.AppendLine(
                    //    Math.Round(time).ToString() + "ms, " + Math.Round(rate) + "ms/M, " + averaged);
                }
                catch (Exception ex)
                { }
            }
        }

        public bool CheckSubscription(DrawableAxis axis)
        {
            return AutoscrollSubscriptions.Contains(axis);
        }
        public void SubscribeAutoScroll(DrawableAxis axis)
        {
            if (!AutoscrollSubscriptions.Contains(axis))
                AutoscrollSubscriptions.Add(axis);
        }
        public void UnsubscribeAutoScroll(DrawableAxis axis)
        {
            if (AutoscrollSubscriptions.Contains(axis))
                AutoscrollSubscriptions.Remove(axis);
        }


        #region Visuals
        public void OnVisualsChanged() { AxisAssociation.NeedsRefresh = true; }
        public int ID { get; protected set; }
        public string Title
        {
            get { return BindingQuantity.Title; }
            set
            {
                BindingQuantity.Title = value;
            }
        }
        public Pen Pen
        {
            get
            {
                SKColor c = LineColor.WithAlpha((byte)(LineOpacity * 255));
                return new Pen(c, LineThickness);
            }
            set
            {
                LineThickness = value.Width;
                LineColor = value.Color;
            }
        }
        public SKColor LineColor
        {
            get { return BindingQuantity.Color; }
            set
            {
                BindingQuantity.Color = value;
                OnVisualsChanged();
            }
        }

        //public void Adapt(TimeSeries series)
        //{
        //    _lt = series.LineThickness;
        //    _lo = series.LineOpacity;
        //    _ln = series.Name;
        //    _lc = series.LineColor;
        //    _se = series.Enabled;
        //}
        public float LineThickness
        {
            get { return BindingQuantity.LineThickness; }
            protected set
            {
                BindingQuantity.LineThickness = value;
                OnVisualsChanged();
            }
        }
        public float LineOpacity
        {
            get { return LineColor.Alpha / 255.0F; }
            set
            {
                LineColor = LineColor.WithAlpha((byte)Math.Round(value * 255));
                OnVisualsChanged();
            }
        }
        public bool LastPointHighlight { get; set; } = false;
        bool _se = true;
        public bool Enabled
        {
            get { return _se; }
            set
            {
                _se = value;
                OnVisualsChanged();
            }
        }
        #endregion
        public override string ToString()
        {
            return "Dataseries: " + BindingQuantity.Title;
        }
    }

}
