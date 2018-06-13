using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentLib
{
    public abstract class ParameterVariation
    {
        public string ParamName { get; protected set; }

        public ParameterVariation(string name)
        {
            this.ParamName = name;
        }

        public abstract IEnumerable<object> getValues();

        public static IEnumerable<(string Param, object Value)[]> getParameterVariations(params ParameterVariation[] variations)
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

            return cartesianProduct.Select(paramValues => Enumerable.Zip(paramValues, paramNames, (value, name) => (name, value)).ToArray());
        }
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

        public StringParamVariation(string parName, string fixedString) : base(parName)
        {
            this.strings = new string[] { fixedString };
        }

        public override IEnumerable<object> getValues()
        {
            foreach (string s in strings)
                yield return s;
        }
    }
}
