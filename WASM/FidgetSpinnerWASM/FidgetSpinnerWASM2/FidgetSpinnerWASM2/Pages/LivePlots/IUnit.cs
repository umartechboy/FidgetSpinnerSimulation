namespace FidgetSpinnerWASM2.Pages.LivePlots
{
    public class UnitCollection
    {
        public interface IUnit
        {
            string Symbol { get; }
            string AlphabaticSymbol { get; }
            string Name { get; }
            /// <summary>
            /// Conversion From SI units
            /// </summary>
            float TF(float v);
            /// <summary>
            /// Conversion to SI units
            /// </summary>
            float ITF(float v);
        }
        public class NullUnit : IUnit
        {
            public string Symbol { get => "units"; }
            public string AlphabaticSymbol { get => "units"; }
            public string Name { get; } = "Unitless";

            public float ITF(float v)
            {
                return v;
            }

            public float TF(float v)
            {
                return v;
            }
            public override string ToString()
            {
                return Symbol;
            }
        }

        public class Unity : IUnit
        {
            public Unity(string symbol, string completeName, string alphaSymbol = null)
            {
                Symbol = symbol;
                if (alphaSymbol == null)
                    AlphabaticSymbol = symbol;
                else
                    AlphabaticSymbol = alphaSymbol;
                Name = completeName;
            }
            public string Symbol { get; private set; }
            public string AlphabaticSymbol { get; private set; }

            public string Name { get; private set; }

            public float ITF(float v)
            {
                return v;
            }

            public float TF(float v)
            {
                return v;
            }
            public override string ToString()
            {
                return Symbol;
            }
        }
        public class GenericUnit : IUnit
        {
            public GenericUnit(string symbol, string name, Func<float, float> tf, Func<float, float> itf)
            {
                Symbol = symbol;
                AlphabaticSymbol = symbol;
                tf_ = tf;
                itf_ = itf;
                Name = name;
            }
            public string Symbol { get; private set; }
            public string AlphabaticSymbol { get; private set; }
            Func<float, float> tf_;
            Func<float, float> itf_;

            public Func<float, float> SecondaryTF;

            public Func<float, float> SecondaryITF;
            public float ITF(float v)
            {
                if (SecondaryITF != null)
                    return SecondaryITF(itf_(v));
                return itf_(v);
            }

            public float TF(float v)
            {
                if (SecondaryTF != null)
                    return SecondaryTF(tf_(v));
                return tf_(v);
            }
            public override string ToString()
            {
                return Symbol;
            }
            public string Name { get; private set; }
        }
        public class Minute : IUnit
        {
            public string Symbol { get => "min"; }
            public string AlphabaticSymbol { get => "min"; }

            public float TF(float v)
            {
                return v / 60.0F;
            }

            public float ITF(float v)
            {
                return v * 60.0F;
            }
            public string Name { get; } = "Minute";
        }
        public class DegreeC : IUnit
        {
            public string Symbol { get => "ºC"; }
            public string AlphabaticSymbol { get => "C"; }

            public float TF(float v)
            {
                return v - 273.15F;
            }

            public float ITF(float v)
            {
                return v + 273.15F;
            }
            public string Name { get; } = "Degree Celsius";
        }
        public class KgF : IUnit
        {
            public string Symbol { get => "kgf"; }
            public string AlphabaticSymbol { get => "kgf"; }

            public float TF(float v)
            {
                return v * 9.812F;
            }

            public float ITF(float v)
            {
                return v / 9.812F;
            }
            public string Name { get; } = "KG force";
        }
        public class DegreeF : IUnit
        {
            public string Symbol { get => "ºF"; }
            public string AlphabaticSymbol { get => "F"; }

            public float TF(float v)
            {
                return (v - 273.15F) * 9 / 5 + 32;
            }

            public float ITF(float v)
            {
                return -32 * 5.0F / 9 + 273.15F;
            }
            public string Name { get; } = "Fahrenheit";
        }
        public class Degree : IUnit
        {
            public string Symbol { get => "°"; }
            public string AlphabaticSymbol { get => "deg"; }

            public float TF(float v)
            {
                return (float)(v * 360 / 2 / Math.PI);
            }

            public float ITF(float v)
            {
                return (float)(v / 360 * 2 * Math.PI);
            }
            public string Name { get; } = "Degree";
        }

        public virtual UnitCollection Clone()
        {
            return Parse(this.ToString());
        }

        public class DegreesPerSecond : IUnit
        {
            public string Symbol { get => "°/s"; }
            public string AlphabaticSymbol { get => "deg/s"; }

            public float TF(float v)
            {
                return (float)(v * 360 / 2 / Math.PI);
            }

            public float ITF(float v)
            {
                return (float)(v / 360 * 2 * Math.PI);
            }
            public string Name { get; } = "Degrees per second";
        }
        public class DegreesPerMinute : IUnit
        {
            public string Symbol { get => "°/min"; }
            public string AlphabaticSymbol { get => "deg/min"; }

            public float TF(float v)
            {
                return (float)(v * 360 / 2 / Math.PI) * 60;
            }

            public float ITF(float v)
            {
                return (float)(v / 360 * 2 * Math.PI) / 60;
            }
            public string Name { get; } = "Degrees per minute";
        }
        public class Revolution : IUnit
        {
            public string Symbol { get => "rev"; }
            public string AlphabaticSymbol { get => "rev"; }

            public float TF(float v)
            {
                return (float)(v / 2 / Math.PI);
            }

            public float ITF(float v)
            {
                return (float)(v * 2 * Math.PI);
            }
            public string Name { get; } = "Revolution";
        }
        public class RevolutionsPerSecond : IUnit
        {
            public string Symbol { get => "rev/s"; }
            public string AlphabaticSymbol { get => "rev/s"; }

            public float TF(float v)
            {
                return (float)(v / 2 / Math.PI);
            }

            public float ITF(float v)
            {
                return (float)(v * 2 * Math.PI);
            }
            public string Name { get; } = "Revolutions per second";
        }
        public class RevolutionsPerMinute : IUnit
        {
            public string Symbol { get => "rev/min"; }
            public string AlphabaticSymbol { get => "rev/min"; }

            public float TF(float v)
            {
                return (float)(v / 2 / Math.PI) * 60;
            }

            public float ITF(float v)
            {
                return (float)(v * 2 * Math.PI) / 60;
            }
            public string Name { get; } = "Revolutiuons per minute";
        }
        public class Gauss : IUnit
        {
            public string Symbol { get => "G"; }
            public string AlphabaticSymbol { get => "G"; }

            public float TF(float v)
            {
                return v * 10000;
            }

            public float ITF(float v)
            {
                return v / 10000;
            }
            public string Name { get; } = "Gauss";
        }
        public static UnitTypesEnum ParseType(string type)
        {
            return (UnitTypesEnum)Enum.Parse(typeof(UnitTypesEnum), type);
        }
        public UnitTypesEnum Type { get; private set; }
        protected UnitCollection(UnitTypesEnum type, string selectedUnitSymbol, params IUnit[] units)
        {
            Type = type;
            Units = units.ToList();
            if (selectedUnitSymbol != null)
                Selected = Units.Find(u => u.Symbol == selectedUnitSymbol);
            else
                Selected = Units[0];
        }
        public List<IUnit> Units { get; private set; } = new List<IUnit>();
        IUnit _sel;
        public IUnit Selected
        {
            get { return _sel; }
            set { _sel = value == null ? null : Units.Find(u => u.Symbol == value.Symbol); }
        }
        public enum UnitTypesEnum : byte
        {
            Voltage = 0,
            Current,
            Power,
            Distance,
            Temperature,
            Unitless,
            Time,
            AngularSpeed,
            Angle,
            Mass,
            ElectricalResistance,
            MagneticFlux,
            FlowRate,
            Pressure,
            LinearSpeed,
            Percentage,
            Frequency,
            Force
        }
        internal static UnitCollection Create(UnitCollection unit)
        {
            if (unit == null)
                return Create(UnitTypesEnum.Unitless);
            if (unit.Type == UnitTypesEnum.Unitless)
                return Create(UnitTypesEnum.Unitless);
            var uc = Create(unit.Type, unit.Selected.Symbol);
            uc.Selected = unit.Selected;
            return uc;
        }
        public static UnitCollection Parse(string str)
        {
            return Create((UnitTypesEnum)Enum.Parse(typeof(UnitTypesEnum), str.Split(new char[] { ';' })[0]), str.Split(new char[] { ';' })[1]);
        }
        public override string ToString()
        {
            return Type.ToString() + ";" + Selected.Symbol;
        }
        public static UnitCollection? Create(UnitTypesEnum type, string selectedSymbol = null)
        {
            if (type == UnitTypesEnum.Voltage)
                return new UnitCollection(type, selectedSymbol, new Unity("V", "Volt"), new GenericUnit("mV", "Millivolt", (v) => { return v * 1000; }, (v) => { return v / 1000; }));
            else if (type == UnitTypesEnum.Current)
                return new UnitCollection(type, selectedSymbol, new Unity("A", "Ampere"), new GenericUnit("mA", "Milliampere", (v) => { return v * 1000; }, (v) => { return v / 1000; }));
            else if (type == UnitTypesEnum.Distance)
                return new UnitCollection(type, selectedSymbol, new Unity("m", "Meter"),
                    new GenericUnit("mm", "Millimeter", (v) => { return v * 1000; }, (v) => { return v / 1000; }),
                    new GenericUnit("cm", "Centimeter", (v) => { return v * 100; }, (v) => { return v / 100; }),
                    new GenericUnit("km", "Killometer", (v) => { return v / 1000; }, (v) => { return v * 1000; })
                    );
            else if (type == UnitTypesEnum.Power)
                return new UnitCollection(type, selectedSymbol, new Unity("W", "Watt"), new GenericUnit("mW", "Milliwatt", (v) => { return v * 1000; }, (v) => { return v / 1000; }));
            if (type == UnitTypesEnum.Temperature)
                return new UnitCollection(type, selectedSymbol, new Unity("K", "Kelvin"), new DegreeC(), new DegreeF());
            else if (type == UnitTypesEnum.Mass)
                return new UnitCollection(type, selectedSymbol, new Unity("Kg", "Kilogram"), new GenericUnit("g", "gram", (v) => { return v * 1000; }, (v) => { return v / 1000; }), new KgF());
            else if (type == UnitTypesEnum.Force)
                return new UnitCollection(type, selectedSymbol,
                    new Unity("N", "newton"),
                    new GenericUnit("kgf", "kgf", (v) => { return v / 9.812F; }, (v) => { return v * 9.812F; }));
            else if (type == UnitTypesEnum.Unitless)
            {
                return new UnitCollection(type, "units", new NullUnit());
            }
            else if (type == UnitTypesEnum.Time)
                return new UnitCollection(type, selectedSymbol, new Unity("s", "Second"), new Minute());
            else if (type == UnitTypesEnum.Angle)
                return new UnitCollection(type, selectedSymbol, new Unity("rad", "Radian"), new Degree(), new Revolution());
            else if (type == UnitTypesEnum.AngularSpeed)
                return new UnitCollection(type, selectedSymbol, new Unity("rad/s", "Radian per second"), new DegreesPerSecond(), new DegreesPerMinute(), new RevolutionsPerSecond(), new RevolutionsPerMinute());
            else if (type == UnitTypesEnum.ElectricalResistance)
                return new UnitCollection(type, selectedSymbol, new Unity("Ω", "Ohm", "ohm")); // no alternate units
            else if (type == UnitTypesEnum.MagneticFlux)
                return new UnitCollection(type, selectedSymbol, new Unity("T", "Tesla"), new Gauss());
            else if (type == UnitTypesEnum.Pressure)
                return new UnitCollection(type, selectedSymbol,
                    new Unity("Pa", "Pascal"),
                    new GenericUnit("kPa", "Killopascal", (v) => { return (float)(v * 1e-3); }, (v) => { return (float)(v / 1e-3); }),
                    new GenericUnit("bar", "Bar", (v) => { return (float)(v * 1e-5); }, (v) => { return (float)(v / 1e-5); }),
                    new GenericUnit("atm", "Atmosphere", (v) => { return (float)(v / 101325); }, (v) => { return (float)(v * 101325); }),
                    new GenericUnit("psi", "Pounds per square inch", (v) => { return (float)(v / 6894.7573); }, (v) => { return (float)(v * 6894.7573); }),
                    new GenericUnit("torr", "millimeters of mercury", (v) => { return (float)(v / 133.3224); }, (v) => { return (float)(v * 133.3224); })
                    );
            else if (type == UnitTypesEnum.FlowRate)
                return new UnitCollection(type, selectedSymbol,
                    new Unity("m^3/s", "Cubic meters per second", "m3/s"),
                    new GenericUnit("ml/h", "Milliliters per minute", (v) => { return (float)(v * 1e6 * 3600); }, (v) => { return (float)(v / 1e6 / 3600); }),
                    new GenericUnit("ul/h", "Microliters per hour", (v) => { return (float)(v * 1e9 * 3600); }, (v) => { return (float)(v / 1e9 / 3600); }),
                    new GenericUnit("ml/min", "Milliliters per minute", (v) => { return (float)(v * 1e6 * 60); }, (v) => { return (float)(v / 1e6 / 60); }),
                    new GenericUnit("ul/m", "Microliters per minute", (v) => { return (float)(v * 1e9 * 60); }, (v) => { return (float)(v / 1e9 / 60); }),
                    new GenericUnit("ml/s", "Milliliters per second", (v) => { return (float)(v * 1e6); }, (v) => { return (float)(v / 1e6); }),
                    new GenericUnit("ul/s", "Microliters per second", (v) => { return (float)(v * 1e9); }, (v) => { return (float)(v / 1e9); })
                    );
            else if (type == UnitTypesEnum.LinearSpeed)
                return new UnitCollection(type, selectedSymbol,
                    new Unity("m/s", "Meters per second", "m/s"),
                    new GenericUnit("km/h", "Killometers per hour", (v) => { return (float)(v / 1000 * 3600); }, (v) => { return (float)(v * 1000 / 3600); }),
                    new GenericUnit("mi/h", "Miles per hour", (v) => { return (float)(v * 0.621371 * 3600); }, (v) => { return (float)(v / 0.621371 / 3600); }),
                    new GenericUnit("mm/s", "Milliliters per second", (v) => { return (float)(v * 1e3); }, (v) => { return (float)(v / 1e3); }),
                    new GenericUnit("mm/min", "Milliliters per minute", (v) => { return (float)(v * 1e3 * 60); }, (v) => { return (float)(v / 1e3 / 60); }),
                    new GenericUnit("mm/h", "Millimeters per hour", (v) => { return (float)(v * 1e3 * 3600); }, (v) => { return (float)(v / 1e3 / 3600); })
                    );
            else if (type == UnitTypesEnum.Frequency)
                return new UnitCollection(type, selectedSymbol,
                    new Unity("hz", "Hertz", "hz"),
                    new GenericUnit("mHz", "Millihertz", (v) => { return (float)(v * 1e3); }, (v) => { return (float)(v * 1e-3); }),
                    new GenericUnit("uHz", "Microhertz", (v) => { return (float)(v * 1e6); }, (v) => { return (float)(v * 1e-6); }),
                    new GenericUnit("kHz", "Killohertz", (v) => { return (float)(v * 1e-3); }, (v) => { return (float)(v * 1e3); }),
                    new GenericUnit("MHz", "Megahertz", (v) => { return (float)(v * 1e-6); }, (v) => { return (float)(v * 1e6); }),
                    new GenericUnit("GHz", "Gegahertz", (v) => { return (float)(v * 1e-9); }, (v) => { return (float)(v * 1e9); })
                    );
            if (type == UnitTypesEnum.Percentage)
                return new UnitCollection(type, selectedSymbol, new Unity("Fraction", ""), new GenericUnit("%", "%", (v) => { return v * 100; }, (v) => { return v / 100; }));

            return null;
        }
    }

}
