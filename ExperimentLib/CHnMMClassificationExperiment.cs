using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib;
using GestureRecognitionLib.CHnMM;
using TraceDb;

namespace ExperimentLib
{
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public class CrossValidationResult<TParamSet, TResult>
    {
        public int Id { get; private set; }
        public TParamSet Parameter { get; set; }
        public ICollection<TResult> Results { get; private set; }
        public int RecognitionTime { get; private set; }

        public CrossValidationResult(TParamSet parameter, TResult[] results, int recogTime)
        {
            Results = results;
            RecognitionTime = recogTime;
            Parameter = parameter;
        }
    }

    public class CHnMMClassificationExperiment
    {
        private string dataSourceName;
        private bool crossValidate;
        private int nTrainingOrSubsets;
        private IEnumerable<CHnMMParameter> configs;

        private IEnumerable<GestureDataSet> trainingSubSets;
        private IEnumerable<GestureDataSet> testSubSets;

        public CHnMMClassificationExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, CHnMMParameter config)
            : this(setName, crossValidate, nTrainingOrSubsets, new CHnMMParameter[] { config }) { }

        public CHnMMClassificationExperiment(string setName, bool crossValidate, int nTrainingOrSubsets, IEnumerable<CHnMMParameter> configs, ITraceDataProcessor traceProcessor = null)
        {
            this.dataSourceName = setName;
            this.nTrainingOrSubsets = nTrainingOrSubsets;
            this.crossValidate = crossValidate;
            this.configs = configs;

            ISubsetCreator subsetCreator = crossValidate ?
                                        new CrossvalidationSubsetCreator(nTrainingOrSubsets) :
                                        (ISubsetCreator)new SimpleSplitSubsetCreator(nTrainingOrSubsets);

            GestureDataSet allGestures = DataSets.getTrajectoryDataSet(setName);

            if (traceProcessor != null)
                allGestures = traceProcessor.processAllGestures(allGestures);

            subsetCreator.createSubsets(allGestures);
            trainingSubSets = subsetCreator.getTrainingSubsets();
            testSubSets = subsetCreator.getTestSubsets();
        }

        public ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>> execute(string changes)
        {
            var results = new CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>[configs.Count()];
            int i = 0;
            foreach (var config in configs)
            {
                var chnmmRec = new CHnMMClassificationSystem(config);
                var subsetResults = ClassificationExperiment.DoRecognition(chnmmRec, trainingSubSets, testSubSets);
                results[i++] = new CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>(config, subsetResults.ToArray(), -1);
            }

            var desc = (crossValidate) ? $"{nTrainingOrSubsets}subsets" : $"{nTrainingOrSubsets}trainingtraces";
            var expDesc = new ExperimentDescription($"Classification_{dataSourceName}", "CHnMM", desc, changes);
            var expResult = new ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>(results, expDesc);

            return expResult;
        }
    }
}
