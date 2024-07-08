using SkiaSharp;

namespace FidgetSpinnerWASM2.Models
{
    public class AnalysisItem
    {
        public readonly string itemTitle = "";
        public string[] ids;
        public List<double> X = new();
        public List<double>[] Y;
        public AnalysisItem(string name, List<double> times, List<double>[] dataPoints, string[] names)
        {
            itemTitle = name;
            ids = names;
            X = times;
            Y = dataPoints;
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
