namespace FidgetSpinnerWASM2.Models
{
    public class AnalysisItem
    {
        public readonly string itemTitle = "";
        public Dictionary<string, Pages.Plotter.XYPoint[]> data = new ();
        public AnalysisItem(string name, List<double> times, List<double>[] dataPoints, string[] names)
        {
            itemTitle = name;
            for (int i = 0; i < dataPoints.Length; i++)
            {
                var ps = new List<Pages.Plotter.XYPoint>();
                for (int j = 0; j < times.Count; j++)
                    ps.Add(new Pages.Plotter.XYPoint { X = (decimal)times[j], Y = (decimal)dataPoints[i][j] });
                data.Add(names[i], ps.ToArray());
            }
        }

        // Note: this is important so the MudSelect can compare pizzas
        public override bool Equals(object o)
        {
            var other = o as AnalysisItem;
            return other?.itemTitle == itemTitle;
        }

        // Note: this is important too!
        public override int GetHashCode() => itemTitle?.GetHashCode() ?? 0;

        // Implement this for the AnalysisItem to display correctly in MudSelect
        public override string ToString() => itemTitle;
    }
}
