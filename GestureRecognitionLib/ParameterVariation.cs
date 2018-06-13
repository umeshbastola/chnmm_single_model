using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib
{
    public abstract class ParameterVariation
    {
        public string ParamName { get; protected set; }

        public ParameterVariation(string name)
        {
            this.ParamName = name;
        }

        public abstract IEnumerable<object> getValues();
    }

    public class DoubleParamVariation : ParameterVariation
    {
        private double start;
        private double step;
        private double end;

        public DoubleParamVariation(string parName, double start, double step, double end) : base(parName)
        {
            this.start = start;
            this.step = step;
            this.end = end;
        }

        public DoubleParamVariation(string parName, double fixedValue) : base(parName)
        {
            start = fixedValue;
            step = 1;
            end = fixedValue;
        }

        public override IEnumerable<object> getValues()
        {
            for (double v = start; v <= end; v += step)
            {
                v = Math.Round(v, 3);
                yield return v;
            }
        }
    }

    public class BoolParamVariation : ParameterVariation
    {
        bool? fixedValue = null;
        public BoolParamVariation(string parName) : base(parName) { }
        public BoolParamVariation(string parName, bool fixedVal) : base(parName)
        {
            fixedValue = fixedVal;
        }

        public override IEnumerable<object> getValues()
        {
            if (!fixedValue.HasValue)
            {
                yield return false;
                yield return true;
            }
            else
            {
                yield return fixedValue.Value;
            }
        }
    }

    public class IntParamVariation : ParameterVariation
    {
        private int start;
        private int step;
        private int end;

        public IntParamVariation(string parName, int start, int step, int end) : base(parName)
        {
            this.start = start;
            this.step = step;
            this.end = end;
        }

        public IntParamVariation(string parName, int fixedVal) : base(parName)
        {
            start = fixedVal;
            step = 1;
            end = fixedVal;
        }

        public override IEnumerable<object> getValues()
        {
            for (int v = start; v <= end; v += step)
                yield return v;
        }
    }

    public class StringParamVariation : ParameterVariation
    {
        private IEnumerable<string> strings;

        public StringParamVariation(string parName, IEnumerable<string> strings) : base(parName)
        {
            this.strings = strings;
        }

        public override IEnumerable<object> getValues()
        {
            foreach (string s in strings)
                yield return s;
        }
    }

    public class CHnMMParameter
    {
        public int Id { get; set; }
        public int nAreaForStrokeMap { get; set; }
        public double toleranceFactorArea { get; set; }
        public double minRadiusArea { get; set; }
        public double areaPointDistance { get; set; }

        public bool useFixAreaNumber { get; set; }
        public bool useSmallestCircle { get; set; }

        public string distEstName { get; set; }
        public double hitProbability { get; set; }

        public bool isTranslationInvariant { get; set; }
        public bool useAdaptiveTolerance { get; set; }
        public bool useContinuousAreas { get; set; }

        public bool useEllipsoid { get; set; }


        //ToDo:
        //InterpolationMethod
        //AreaCreationMethod
        //SymbolGenerationMetod
        //ModelCreationMethod

        public static string getCSVHeaders()
        {
            var fields = typeof(CHnMMParameter).GetFields();

            return fields.Select(f => f.Name).Aggregate("", (s, n) => s + ";" + n);
        }

        public string getCSVValues()
        {
            var fields = GetType().GetFields();

            return fields.Select(f => f.GetValue(this)).Aggregate("", (s, o) => s + ";" + o.ToString());
        }

        /// <summary>
        /// verwendet momentanes Parameter Objekt (this) als Grundlage zur ParameterVariation
        /// </summary>
        /// <param name="variations"></param>
        /// <returns></returns>
        public static IEnumerable<CHnMMParameter> getParameterVariations(params ParameterVariation[] variations)
        {
            var paramNames = variations.Select(v => v.ParamName).ToArray();
            var allValuesPerParameter = variations.Select(v => v.getValues()).ToArray();

            //cartesisches Produkt bilden (also alle möglichen Kombinationen)
            //Quelle: http://stackoverflow.com/questions/10519619/create-all-possible-combinations-of-items-in-a-list-using-linq-and-c-sharp
            IEnumerable<IEnumerable<object>> emptyProduct = new[] { Enumerable.Empty<object>() };
            var cartesianProduct = allValuesPerParameter.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));

            var paramSets = cartesianProduct.Select(paramValues => Enumerable.Zip(paramValues, paramNames, (value, name) => new { ParamName = name, Value = value }).ToArray());

            foreach (var paramSet in paramSets)
            {
                var systemParams = new CHnMMParameter();
                foreach (var p in paramSet)
                {
                    var fieldInfo = systemParams.GetType().GetField(p.ParamName);
                    fieldInfo.SetValue(systemParams, Convert.ChangeType(p.Value, fieldInfo.FieldType));
                }
                yield return systemParams;
            }
        }
    }
}
