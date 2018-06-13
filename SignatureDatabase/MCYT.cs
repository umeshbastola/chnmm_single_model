using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GestureRecognitionLib;

namespace SignatureDatabase
{
    using TrajectoryDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    [Serializable]
    public class MCYT_Signature : BaseTrajectory
    {
        public int UserID { get; }
        public int TraceID { get; }
        public bool IsForgery { get; }

        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public MCYT_Signature(int user, int trace, bool isForgery, TrajectoryPoint[] points)
        {
            UserID = user;
            TraceID = trace;
            IsForgery = isForgery;
            TrajectoryPoints = points;
        }
    }

    public static class MCYT
    {
        private const int SAMPLING_FREQUENCY = 100;
        private const int DELTA_T = 1000 / SAMPLING_FREQUENCY; //ms
        private const int TABLET_WIDTH = 12700; //in mm
        private const int TABLET_HEIGHT = 9700; //in mm
        private const string MCYT_PATH = @"..\..\..\Databases\MCYT_Signature_100";

        public static IEnumerable<TrajectoryDataSet> getVerificationSet(int? nUser, int? nTraining, bool useSkilledForgeries)
        {
            TrajectoryDataSet trainingSets = new TrajectoryDataSet();
            TrajectoryDataSet genuineSets = new TrajectoryDataSet();
            TrajectoryDataSet forgerySets = new TrajectoryDataSet();

            var allTrajectories = getAllSignaturesCached(nUser);

            foreach (var userTraces in allTrajectories)
            {
                string user = userTraces.First().UserID.ToString();
                var trainingTraces = (nTraining.HasValue) ?
                    userTraces.Where(t => !t.IsForgery).Take(nTraining.Value).ToArray() :
                    userTraces.Where(t => !t.IsForgery).ToArray();
                var rest = userTraces.Except(trainingTraces).ToArray();
                var genuineTraces = rest.Where(t => !t.IsForgery).ToArray();
                var forgeryTraces = (useSkilledForgeries) ?
                    rest.Where(t => t.IsForgery).ToArray() :
                    allTrajectories.Select(t => t.First()).Where(d => d.UserID.ToString() != user).ToArray();

                trainingSets[user] = trainingTraces;
                genuineSets[user] = genuineTraces;
                forgerySets[user] = forgeryTraces;
            }

            yield return trainingSets;
            yield return genuineSets;
            yield return forgerySets;
        }


        public static IEnumerable<IEnumerable<MCYT_Signature>> getAllSignaturesCached(int? nUser = null)
        {
            //does cache exists?
            var strNUser = (nUser.HasValue) ? $"_{nUser.Value}" : "";
            var cacheFilePath = MCYT_PATH + @"\AllSignatures" + strNUser + ".trajectories";

            var signatures = SignatureDatabase.DeserializeData<MCYT_Signature>(cacheFilePath);

            if(signatures == null)
            {
                var allSigs = getAllSignatures(nUser);
                signatures = allSigs.Select(uSigs => uSigs.ToArray()).ToArray();

                if (!SignatureDatabase.SerializeData(cacheFilePath, signatures))
                    throw new ArgumentException("File already exists");
            }

            return signatures;
        }


        public static IEnumerable<IEnumerable<MCYT_Signature>> getAllSignatures(int? nUser = null)
        {
            var userFolders = Directory.GetDirectories(MCYT_PATH);

            nUser = nUser ?? int.MaxValue;

            int i = 0;
            foreach (var userFolder in userFolders)
                if (i++ < nUser) yield return readUserFolder(userFolder);
        }

        private static IEnumerable<MCYT_Signature> readUserFolder(string folderPath, bool normalize = true)
        {
            var files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                yield return readFPGFile(file, normalize);
            }
        }

        private static MCYT_Signature readFPGFile(string filePath, bool normalize = true)
        {
            var inp = new BinaryReader(File.OpenRead(filePath));

            var sig = new string( inp.ReadChars(4) );

            if (sig != "FPG ") throw new FormatException("FPG signature expected");

            var hsize = inp.ReadUInt16();
            var ver = (hsize == 48) || (hsize == 60) ? 2 : 1;
            var format = inp.ReadUInt16();

            if (format == 4)
                inp.ReadUInt16();

            var can = inp.ReadUInt16();
            var TS = inp.ReadUInt32();
            var res = inp.ReadUInt16();
            inp.BaseStream.Position += 4;
            var coef = inp.ReadUInt32();
            var mvector = inp.ReadUInt32();
            var nvectores = inp.ReadUInt32();
            var nc = inp.ReadUInt16();

            if(ver == 2)
            {
                var FS = inp.ReadUInt32();
                var mventana = inp.ReadUInt32();
                var msolapadas = inp.ReadUInt32();
            }

            //inp.BaseStream.Position = hsize-12;

            var datos = inp.ReadUInt32();
            var delta = inp.ReadUInt32();
            var ddelta = inp.ReadUInt32();

            //inp.BaseStream.Position = hsize;

            //switch(res)
            //{
            //    case 8:
            //    case 16:
            //    case 32:
            //    default: break;
            //}

            if (res != 32) throw new FormatException("Only 32bit resolution supported");

            var tam_tot = nvectores * can * mvector;

            var fileName = Path.GetFileName(filePath);
            var userID = int.Parse(fileName.Substring(0, 4));
            var isForgery = (fileName[4] == 'f') ? true : false;
            var traceID = int.Parse(fileName.Substring(5, 2));

            var points = Enumerable.Range(0, (int)nvectores).Select(i =>
             {
                 var x = (double)inp.ReadSingle();
                 var y = (double)inp.ReadSingle();

                 //skip unneccessary stuff
                 inp.BaseStream.Position += (mvector - 2) * 4;

                 var t = i * DELTA_T;

                 if (normalize)
                     SignatureDatabase.normalizeCoords(ref x, ref y, TABLET_WIDTH, TABLET_HEIGHT);

                 return new TrajectoryPoint(x, y, t, 0);
             }).ToArray();

            inp.Close();

            return new MCYT_Signature(userID, traceID, isForgery, points);
        }
    }

}
