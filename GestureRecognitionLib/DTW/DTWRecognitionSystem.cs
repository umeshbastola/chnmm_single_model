﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MoreLinq;

namespace GestureRecognitionLib.DTW
{
    public class DTWRecognitionSystem : IClassificationSystem
    {
        private bool withTime = false;
        private LinkedList<TrajectoryModel> knownTrajectories = new LinkedList<TrajectoryModel>();

        public DTWRecognitionSystem(bool withTime=false)
        {
            this.withTime = withTime;
        }

        public void clearGestures()
        {
            knownTrajectories.Clear();
        }

        public IEnumerable<string> getKnownGesturenames()
        {
            return knownTrajectories.Select(t => t.Name);
        }

        public string recognizeGesture(BaseTrajectory trace)
        {
            var t = new Template(trace.TrajectoryPoints);
            //var best = knownTrajectories.MinBy(kt => kt.getBestCost(t, withTime));

            double minCost = double.MaxValue;
            TrajectoryModel best = null;
            foreach(var kt in knownTrajectories)
            {
                var cost = kt.getBestCost(t, withTime);
                if(cost < minCost)
                {
                    minCost = cost;
                    best = kt;
                }
            }

            return best.Name;
        }

        public void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces)
        {
            var trajectoryModel = new TrajectoryModel(gestureName, trainingTraces.Select(t => t.TrajectoryPoints));
            knownTrajectories.AddLast(trajectoryModel);
        }
    }
}
