using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestureRecognitionLib;
using System.IO;

namespace SignatureDatabase
{
    using TrajectoryDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    [Serializable]
    public class SUSig_Signature : BaseTrajectory
    {
        public int UserID { get; }
        public int TraceID { get; }
        public bool IsForgery { get; }

        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public SUSig_Signature(int user, int trace, bool isForgery, TrajectoryPoint[] points)
        {
            UserID = user;
            TraceID = trace;
            IsForgery = isForgery;
            TrajectoryPoints = points;
        }
    }

    public static class SUSig
    {
        private const int BLIND_WIDTH = 5020; //in mm
        private const int BLIND_HEIGHT = 3650; //in mm
        private const int VISUAL_WIDTH = 900; //in mm
        private const int VISUAL_HEIGHT = 660; //in mm
        private const string SUSIG_BLIND_PATH = @"..\..\..\Databases\SUSig\BlindSubCorpus";
        private const string SUSIG_VISUAL_PATH = @"..\..\..\Databases\SUSig\VisualSubCorpus";

        //public static IEnumerable<IEnumerable<SUSig_Signature>> getAllSignatures(int nUser = int.MaxValue, bool useBlindDb = true)
        //{
        //    var dbPath = (useBlindDb) ? SUSIG_BLIND_PATH : SUSIG_VISUAL_PATH;
        //    var userFolders = Directory.GetDirectories(dbPath);

        //    int i = 0;
        //    foreach (var userFolder in userFolders)
        //        if (i++ < nUser) yield return readUserFolder(userFolder);
        //}

        public static IEnumerable<TrajectoryDataSet> getVerificationSet(bool useBlindDB, int? nUser, int? nTraining, bool useSkilledForgeries, bool normalize)
        {
            TrajectoryDataSet trainingSets = new TrajectoryDataSet();
            TrajectoryDataSet genuineSets = new TrajectoryDataSet();
            TrajectoryDataSet forgerySets = new TrajectoryDataSet();

            var allTrajectories = getAllSignaturesCached(useBlindDB, normalize, nUser);

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


        public static IEnumerable<IEnumerable<SUSig_Signature>> getAllSignaturesCached(bool useBlindDB, bool normalize, int? nUser = null)
        {
            //does cache exists?
            var strNUser = (nUser.HasValue) ? $"_{nUser.Value}" : "";
            var cacheFilePath = ((useBlindDB) ? SUSIG_BLIND_PATH : SUSIG_VISUAL_PATH) + @"\AllSignatures" + strNUser + ".trajectories";

            var signatures = SignatureDatabase.DeserializeData<SUSig_Signature>(cacheFilePath);

            if (signatures == null)
            {
                var allSigs = getAllSignatures(useBlindDB, normalize, nUser);
                signatures = allSigs.Select(uSigs => uSigs.ToArray()).ToArray();

                if (!SignatureDatabase.SerializeData(cacheFilePath, signatures))
                    throw new ArgumentException("File already exists");
            }

            return signatures;
        }


        public static IEnumerable<IEnumerable<SUSig_Signature>> getAllSignatures(bool useBlindDB, bool normalize, int? nUser = null)
        {
            if (nUser == null)
            {
                var users = getAllUserSignatures(null, normalize, useBlindDB).GroupBy(sig => sig.UserID);

                foreach (var user in users)
                    yield return user.AsEnumerable();
            }
            else
            {
                var userID = 1;
                for (int i = 0; i < nUser; userID++)
                {
                    
                    var sigs = getAllUserSignatures(userID, normalize, useBlindDB);

                    //skip missing userIDs
                    if (sigs.Count() == 0)
                        continue;

                    yield return sigs;
                    i++;
                }
            }
        }

        public static IEnumerable<SUSig_Signature> getAllUserSignatures(int? userID, bool normalize, bool useBlindDB)
        {
            var dbPath = useBlindDB ? SUSIG_BLIND_PATH : SUSIG_VISUAL_PATH;
            var userSearchPattern = (userID.HasValue) ? $"{userID:D3}_*.sig" : "*.sig";
            var sigFiles = Directory.GetFiles(dbPath, userSearchPattern, SearchOption.AllDirectories);

            foreach (var sigFile in sigFiles)
                yield return readSignatureFile(sigFile, normalize, useBlindDB);
        }

        private static SUSig_Signature readSignatureFile(string filePath, bool normalize = true, bool isBlindCorpus=true)
        {
            var fileNameSplit = Path.GetFileNameWithoutExtension(filePath).Split('_');
            var userID = int.Parse(fileNameSplit[0]);
            var isForgery = (fileNameSplit[1] == "f") ? true : false;
            var traceID = int.Parse(fileNameSplit[2]);

            int w = (isBlindCorpus) ? BLIND_WIDTH : VISUAL_WIDTH;
            int h = (isBlindCorpus) ? BLIND_HEIGHT : VISUAL_HEIGHT;

            var points = SignatureDatabase.readTrajectoryTextFile(filePath, ' ', 2, false, false, normalize, w, h).ToArray();

            return new SUSig_Signature(userID, traceID, isForgery, points);
        }
    }
}
