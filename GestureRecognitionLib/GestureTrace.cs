﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GestureRecognitionLib
{
    [Serializable]
    public abstract class BaseTrajectory
    {
        public abstract TrajectoryPoint[] TrajectoryPoints { get; }

        public TrajectoryPoint[] getInvariantPoints()
        {
            var start = TrajectoryPoints[0];
            return TrajectoryPoints.Select(p => p - start).ToArray();
        }
    }

    [Serializable]
    public abstract class BaseMultiTrajectory
    {
        public abstract BaseTrajectory[] Trajectories { get; }
    }

    [Serializable]
    public class TrajectoryPoint: IEqualityComparer<TrajectoryPoint>
    {
        public double X {get; set;}
        public double Y {get; set;}
        public long Time {get; set;}
        public int StrokeNum { get; set; }

        public TrajectoryPoint(double x, double y, long t, int stroke)
        {
            X = x;
            Y = y;
            Time = t;
            StrokeNum = stroke;
        }

        public bool Equals(TrajectoryPoint a, TrajectoryPoint b)
        {
            if (a != null)
            {
                if (b != null)
                {
                    if (a.X != b.X) return false;
                    if (a.Y != b.Y) return false;
                    if (a.Time != b.Time) return false;
                }
                else return false;
            }
            else if (b != null) return false;

            return true;
        }

        public int GetHashCode(TrajectoryPoint obj)
        {
            return obj.Time.GetHashCode() + obj.X.GetHashCode() + obj.Y.GetHashCode();
        }

        public static TrajectoryPoint operator -(TrajectoryPoint a, TrajectoryPoint b)
        {
            if(a is TrajectoryPoint3D a3d && b is TrajectoryPoint3D b3d)
            {
                return a3d - b3d;
            }

            return new TrajectoryPoint(a.X - b.X, a.Y - b.Y, a.Time, 0);
        }
    }

    [Serializable]
    public class TrajectoryPoint3D : TrajectoryPoint, IEqualityComparer<TrajectoryPoint3D>
    {
        public double Z { get; set; }

        public TrajectoryPoint3D(double x, double y, double z, long t) : base(x,y,t,0)
        {
            Z = z;
        }

        public bool Equals(TrajectoryPoint3D a, TrajectoryPoint3D b)
        {
            if (a != null)
            {
                if (b != null)
                {
                    if (a.X != b.X) return false;
                    if (a.Y != b.Y) return false;
                    if (a.Z != b.Z) return false;
                    if (a.Time != b.Time) return false;
                }
                else return false;
            }
            else if (b != null) return false;

            return true; ;
        }

        public int GetHashCode(TrajectoryPoint3D obj)
        {
            return obj.Time.GetHashCode() + obj.X.GetHashCode() + obj.Y.GetHashCode() + obj.Z.GetHashCode();
        }

        public static TrajectoryPoint3D operator -(TrajectoryPoint3D a, TrajectoryPoint3D b)
        {
            return new TrajectoryPoint3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.Time);
        }
    }

    [Serializable]
    public class Stroke : BaseTrajectory
    {
        public long FingerID { get; private set; }

        public int Duration { get; private set; }

        public override TrajectoryPoint[] TrajectoryPoints { get; }

        //translationinvariant points
        //public TrajectoryPoint[] InvariantPoints { get; private set; }

        public Stroke(TrajectoryPoint[] points, long fingerID)
        {
            //distinct um fehlerhafte(doppelte) Punktdaten herauszufiltern
            TrajectoryPoints = points.Distinct(points[0]).ToArray();
            //InvariantPoints = TrajectoryPoints.Select(p => p - TrajectoryPoints[0]).ToArray();

            FingerID = fingerID;
            Duration = (int)(TrajectoryPoints.Last().Time - TrajectoryPoints[0].Time);
        }
    }

    [Serializable]
    public class GestureTrace : BaseMultiTrajectory
    {
        public long ID { get; private set; }
        public GestureTrace(Stroke[] strokes, long id)
        {
            Trajectories = strokes;
            ID = id;
        }

        public override BaseTrajectory[] Trajectories { get; }

        public Stroke LongestStroke
        {
            get
            {
                //return (Stroke)Trajectories.MaxBy(s => ((Stroke)s).Duration);

                int maxDuration = 0;
                Stroke longestStroke = null;
                foreach(var t in Trajectories)
                {
                    var dur = ((Stroke)t).Duration;
                    if ( dur > maxDuration )
                    {
                        maxDuration = dur;
                        longestStroke = (Stroke)t;
                    }
                }

                return longestStroke;

            }
        }    
    }
}
