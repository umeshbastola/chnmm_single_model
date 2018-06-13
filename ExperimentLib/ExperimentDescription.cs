using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentLib
{
    public class ExperimentDescription
    {
        public int Id { get; private set; }
        public string ExperimentType { get; private set; }
        public string UsedMethod { get; private set; }
        public string Description { get; private set; }
        public string Changes { get; private set; }

        public ExperimentDescription(string expType, string usedMethod, string desc, string changes)
        {
            ExperimentType = expType;
            UsedMethod = usedMethod;
            Description = desc;
            Changes = changes;
        }
    }

    public class ExperimentResults<T>
    {
        public int Id { get; private set; }
        public ExperimentDescription ExperimentDescription { get; private set; }
        public DateTime Created { get; private set; }

        public ICollection<T> Results { get; private set; }

        public ExperimentResults(T[] results, ExperimentDescription expDesc)
        {
            ExperimentDescription = expDesc;
            Results = results;
            Created = DateTime.Now;
        }
    }
}
