using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using MathNet.Numerics.Interpolation.Algorithms;
using LfS.ModelLib.Models;
using GestureRecognitionLib.CHnMM.Algorithms;
using GestureRecognitionLib.CHnMM.Estimators;
using MathNet.Numerics.LinearAlgebra;

namespace GestureRecognitionLib.CHnMM
{

    //    public enum IntersectionType
    //    {
    //        None = 0,
    //        LineEntersArea,
    //        LineInArea,
    //        LineLeavesArea,
    //        LineCompletelyIntersectsArea
    //    }

    //    public class Area
    //    {
    //        //private TouchPoint[] srcPoints;
    //        private double minimumRadius = 0.03;

    //        public double X { get; private set; }
    //        public double Y { get; private set; }

    //        //ToDo: evtl. Ellipse?
    //        public double Radius { get; private set; }

    //        public double ToleranceRadius { get; private set; }

    //        /// <summary>
    //        /// returns whether a given points lies within the area
    //        /// </summary>
    //        /// <param name="x"></param>
    //        /// <param name="y"></param>
    //        /// <returns></returns>
    //        public bool PointInArea(double x, double y)
    //        {
    //            var difX = x - X;
    //            var difY = y - Y;
    //            var dis = Math.Sqrt( difX*difX + difY*difY );
    //            if (dis <= Radius) return true;

    //            return false;
    //        }

    //        /// <summary>
    //        /// returns whether a given points lies within the tolerance area
    //        /// </summary>
    //        /// <param name="x"></param>
    //        /// <param name="y"></param>
    //        /// <returns></returns>
    //        public bool PointInToleranceArea(double x, double y)
    //        {
    //            var difX = x - X;
    //            var difY = y - Y;
    //            var dis = Math.Sqrt(difX * difX + difY * difY);
    //            if (dis <= ToleranceRadius) return true;

    //            return false;
    //        }
    //        /// <summary>
    //        /// 
    //        /// </summary>
    //        /// <param name="x1"></param>
    //        /// <param name="y1"></param>
    //        /// <param name="x2"></param>
    //        /// <param name="y2"></param>
    //        /// <returns> 0 - line does not intersect, 1 - line enters area, 2 - line lies within area, 3 - line leaves area, 4 - line completely intersects area</returns>
    //        public IntersectionType LineIntersectsArea(double x1, double y1, double x2, double y2)
    //        {


    //            //if (inArea1 == 1)
    //            //{
    //            //    //erster Punkt in Area

    //            //}


    //            //Geradenparameter aus gegebenen Punkten bestimmen
    //            //Koordinatenform für Geraden: ax + by = c
    //            //Quelle: http://de.wikipedia.org/wiki/Koordinatenform#Aus_der_Zweipunkteform
    //            var a = y1 - y2;
    //            var b = x2 - x1;
    //            var c = x2 * y1 - x1 * y2;

    //            //Anzahl der Schnittpunkte bestimmen
    //            //Quelle: http://de.wikipedia.org/wiki/Schnittpunkt#Schnittpunkte_einer_Gerade_mit_einem_Kreis
    //            var d = c - a * X - b * Y;
    //            var det = Radius * Radius * (a * a + b * b);

    //            if(det < d)
    //            {
    //                //kein Schnittpunkt
    //                return 0;
    //            }
    //            else
    //            {
    //                var inArea1 = PointInArea(x1,y1);
    //                var inArea2 = PointInArea(x2,y2);

    //                if (det > d)
    //                {
    //                    //2 Schnittpunkte
    //                    if(inArea1)
    //                    {
    //                        if (inArea2) return IntersectionType.LineInArea;
    //                        else return IntersectionType.LineLeavesArea;
    //                    }
    //                    else
    //                    {
    //                        if (inArea2) return IntersectionType.LineEntersArea;
    //                        else
    //                        {
    //                            //beide Punkte außerhalb der Area; Gerade hat 2 Schnittpunkte
    //                            //einen der 2 Schnittpunkte berechnen:

