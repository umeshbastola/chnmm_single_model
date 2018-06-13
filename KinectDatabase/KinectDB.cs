using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using GestureRecognitionLib;
using System.Diagnostics;

namespace KinectDatabase
{
    using TrajectoryDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public class KinectGesture : BaseTrajectory
    {
        public int GestureID { get; }
        public int UserID { get; }
        public int ShapeID { get; }
        public override TrajectoryPoint[] TrajectoryPoints { get; }

        public KinectGesture(int gid, int uid, int sid, IEnumerable<TrajectoryPoint3D> points)
        {
            GestureID = gid;
            UserID = uid;
            ShapeID = sid;
            TrajectoryPoints = points.ToArray();
        }
    }

    public static class KinectDB
    {
        public const int MAXRANGE_X = 65;
        public const int MAXRANGE_Y = 85;
        public const int MAXRANGE_Z = 320;

        public const int MINRANGE_X = -85;
        public const int MINRANGE_Y = -50;
        public const int MINRANGE_Z = 215;

        public const int RANGE_X = MAXRANGE_X - MINRANGE_X;
        public const int RANGE_Y = MAXRANGE_Y - MINRANGE_Y;
        public const int RANGE_Z = MAXRANGE_Z - MINRANGE_Z;

        public static int[] CIRCLE_SHAPES = { 2, 5, 6, 7 };
        public static int[] TRIANGLE_SHAPES = { 4, 8 };

        public static bool normalizeCoords(ref double x, ref double y, ref double z)
        {
            double max = Math.Max( Math.Max(RANGE_X, RANGE_Y), RANGE_Z );

            x = x / max;
            y = y / max;
            z = z / max;

            if (x > 1.0 || x < 0 || y > 1.0 || y < 0 || z > 1.0 || z < 0)
                return false;

            return true;
        }

        public static IEnumerable<KinectGesture> GetAllGestures(bool normalize=false, bool syntheticTime=true)
        {
            var sql = @"SELECT GestureID, ShapeID, UID, X, Y, Z, SqlTime FROM finaltrain ORDER BY FrameID";
            using (var con = new SqlConnection(Properties.Settings.Default.KinectDBCon))
            {
                con.Open();
                var com = new SqlCommand(sql, con);
                var reader = com.ExecuteReader();

                reader.Read();

                while (true)
                {
                    object[] row = new object[7];
                    var n = reader.GetValues(row);
                    var id = (int)row[0];
                    var sID = (int)row[1];
                    var uID = (int)row[2];

                    var points = GetPointsOfGesture(reader, id, sID, uID, syntheticTime, normalize).ToArray();

                    yield return new KinectGesture(id, uID, sID, points);
                    if (reader.IsClosed) yield break;
                }
            }
        }

        private static IEnumerable<TrajectoryPoint3D> GetPointsOfGesture(SqlDataReader reader, int gid, int sid, int uid, bool syntheticTime = true, bool normalize = true)
        {
            var synTime = 0L;
            do
            {
                object[] row = new object[7];
                var n = reader.GetValues(row);
                var gID = (int)row[0];
                var sID = (int)row[1];
                var uID = (int)row[2];
                var x = (double)row[3];
                var y = (double)row[4];
                var z = (double)row[5];
                var time = (long)row[6];

                if (gID != gid) yield break;

                Debug.Assert(uID == uid && sID == sid);

                if (normalize)
                {
                    normalizeCoords(ref x, ref y, ref z);
                }

                if (syntheticTime)
                {
                    time = synTime;
                    synTime += (1000 / 30);
                }
                yield return new TrajectoryPoint3D(x, y, z, time);

            } while (reader.Read());
            reader.Close();
        }

        public static IEnumerable<TrajectoryDataSet> GetVerificationSet(int nTraining, bool useRandomForgery)
        {
            TrajectoryDataSet trainingSets = new TrajectoryDataSet();
            TrajectoryDataSet genuineSets = new TrajectoryDataSet();
            TrajectoryDataSet forgerySets = new TrajectoryDataSet();

            var allTrajectories = GetAllGestures(true);
            var groupedTrajectories = allTrajectories.GroupBy(g => g.UserID + "_" + g.ShapeID);

            foreach (var userTraces in groupedTrajectories)
            {
                string user = userTraces.Key;
                var split = user.Split('_');
                var userid = int.Parse(split[0]);
                var shapeid = int.Parse(split[1]);

                var trainingTraces = userTraces.Take(nTraining).ToArray();
                var genuineTraces = userTraces.Skip(nTraining).ToArray();

                var forgeryTraces = (!useRandomForgery) ?
                    allTrajectories.Where(g => g.UserID != userid && g.ShapeID == shapeid).ToArray() :
                    groupedTrajectories.Where(grp => IsDifferentShapeTypeAndUser(userid, shapeid, grp.Key)).Select(grp => grp.First()).ToArray();

                trainingSets[user] = trainingTraces;
                genuineSets[user] = genuineTraces;
                forgerySets[user] = forgeryTraces;
            }

            yield return trainingSets;
            yield return genuineSets;
            yield return forgerySets;
        }

        private static bool IsDifferentShapeTypeAndUser(int curUserID, int curShapeID, string otherUser)
        {
            var split = otherUser.Split('_');
            var otherUserID = int.Parse(split[0]);
            var otherShapeID = int.Parse(split[1]);

            if (curUserID == otherUserID) return false;

            var bothCircle = CIRCLE_SHAPES.Contains(curShapeID) && CIRCLE_SHAPES.Contains(otherShapeID);
            var bothTriangle = TRIANGLE_SHAPES.Contains(curShapeID) && TRIANGLE_SHAPES.Contains(otherShapeID);

            return !bothCircle && !bothTriangle;
        }
    }
}
