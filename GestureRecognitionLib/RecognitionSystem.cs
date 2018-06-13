using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognitionLib
{
    public interface IClassificationSystem
    {
        IEnumerable<string> getKnownGesturenames();
        string recognizeGesture(BaseTrajectory trace);
        void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces);
        void clearGestures();
    }

    public interface IVerificationSystem
    {
        void clearGestures();
        bool verifyGesture(string gestureName, BaseTrajectory trace);
        bool verifyGesture(string name, BaseTrajectory trace, out double score);
        void trainGesture(string gestureName, IEnumerable<BaseTrajectory> trainingTraces);
    }
}