    //                            var t1 = -(Math.Sqrt((-(x1 * x1) + 2 * X * x1 - X * X + Radius * Radius) * y2 * y2 + (((2 * x1 - 2 * X) * x2 - 2 * X * x1 + 2 * X * X - 2 * Radius * Radius) * y1 + ((2 * X - 2 * x1) * x2 + 2 * x1 * x1 - 2 * X * x1) * Y) * y2 + (-(x2 * x2) + 2 * X * x2 - X * X + Radius * Radius) * y1 * y1 +
    //(2 * x2 * x2 + (-2 * x1 - 2 * X) * x2 + 2 * X * x1) * Y * y1 + (-(x2 * x2) + 2 * x1 * x2 - x1 * x1) * Y * Y + Radius * Radius * x2 * x2 - 2 * Radius * Radius * x1 * x2 + Radius * Radius * x1 * x1) - y2 * y2 + (y1 + Y) * y2 - Y * y1 - x2 * x2 + (x1 + X) * x2 - X * x1) / (y2 * y2 - 2 * y1 * y2 + y1 * y1 + x2 * x2 - 2 *
    //x1 * x2 + x1 * x1);
    //                            if (t1 >= 0 && t1 <= 1) return IntersectionType.LineCompletelyIntersectsArea;
    //                            else return IntersectionType.None;

    //                        }
    //                    }

    //                }
    //                else //det == d
    //                {
    //                    //1 Schnittpunkt
    //                    throw new NotImplementedException();
    //                }
    //            }

    //        }

    //        public Area(IEnumerable<TouchPoint> srcPoints, double toleranceFactor, double minRadius)
    //        {
    //            //ToDo: Varianten: Umkreis um Punkte oder gewichteten Umkreis um Punkte; momentan gewichtet
    //            //srcPoints = points;

    //            minimumRadius = minRadius;

    //            //calculate Area center
    //            X = srcPoints.Average(p => p.X);
    //            Y = srcPoints.Average(p => p.Y);

    //            //calculate radius
    //            Radius = srcPoints.Max(p => { var dx = X - p.X; var dy = Y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
    //            if(Radius < minimumRadius) Radius = minimumRadius;

    //            ToleranceRadius = Radius * toleranceFactor;
    //        }
    //    }


    /// <summary>
    /// represents the areas created from a set of strokes
    /// </summary>
    public abstract class StrokeMap
    {
        public static bool translationInvariant = false;
        public static double minimumRadius = 0.01;
        public static double toleranceFactor = 1.5;
        public static bool useSmallestCircle = false;
        public static bool useAdaptiveTolerance = false;
        public static double hitProbability = 0.9;
        public static bool useEllipsoid = true;

        //used to provide unique IDs for each strokeMap which are used for symbols
        protected static int IDCounter = 0;
        protected IStrokeInterpolation[] interpolatedStrokes;

        public int ID { get; private set; }
        public int Strokes { get; protected set; }
        public Area[] Areas { get; protected set; }

        public StrokeMap(BaseTrajectory[] srcStrokes)
        {
            //verwendet vorerst lineare Interpolation

            interpolatedStrokes = srcStrokes.Select(s => CreateInterpolation(s)).ToArray();
            ID = IDCounter++;

            //sind Strokes in etwa gleichlang?
            //var minArcLength = interpolatedStrokes.Min(s => s.ArcLength);
            //var maxArcLength = interpolatedStrokes.Max(s => s.ArcLength);
            //Debug.Assert(maxArcLength - minArcLength < 0.5);
        }

        protected IStrokeInterpolation CreateInterpolation(BaseTrajectory stroke)
        {
            var points = (translationInvariant) ? stroke.getInvariantPoints() : stroke.TrajectoryPoints;
            if (points[0] is TrajectoryPoint3D)
                return new LinearInterpolation3D(points.Cast<TrajectoryPoint3D>().ToArray());
            else
                return new LinearInterpolation(points);
        }

        public abstract Observation[] getSymbolTrace(BaseTrajectory s);
    }

    /// <summary>
    /// represents the areas created from a set of strokes
    /// </summary>
    public class FixedAreaNumberStrokeMap : StrokeMap
    {
        public static int nAreas = 10;
        //public static bool useSmallestCircle = true;
        public static bool useContinuousAreas = true;

