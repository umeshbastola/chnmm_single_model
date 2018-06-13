using System;
using System.Data;
using System.Collections.Generic;
using Npgsql;
using GestureRecognitionLib;
using GestureRecognitionLib.CHnMM;
using ExperimentLib;
using GestureRecognitionLib.CHnMM.Estimators;
using System.Linq;
using System.IO;

namespace MultiStrokeGestureRecognitionLib
{
    public class Program
    {
        public static void Main()
        {
            var param1 = new IntParamVariation("nAreaForStrokeMap", 10, 5, 20);
            var param2 = new DoubleParamVariation("minRadiusArea", 0.01);
            var param3 = new DoubleParamVariation("toleranceFactorArea", 1.7, 0.2, 2.1);
            var param5 = new BoolParamVariation("useFixAreaNumber", true);
            var param6 = new BoolParamVariation("useSmallestCircle", true);
            var param7 = new BoolParamVariation("isTranslationInvariant", false);
            var param8 = new BoolParamVariation("useAdaptiveTolerance", false);
            var param9 = new DoubleParamVariation("hitProbability", 0.9);
            var param10 = new StringParamVariation("distEstName", new string[]
            {
                nameof(NaiveUniformEstimator),
                nameof(NormalEstimator)
            });

            var param11 = new BoolParamVariation("useEllipsoid", false);
            var configSet = ParameterVariation.getParameterVariations(param1, param2, param3, param5, param6, param7, param8, param9, param10, param11).Select(ps => new CHnMMParameter(ps)).ToArray();
            string ConnectionString = "Server=localhost; Port=5432; User Id=macbook; Database = touchy_data_development";
            string ConnectionString_heroku = "Database=dcbpejtem8e4qu; Server=ec2-54-75-239-237.eu-west-1.compute.amazonaws.com; Port=5432; User Id=pbcgcsyjsmpeds; Password=323743a3eec80c0a49dcee493617af7b94fee458a6a89a671dc3acaad0c3f437; Sslmode=Require;Trust Server Certificate=true";
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            NpgsqlCommand command = connection.CreateCommand();
            //#########################################################
            //RECOGNITION TASK//
            //#########################################################

            command.CommandText = "SELECT id FROM Tgestures";
            NpgsqlDataReader ges_read = command.ExecuteReader();
            DataTable gesture_table = new DataTable();
            gesture_table.Load(ges_read);
            ges_read.Close();
            var set = configSet[0];
            //foreach (var set in configSet)
            //{
            bool head = true;
            var file_name = "../../recognition/";
            file_name += "tol_" + set.toleranceFactorArea;
            file_name += "_dist_" + set.distEstName;
            file_name += "_nArea_" + set.nAreaForStrokeMap;
            file_name += ".csv";
            Console.WriteLine(file_name);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            CHnMMClassificationSystem cs = new CHnMMClassificationSystem(set);
            for (int global_user = 1; global_user < 12; global_user++)
            {
                foreach (DataRow gesture in gesture_table.Rows)
                {
                    List<StrokeData> gestureStrokes = new List<StrokeData>();
                    var accumulate = new List<KeyValuePair<int, string[]>>();
                    command.CommandText = "SELECT * FROM trajectories WHERE user_id = "+ global_user + " AND gesture_id="+ gesture["id"] +" AND exec_num % 2 = 1 ORDER BY exec_num, stroke_seq;";
                    NpgsqlDataReader reader = command.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    reader.Close();
                    var prev_exec = 1;
                    var prev_stroke = 0;
                    var time_lapse = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if(prev_exec != Convert.ToInt32(row["exec_num"])){
                            var trajectory = new StrokeData(global_user, accumulate);
                            gestureStrokes.Add(trajectory);
                            prev_exec = Convert.ToInt32(row["exec_num"]);
                            time_lapse = 0;
                            accumulate.Clear();
                        }

                        string[,] db_points = row["points"] as string[,];
                        int trace = Convert.ToInt32(row["stroke_seq"]);
                        int rowLength = db_points.GetLength(0);
                        if (prev_stroke != trace)
                        {
                            time_lapse = Convert.ToInt32(accumulate.Last().Value[2]);
                        }
                        for (int i = 0; i < rowLength; i++)
                        {
                            string[] single_pt = new string[4];
                            single_pt[0] = db_points[i, 0];
                            single_pt[1] = db_points[i, 1];
                            single_pt[2] = (Convert.ToInt32(db_points[i, 2])+time_lapse).ToString();
                            single_pt[3] = trace.ToString();
                            accumulate.Add(new KeyValuePair<int, string[]>(Convert.ToInt32(db_points[i,2]),single_pt));
                        }
                    }
                    if (accumulate.Count > 0)
                    {
                        var last_trajectory = new StrokeData(global_user, accumulate);
                        gestureStrokes.Add(last_trajectory);
                    }
                    var gestureName = global_user + "-" + gesture["id"];
                    cs.trainGesture(gestureName, gestureStrokes.Cast<BaseTrajectory>());
                    Console.WriteLine("=============================================================================\n");
                    Console.WriteLine("=============================================================================");
                }

            }
            for (int global_user = 1; global_user < 12; global_user++)
            {
                Console.WriteLine("User=========================================================" + global_user);
                foreach (DataRow gesture in gesture_table.Rows)
                {
                    var accumulate = new List<KeyValuePair<int, string[]>>();
                    command.CommandText = "SELECT * FROM trajectories WHERE user_id = " + global_user + " AND gesture_id=" + gesture["id"] + " AND exec_num % 2 = 0 ORDER BY exec_num, stroke_seq;";
                    NpgsqlDataReader reader = command.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    reader.Close();
                    var prev_exec = 2;
                    var prev_stroke = 0;
                    var time_lapse = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (prev_exec != Convert.ToInt32(row["exec_num"]))
                        {
                            var trajectory = new StrokeData(global_user, accumulate);
                            cs.recognizeGesture(trajectory);
                            prev_exec = Convert.ToInt32(row["exec_num"]);
                            time_lapse = 0;
                            accumulate.Clear();
                        }

                        string[,] db_points = row["points"] as string[,];
                        int trace = Convert.ToInt32(row["stroke_seq"]);
                        int rowLength = db_points.GetLength(0);
                        if (prev_stroke != trace)
                        {
                            time_lapse = Convert.ToInt32(accumulate.Last().Value[2]);
                        }
                        for (int i = 0; i < rowLength; i++)
                        {
                            string[] single_pt = new string[4];
                            single_pt[0] = db_points[i, 0];
                            single_pt[1] = db_points[i, 1];
                            single_pt[2] = (Convert.ToInt32(db_points[i, 2]) + time_lapse).ToString();
                            single_pt[3] = trace.ToString();
                            accumulate.Add(new KeyValuePair<int, string[]>(Convert.ToInt32(db_points[i, 2]), single_pt));
                        }
                    }
                    if (accumulate.Count > 0)
                    {
                        var last_trajectory = new StrokeData(global_user, accumulate);
                        var result = cs.recognizeGesture(last_trajectory);
                        if (result == null)
                            Console.WriteLine(gesture["id"]+ ": Not Recognized");
                        else 
                            Console.WriteLine(gesture["id"] + ": "+result);
                    }
                }
            }
        }
    }
}