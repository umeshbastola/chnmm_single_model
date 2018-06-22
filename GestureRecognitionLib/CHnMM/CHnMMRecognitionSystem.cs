using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib.CHnMM
{
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

        public CHnMMParameter()
        {
        }

        public CHnMMParameter((string Param, object Value)[] paramSet)
        {
            foreach (var p in paramSet)
            {
                var propertyInfo = GetType().GetProperty(p.Param);
                propertyInfo.SetValue(this, Convert.ChangeType(p.Value, propertyInfo.PropertyType));
            }
        }

        public bool IsIdentical(CHnMMParameter other)
        {
            if (nAreaForStrokeMap != other.nAreaForStrokeMap
                || toleranceFactorArea != other.toleranceFactorArea
                || minRadiusArea != other.minRadiusArea
                || areaPointDistance != other.areaPointDistance
                || useFixAreaNumber != other.useFixAreaNumber
                || useSmallestCircle != other.useSmallestCircle
                || distEstName != other.distEstName
                || hitProbability != other.hitProbability
                || isTranslationInvariant != other.isTranslationInvariant
                || useAdaptiveTolerance != other.useAdaptiveTolerance
                || useContinuousAreas != other.useContinuousAreas
                || useEllipsoid != other.useEllipsoid
                ) return false;

            return true;
        }


        //ToDo:
        //InterpolationMethod
        //AreaCreationMethod
        //SymbolGenerationMetod
        //ModelCreationMethod

        //public static string getCSVHeaders()
        //{
        //    var fields = typeof(CHnMMParameter).GetFields();

        //    return fields.Select(f => f.Name).Aggregate("", (s, n) => s + ";" + n);
        //}

        //public string getCSVValues()
        //{
        //    var fields = GetType().GetFields();

        //    return fields.Select(f => f.GetValue(this)).Aggregate("", (s, o) => s + ";" + o.ToString());
        //}

        /// <summary>
        /// verwendet momentanes Parameter Objekt (this) als Grundlage zur ParameterVariation
        /// </summary>
        /// <param name="variations"></param>
        /// <returns></returns>

        //foreach (var paramSet in paramSets)
        //{
        //    var systemParams = new CHnMMParameter();
        //    foreach (var p in paramSet)
        //    {
        //        var fieldInfo = systemParams.GetType().GetField(p.ParamName);
        //        fieldInfo.SetValue(systemParams, Convert.ChangeType(p.Value, fieldInfo.FieldType));
        //    }
        //    yield return systemParams;
        //}

    }

    public class CHnMMClassificationSystem : IClassificationSystem, IVerificationSystem
    {
        private Dictionary<string, TrajectoryModel> knownGestures = new Dictionary<string, TrajectoryModel>(100);

        public CHnMMParameter ParameterSet { get; private set; }

        public GestureModelCreator HiddenModelCreator;

        public CHnMMClassificationSystem(CHnMMParameter parameter)
        {
            ParameterSet = parameter;

            //apply parameters
            StrokeMap.minimumRadius = parameter.minRadiusArea;
            StrokeMap.toleranceFactor = parameter.toleranceFactorArea;
            StrokeMap.useAdaptiveTolerance = parameter.useAdaptiveTolerance;
            StrokeMap.hitProbability = parameter.hitProbability;

            StrokeMap.useEllipsoid = parameter.useEllipsoid;
            StrokeMap.translationInvariant = parameter.isTranslationInvariant;
            FixedAreaNumberStrokeMap.nAreas = parameter.nAreaForStrokeMap;
            FixedAreaNumberStrokeMap.useContinuousAreas = parameter.useContinuousAreas;
            StrokeMap.useSmallestCircle = parameter.useSmallestCircle;
            DynamicAreaNumberStrokeMap.AreaPointDistance = parameter.areaPointDistance;
            DynamicAreaNumberStrokeMap.useSmallestCircle = parameter.useSmallestCircle;


            var transitionSetup = new TransitionCreator(parameter.hitProbability, parameter.distEstName);

            if (parameter.useFixAreaNumber)
            {
                //HiddenModelCreator = new FixedAreaNumberModelCreator(transitionSetup);
                HiddenModelCreator = new FixedAreaNumberModelCreator(transitionSetup);
            }
            else
            {
                HiddenModelCreator = new DynamicAreaNumberModelCreator(transitionSetup);
            }

        }

        public void clearGestures()
        {
            knownGestures.Clear();
        }

        public IEnumerable<string> getKnownGesturenames()
        {
            return knownGestures.Keys;
        }

        public TrajectoryModel getTrajectoryModel(string gestureName)
        {
            return knownGestures[gestureName];
        }

        public void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces)
        {
            knownGestures[gestureName] = new TrajectoryModel(this, gestureName, trainingTraces);
        }

        public bool verifyGesture(string userName, BaseTrajectory trace)
        {
            var targetGesture = knownGestures[userName];

            var similarity = targetGesture.validateGestureTrace(trace);
            //ToDo: evtl. Schwellwerte hier prüfen?

            return (similarity > 0);
        }

        public bool verifyGesture(string userName, BaseTrajectory trace, out double score)
        {
            var targetGesture = knownGestures[userName];

            var similarity = targetGesture.validateGestureTrace(trace);
            //ToDo: evtl. Schwellwerte hier prüfen?

            score = similarity;

            return (similarity > 0);
        }
        public double getSimilarity(string userName, BaseTrajectory trace)
        {
            var targetGesture = knownGestures[userName];

            var similarity = targetGesture.validateGestureTrace(trace);
            return similarity;
        }

        public bool authenticateUser(string userName, GestureTrace trace, out TrajectoryModel.ReasonForFail failReason)
        {
            var targetGesture = knownGestures[userName];
            var similarity = targetGesture.validateGestureTrace(trace.LongestStroke, out failReason);

            //ToDo: evtl. Schwellwerte hier prüfen?
            return (similarity > 0);
        }

        public List<KeyValuePair<string, double>> recognizeMultiStroke(BaseTrajectory trace)
        {
            var calculations = knownGestures.Select(gest => new { GestureName = gest.Key, Similarity = gest.Value.validateGestureTrace(trace) });
            var result = new List<KeyValuePair<string, double>>();
            foreach (var calc in calculations)
            {
                if (calc.Similarity > 0)
                {
                    result.Add(new KeyValuePair<string, double>(calc.GestureName, calc.Similarity));
                }
            }
            return result;

        }

        public int checkFeasibility(int prev_length, BaseTrajectory candidate, int nArea_count){
            var length = 0;
            foreach(var targetGesture in knownGestures){
                var sim_length = targetGesture.Value.getTrace_match(candidate);
                if (sim_length > nArea_count && (sim_length / nArea_count)  > prev_length)
                {
                    length = sim_length / nArea_count;
                    break;
                }
            }
            return length;
        }

        public int checkMultiStrokeFeasibility(string name, BaseTrajectory candidate, int nArea_count)
        {
            var length = 0;
            var targetGesture = knownGestures[name];
                var sim_length = targetGesture.getTrace_match(candidate);
                if (sim_length > nArea_count)
                {
                    length = sim_length / nArea_count;
                }

            return length;
        }

        public string recognizeGesture(BaseTrajectory trace)
        {
            var calculations = knownGestures.Select(gest => new { GestureName = gest.Key, Similarity = gest.Value.validateGestureTrace(trace) });

            //var bestGesture = calculations.MaxBy(g => g.Similarity);


            double maxSim = 0;
            string bestGesture = null;
            foreach(var calc in calculations)
            {
                //Console.WriteLine(calc.GestureName + "---" + calc.Similarity);
                if (calc.Similarity > maxSim)
                {
                    maxSim = calc.Similarity;
                    bestGesture = calc.GestureName;
                }
            }


            if (maxSim == 0) return null;
            else return bestGesture+":"+maxSim;
        }

        public string recognizeGesture(string gestureName, BaseTrajectory trace, out TrajectoryModel.ReasonForFail failReason)
        {
            //GestureRepresentation.ReasonForFail failReason;
            //var calculations = knownGestures.Select((gest,i) => new { GestureName = gest.Key, Similarity = gest.Value.validateGestureTrace(trace, out failReasons[i]) });

            double best = 0;
            string winner = "";
            TrajectoryModel.ReasonForFail tmp;
            failReason = TrajectoryModel.ReasonForFail.UNDEFINED;
            foreach (var gest in knownGestures)
            {
                var sim = gest.Value.validateGestureTrace(trace, out tmp);
                if (gest.Key == gestureName) failReason = tmp;
                if (sim > best)
                {
                    best = sim;
                    winner = gest.Key;
                }
            }

            if (best == 0) return null;
            else return winner;
        }
    }
}