        public FixedAreaNumberStrokeMap(BaseTrajectory[] srcStrokes)
            : base(srcStrokes)
        {
            int current_stroke = 0;
            int total_strokes = srcStrokes.First().TrajectoryPoints.Last().StrokeNum;
            var accumulate = new List<TrajectoryPoint>();
            var gesture = new List<TrajectoryPoint[][]>();
            var gesture_equidistant = new List<TrajectoryPoint[]>();
            foreach (var execution in srcStrokes)
            {
                foreach (var point in execution.TrajectoryPoints)
                {
                    if (point.StrokeNum == current_stroke)
                    {
                        accumulate.Add(point);
                    }
                    else
                    {
                        var trajectory = new LinearInterpolation(accumulate.ToArray());
                        gesture_equidistant.Add(LinearInterpolation.getEquidistantPoints(trajectory, nAreas));
                        accumulate.Clear();
                        current_stroke++;
                        if (current_stroke > total_strokes)
                        {
                            gesture.Add(gesture_equidistant.ToArray());
                            gesture_equidistant.Clear();
                            current_stroke = 0;
                        }
                    }
                }

            }
            gesture_equidistant.Add(LinearInterpolation.getEquidistantPoints(new LinearInterpolation(accumulate.ToArray()), nAreas));
            gesture.Add(gesture_equidistant.ToArray());

            int total_areas = nAreas * (total_strokes + 1);
            Areas = new Area[total_areas];
            Strokes = total_strokes+1;
            List<TrajectoryPoint>[] arrayList = new List<TrajectoryPoint>[total_areas];
            for (int i = 0; i < total_areas; i++)
                arrayList[i] = new List<TrajectoryPoint>();
            foreach (var gesture_stroke in gesture)
            {
                int counter = 0;
                for (int b = 0; b <= total_strokes; b++)
                {
                    for (int a = 0; a < nAreas; a++)
                    {
                        arrayList[counter].Add(gesture_stroke[b][a]);
                        counter++;
                    }
                }
            }
            for (int a = 0; a < total_areas; a++)
            {
                Areas[a] = createArea(a, arrayList[a]);
            }
        }

        private Area createArea(int ID, IEnumerable<TrajectoryPoint> srcPoints)
        {
            if (srcPoints.First() is TrajectoryPoint3D)
            {
                return createArea3D(ID, srcPoints.Cast<TrajectoryPoint3D>());
            }

            return createArea2D(ID, srcPoints);
        }

