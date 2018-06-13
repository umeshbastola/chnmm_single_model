using System;
using System.Collections.Generic;
using System.Linq;
using GestureRecognitionLib;

namespace MultiStrokeGestureRecognitionLib
{
    public class StrokeData : BaseTrajectory
    {
        public int UserID { get; }
        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public StrokeData(int user, List<KeyValuePair<int, string[]>> points)
        {
            UserID = user;
            TrajectoryPoints = convertPoints(points).ToArray();
        }

        public static IEnumerable<TrajectoryPoint> convertPoints(List<KeyValuePair<int, string[]>> points)
        {
            foreach (KeyValuePair<int, string[]> entry in points)
            {
                var point = entry.Value;
                int x = Convert.ToInt32(point[0]);
                int y = Convert.ToInt32(point[1]);
                long t = Convert.ToInt64(point[2]);
                int seq = Convert.ToInt32(point[3]);
                double dx = x;
                double dy = y;
                yield return new TrajectoryPoint(dx, dy, t, seq);
            }
        }
    }
}