using GestureRecognitionLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignatureDatabase
{
    [Serializable]
    public class SG_NOTE_Signature : BaseTrajectory
    {
        public int UserID { get; }
        public int TraceID { get; }
        public bool IsSession2 => TraceID > 10;

        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public SG_NOTE_Signature(int user, int trace, TrajectoryPoint[] points)
        {
            UserID = user;
            TraceID = trace;
            TrajectoryPoints = points;
        }
    }

    public static class SG_NOTE
    {
        private const int WIDTH = 1280; //in mm
        private const int HEIGHT = 1280; //in mm
        private const string DB_PATH = @"..\..\..\Databases\SG_NOTE\SG_NOTE\galaxy_SignatureDB";

        public static IEnumerable<IEnumerable<SG_NOTE_Signature>> getAllSignatures(int nUser = int.MaxValue)
        {
            var userFolders = Directory.GetDirectories(DB_PATH);

            int i = 0;
            foreach (var userFolder in userFolders)
                if (i++ < nUser) yield return readUserFolder(userFolder);
        }

        private static IEnumerable<SG_NOTE_Signature> readUserFolder(string folderPath, bool normalize = true)
        {
            var files = Directory.GetFiles(folderPath, "*.txt");

            foreach (var file in files)
            {
                yield return readSignatureFile(file, normalize);
            }
        }

        private static SG_NOTE_Signature readSignatureFile(string filePath, bool normalize = true)
        {
            var fileNameSplit = Path.GetFileName(filePath).Split('_');
            var userID = int.Parse(fileNameSplit[0]);
            var traceID = int.Parse(fileNameSplit[1]);

            var points = SignatureDatabase.readTrajectoryTextFile(filePath, ' ', 1, true, false, normalize, WIDTH, HEIGHT).ToArray();

            return new SG_NOTE_Signature(userID, traceID, points);
        }
    }
}