        private Area createArea2D(int ID, IEnumerable<TrajectoryPoint> srcPoints)
        {
            Area area;
            double x, y, r; //circle
            double tolR;

            if (useSmallestCircle)
            {
                var enclCirc = SmallestCircleAlgorithm.makeCircle(srcPoints.Select(tp => new Point(tp.X, tp.Y)));

                x = enclCirc.c.x;
                y = enclCirc.c.y;
                r = enclCirc.r;
            }
            else
            {
                x = srcPoints.Average(p => p.X);
                y = srcPoints.Average(p => p.Y);
                r = srcPoints.Max(p => { var dx = x - p.X; var dy = y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
            }

            if (r < minimumRadius) r = minimumRadius;

            if (useAdaptiveTolerance)
            {
                var n = srcPoints.Count();
                var f = ((double)(n + 1) / n); //converges from 2 to 1
                tolR = r + (r * toleranceFactor - r) * f;
            }
            else
            {
                tolR = r * toleranceFactor;
            }

            if (useContinuousAreas)
            {
                var distances = srcPoints.Select(p => { var dx = x - p.X; var dy = y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
                var stochastic = new AreaNormalEstimator(minimumRadius);
                area = new ContinuousCircle(ID, x, y, stochastic.Distribution, stochastic.StandardDeviation);
            }
            else
            {
                area = new DiscreteCircle(ID, x, y, r, tolR, hitProbability);
            }

            return area;
        }

        private Area createArea3D(int ID, IEnumerable<TrajectoryPoint3D> srcPoints)
        {
            Area area;
            double x, y, z, r; //sphere
            double tolR;

            if (useSmallestCircle)
            {
                var bs = BoundingSphere.Calculate(srcPoints.Select(p => Vector<float>.Build.DenseOfArray(new float[] { (float)p.X, (float)p.Y, (float)p.Z })));
                var enclCirc = SmallestCircleAlgorithm.makeCircle(srcPoints.Select(tp => new Point(tp.X, tp.Y)));

                x = bs.center[0];
                y = bs.center[1];
                z = bs.center[2];
                r = bs.radius;
            }
            else
            {
                x = srcPoints.Average(p => p.X);
                y = srcPoints.Average(p => p.Y);
                z = srcPoints.Average(p => p.Z);
                r = srcPoints.Max(p => { var dx = x - p.X; var dy = y - p.Y; var dz = z - p.Z; return Math.Sqrt(dx * dx + dy * dy + dz * dz); });
            }

            if (r < minimumRadius) r = minimumRadius;

            if (useAdaptiveTolerance)
            {
                var n = srcPoints.Count();
                var f = ((double)(n + 1) / n); //converges from 2 to 1
                tolR = r + (r * toleranceFactor - r) * f;
            }
            else
            {
                tolR = r * toleranceFactor;
            }

            if (useContinuousAreas)
            {
                throw new NotImplementedException();
                //var distances = srcPoints.Select(p => { var dx = x - p.X; var dy = y - p.Y; return Math.Sqrt(dx * dx + dy * dy); });
                //var stochastic = new AreaNormalEstimator(minimumRadius);
                //area = new ContinuousCircle(ID, x, y, stochastic.Distribution, stochastic.StandardDeviation);
            }
            else
            {
                //area = new DiscreteSphere(ID, x, y, z, r, tolR, hitProbability);
                if (useEllipsoid)
                {
                    area = new DiscreteEllipsoid(ID, srcPoints);
                }
                else
                {
                    area = new DiscreteSphere(ID, x, y, z, r, tolR, hitProbability);
                }
            }

            return area;
        }



        //public override bool strokeFitsMap(Stroke s)
        //{
        //    var areaPoints = LinearInterpolation.getEquidistantPoints(new LinearInterpolation(s.Points), areas.Length);
        //    return !areaPoints.Zip(areas,
        //                (p, a) => a.PointInToleranceArea(p.X, p.Y)
        //           ).Any(b => !b);
        //}

        // usage: someObject.SingleItemAsEnumerable();
        public static IEnumerable<T> SingleItemAsEnumerable<T>(T item)
        {
            yield return item;
        }

        public override Observation[] getSymbolTrace(BaseTrajectory s)
        {
            int current_stroke = s.TrajectoryPoints.First().StrokeNum;
            var accumulate = new List<TrajectoryPoint>();
            var gesture_AreaPoints = new List<TrajectoryPoint[]>();
            foreach (var point in s.TrajectoryPoints)
            {
                if (point.StrokeNum == current_stroke)
                {
                    accumulate.Add(point);
                }
                else
                {
                    var trajectory = new LinearInterpolation(accumulate.ToArray());
                    gesture_AreaPoints.Add(LinearInterpolation.getEquidistantPoints(trajectory, nAreas));
                    accumulate.Clear();
                    current_stroke = point.StrokeNum;
                }
            }
            gesture_AreaPoints.Add(LinearInterpolation.getEquidistantPoints(new LinearInterpolation(accumulate.ToArray()), nAreas));

            var symbolTrace = new Observation[Areas.Length + gesture_AreaPoints.Count];
            int counter = 0;
            int stroke_counter = 0;
            foreach (var fStroke in gesture_AreaPoints)
            {
                var Areas_fraction = Areas.Skip(stroke_counter * fStroke.Length).Take(nAreas);
                var query = fStroke.Zip(Areas_fraction, (p, a) =>
                {
                    var sym = a.CreateSymbol(p);
                    if (sym == null) return null;
                    string symbol = "S" + ID + "_" + sym;
                    return new Observation(symbol, p.Time);
                });
                //GestureStart symbol
                var startO = new Observation("GestureStart", s.TrajectoryPoints[0].Time);
                foreach (var single_observation in SingleItemAsEnumerable(startO).Concat(query.TakeWhile(o => o != null)).ToArray())
                {
                    symbolTrace[counter] = single_observation;
                    counter++;
                }
                stroke_counter++;
            }

            //wurden alle Areas erwischt?
            if (symbolTrace.Length < Areas.Length) return null;

            return symbolTrace;
        }
    }

    public class DynamicAreaNumberStrokeMap : StrokeMap
    {
        public static double AreaPointDistance = 0.05;
        public static bool useSmallestCircle = true;

        public DynamicAreaNumberStrokeMap(BaseTrajectory[] srcStrokes)
            : base(srcStrokes)
        {
            if (useSmallestCircle)
                generateSmallestEnclosingCircles();
            else
                generateWeightedEnclosingCircles();
        }

        private void generateWeightedEnclosingCircles()
        {
            throw new NotImplementedException();
            //var areaPointsPerStroke = interpolatedStrokes.Select(s => LinearInterpolation.getPointsByDistance(s, AreaPointDistance)).ToArray();
            ////var maxNArea = areaPointsPerStroke.Max(areaPoints => areaPoints.Length);

            ////Areas = Enumerable.Range(0, maxNArea).Select(
            ////            a => new WeightedEnclosingCircle(areaPointsPerStroke.Where(points => points.Length > a).Select(points => points[a])) //Where was added here due to different area counts
            ////        ).ToArray();

            //var minNArea = areaPointsPerStroke.Min(areaPoints => areaPoints.Length);

            //Areas = Enumerable.Range(0, minNArea).Select(
            //            a => new WeightedEnclosingCircle(areaPointsPerStroke.Select(points => points[a]))
            //        ).ToArray();
        }

        private void generateSmallestEnclosingCircles()
        {
            throw new NotImplementedException();
            //var areaPointsPerStroke = interpolatedStrokes.Select(s => LinearInterpolation.getPointsByDistance(s, AreaPointDistance)).ToArray();
            ////var maxNArea = areaPointsPerStroke.Max(areaPoints => areaPoints.Length);

            ////Areas = Enumerable.Range(0, maxNArea).Select(
            ////            a => new SmallestEnclosingCircle(areaPointsPerStroke.Where(points => points.Length > a).Select(points => points[a])) //Where was added here due to different area counts
            ////        ).ToArray();

            //var minNArea = areaPointsPerStroke.Min(areaPoints => areaPoints.Length);

            //Areas = Enumerable.Range(0, minNArea).Select(
            //            a => new SmallestEnclosingCircle(areaPointsPerStroke.Select(points => points[a]))
            //        ).ToArray();
        }

        //public override bool strokeFitsMap(Stroke s)
        //{
        //    var areaPoints = LinearInterpolation.getPointsByDistance(new LinearInterpolation(s.Points), AreaPointDistance);
        //    return !areaPoints.Zip(areas,
        //                (p, a) => a.PointInToleranceArea(p.X, p.Y)
        //           ).Any(b => !b);
        //}

        public override Observation[] getSymbolTrace(BaseTrajectory s)
        {
            var strokeInterpolation = (translationInvariant) ?
                new LinearInterpolation(s.getInvariantPoints()) :
                new LinearInterpolation(s.TrajectoryPoints);
            var areaPoints = LinearInterpolation.getPointsByDistance(strokeInterpolation, AreaPointDistance);

            //ToDo: tolerate longer inputs than trained?
            //if (areaPoints.Length > Areas.Length) return null;

            //ToDo: tolerate shorter inputs?
            if (areaPoints.Length < Areas.Length)
            {
                return null;
            }

            var observations = new Observation[Areas.Length + 2]; //+ GS and GE
            int iObs = 1;

            for (int iArea = 0; iArea < Areas.Length; iArea++)
            {
                var area = Areas[iArea];
                if (iArea >= areaPoints.Length) break;

                var point = areaPoints[iArea];
                var sym = area.CreateSymbol(point);
                if (sym == null) return null;
                string symbol = "S" + ID + "_" + sym;

                //if (area.PointInArea(point.X, point.Y))
                //{
                //    symbol = baseSym + (iArea + 1) + "_Hit";
                //}
                //else if (area.PointInToleranceArea(point.X, point.Y))
                //{
                //    symbol = baseSym + (iArea + 1) + "_Tolerance";
                //}
                //else return null;

                observations[iObs++] = new Observation(symbol, point.Time);
            }

            //GestureStart symbol
            observations[0] = new Observation("GestureStart", s.TrajectoryPoints[0].Time);
            //GestureEnd symbol
            observations[observations.Length - 1] = new Observation("GestureEnd", s.TrajectoryPoints[s.TrajectoryPoints.Length - 1].Time);

            return observations;
        }

    }
}
