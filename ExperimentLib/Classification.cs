using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib;
using System.Diagnostics;

namespace ExperimentLib
{
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public class ClassificationResult
    {
        public long Id { get; private set; }
        public string ExecutedGesture { get; private set; }
        public string ClassifiedGesture { get; private set; }
        public int RecognitionTime { get; private set; }
        public long ExecutedTraceID { get; private set; }

        //public GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail FailReason { get; private set; }

        public ClassificationResult(string executed, string classified, int recogTime, long traceId/*, GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail fail*/)
        {
            ExecutedGesture = executed;
            ClassifiedGesture = classified;
            RecognitionTime = recogTime;
            ExecutedTraceID = traceId;
            //FailReason = fail;
        }
    }

    public class ClassificationSubsetResult
    {
        public int Id { get; private set; }
        public int SubsetIndex { get; private set; }
        public ICollection<ClassificationResult> Results { get; private set; }
        public int RecognitionTime { get; private set; }
        public int TrainTime { get; private set; }

        public ClassificationSubsetResult(ClassificationResult[] results, int subsetIndex, int recogTime, int trainTime)
        {
            SubsetIndex = subsetIndex;
            Results = results;
            RecognitionTime = recogTime;
            TrainTime = trainTime;
        }

    }

    public static class ClassificationExperiment
    {
        public static LinkedList<ClassificationSubsetResult> DoRecognition(IClassificationSystem recognizer, IEnumerable<GestureDataSet> trainingSubSets, IEnumerable<GestureDataSet> testSubSets)
        {
            Stopwatch swSingle = new Stopwatch();
            Stopwatch swSubset = new Stopwatch();
            Stopwatch swTrain = new Stopwatch();

            var result = new LinkedList<ClassificationSubsetResult>();
            var subsets = trainingSubSets.Zip(testSubSets, (train, test) => new { TrainingSubset = train, TestSubset = test });
            int subsetIndex = 0;
            foreach (var subset in subsets)
            {
                //train recognition system
                recognizer.clearGestures();
                swTrain.Restart();
                foreach (var e in subset.TrainingSubset)
                {
                    recognizer.trainGesture(e.Key, e.Value);
                }
                swTrain.Stop();

                var tmpResult = new LinkedList<ClassificationResult>();
                //test recognition system
                swSubset.Restart();
                foreach (var e in subset.TestSubset)
                {
                    foreach (var testTrace in e.Value)
                    {
                        swSingle.Restart();
                        var classifiedGesture = recognizer.recognizeGesture(testTrace);
                        swSingle.Stop();

                        tmpResult.AddLast(new ClassificationResult(e.Key, classifiedGesture, (int)swSingle.ElapsedMilliseconds, -1/*, GestureRecognitionLib.CHnMM.TrajectoryModel.ReasonForFail.UNDEFINED*/));
                    }
                }
                swSubset.Stop();

                result.AddLast(new ClassificationSubsetResult(tmpResult.ToArray(), subsetIndex, (int)swSubset.ElapsedMilliseconds, (int)swTrain.ElapsedMilliseconds));
                subsetIndex++;
            }

            return result;
        }
    }
}
