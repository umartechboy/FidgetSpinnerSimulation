using SkiaSharp;

namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class LoggerTerminalQuantity
    {
        public bool IsTime { get; set; } = false;
        UnitCollection _uc = UnitCollection.Create(UnitCollection.UnitTypesEnum.Unitless);
        public string Title { get; set; } = "";
        public SKColor Color { get; set; }
        public float LineThickness { get; set; } = 2;
        public UnitCollection Unit
        {
            get { return _uc; }
            set
            {
                _uc = value.Clone();
            }
        }
        float value = 0;
        public float getValue()
        {
            return value;
        }
    }
}
