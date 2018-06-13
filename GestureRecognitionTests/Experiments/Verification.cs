using GestureRecognitionLib;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using GestureRecognitionLib.CHnMM;
using System.IO;

namespace LfS.GestureRecognitionTests.Experiments
{
    using System;
    using GestureDataSet = Dictionary<string, IEnumerable<BaseTrajectory>>;

    public static class VerificationResults
    {
        public class BasicResult
        {
            public double FAR { get; }
            public double FRR { get; }
            //public Configuration Config { get; }

            public BasicResult(double far, double frr)
            {
                FAR = far;
                FRR = frr;
            }

            public static string getCSVHead()
            {
                return $"FAR;FRR;";
            }

            public string getCSVData()
            {
                return $"{FAR:F2}; {FRR:F2}";
            }
        }

        public class SingleVerificationResult
        {
            public string SupposedTrajectory { get; }
            public bool IsForgery { get; }

            public double EvaluationScore { get; }

            public static string getCSVHead()
            {
                return $"Executed;IsForgery;Score;";
            }

            public string getCSVData()
            {
                return $"{SupposedTrajectory}; {IsForgery}; {EvaluationScore}";
            }

            public SingleVerificationResult(string supposed, bool isForgery, double score)
            {
                SupposedTrajectory = supposed;
                IsForgery = isForgery;
                EvaluationScore = score;
            }
        }

        public class ScoringResult: BasicResult
        {
            public SingleVerificationResult[] VerificationResults { get; }

            public ScoringResult(SingleVerificationResult[] results, double far, double frr) : base(far, frr)
            {
                VerificationResults = results;
            }
        }

        public static void saveResultsToFile(string file, IEnumerable<SingleVerificationResult> results)
        {
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(SingleVerificationResult.getCSVHead());

            foreach (var result in results)
            {
                sw.WriteLine(result.getCSVData());
            }
            sw.Close();
        }

        public static void saveResultsToFile(string file, IEnumerable<CHnMMParameter> configs, IEnumerable<BasicResult> results)
        {
            Debug.Assert(configs.Count() == results.Count());
            var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(stream);

            sw.WriteLine(CHnMMParameter.getCSVHeaders() + ";" + BasicResult.getCSVHead());

            foreach (var result in configs.Zip(results, (c,r) => new {Config = c, Result = r }))
            {
                sw.WriteLine(result.Config.getCSVValues() + ";" + result.Result.getCSVData());
            }
            sw.Close();
        }
    }

    public static class VerificationExperiment
    {
        public static VerificationResults.BasicResult DoVerification(IVerificationSystem verifier, GestureDataSet trainingSet, GestureDataSet genuineSet, GestureDataSet forgerySet, bool getScores = false)
        {
            Stopwatch swTrain = new Stopwatch();
            //train recognition system
            verifier.clearGestures();
            swTrain.Restart();
            foreach (var e in trainingSet)
            {
                verifier.trainGesture(e.Key, e.Value);
            }
            swTrain.Stop();

            int nFalseAccepts = 0;
            int nFalseRejects = 0;

            int nGenuineAttempts = 0;

            var scores = new LinkedList<VerificationResults.SingleVerificationResult>();

            //test genuine
            foreach (var e in genuineSet)
            {
                foreach (var trace in e.Value)
                {
                    bool userVerified;
                    double score;
                    if (getScores)
                    {
                        userVerified = verifier.verifyGesture(e.Key, trace, out score);
                        var res = new VerificationResults.SingleVerificationResult(e.Key, false, score);
                        scores.AddLast(res);
                    }
                    else
                    {
                        userVerified = verifier.verifyGesture(e.Key, trace);
                    }
                    if (!userVerified) nFalseRejects++;
                    nGenuineAttempts++;
                }
            }

            int nForgeryAttempts = 0;
            //test forgeries
            foreach (var e in forgerySet)
            {
                foreach (var trace in e.Value)
                {
                    bool userVerified;
                    double score;
                    if (getScores)
                    {
                        userVerified = verifier.verifyGesture(e.Key, trace, out score);
                        var res = new VerificationResults.SingleVerificationResult(e.Key, true, score);
                        scores.AddLast(res);
                    }
                    else
                    {
                        userVerified = verifier.verifyGesture(e.Key, trace);
                    }
                    if (userVerified) nFalseAccepts++;
                    nForgeryAttempts++;
                }
            }

            double FAR = (double)nFalseAccepts / nForgeryAttempts;
            double FRR = (double)nFalseRejects / nGenuineAttempts;

            if (getScores)
                return new VerificationResults.ScoringResult(scores.ToArray(), FAR, FRR);
            else
                return new VerificationResults.BasicResult(FAR, FRR);
        }
    }

    public class CHnMMVerificationExperiment
    {
        private string dataSourceName;
        private int nTraining;
        private bool? session2;
        private CHnMMParameter[] configs;
        private bool writeScores;
        private int? nUser;

        private GestureDataSet trainingSet;
        private GestureDataSet genuineSet;
        private GestureDataSet forgerySet;

        public CHnMMVerificationExperiment(string setName, int nTraining, bool? session2, int? nUser, CHnMMParameter config, bool writeScores=false)
            : this(setName, nTraining, session2, nUser, new CHnMMParameter[] { config }, writeScores) { }

        public CHnMMVerificationExperiment(string setName, int nTraining, bool? session2, int? nUser, CHnMMParameter[] configs, bool writeScores = false)
        {
            this.dataSourceName = setName;
            this.nTraining = nTraining;
            this.configs = configs;
            this.session2 = session2;
            this.writeScores = writeScores;
            this.nUser = nUser;

            var allSets = DataSets.getVerificationDataSet(setName, nTraining, session2, nUser);

            trainingSet = allSets.First();
            genuineSet = allSets.Skip(1).First();
            forgerySet = allSets.Skip(2).First();
        }

        public void execute()
        {
            var results = new LinkedList<VerificationResults.BasicResult>();
            foreach (var config in configs)
            {
                var chnmmRec = new CHnMMClassificationSystem(config);
                var res = VerificationExperiment.DoVerification(chnmmRec, trainingSet, genuineSet, forgerySet, writeScores);
                results.AddLast(res);
            }

            var txtSession = "";
            if(session2.HasValue) txtSession = (session2.Value) ? "_Session2" : "_Session1";
            var txtUser = nUser.HasValue ? $"_nUsers{nUser}" : "";
            string basePath = "..\\..\\ExperimentResults\\";
            string expName = $"Verification_CHnMM_{dataSourceName}_{nTraining}trainingTraces{txtSession}{txtUser}_{DateTime.Now.ToFileTime()}";
            string dirPath = $"{basePath}{expName}";
            string filePath = $"{dirPath}.csv";
            VerificationResults.saveResultsToFile(filePath, configs, results);

            if(writeScores)
            {
                Directory.CreateDirectory(dirPath);

                foreach (var confRes in configs.Zip(results, (c, r) => new { Config = c, Result = (VerificationResults.ScoringResult) r }))
                {
                    string fileName = dirPath + "\\" + confRes.Config.getCSVValues().Replace(';','_') + ".csv";
                    VerificationResults.saveResultsToFile(fileName, confRes.Result.VerificationResults);
                }
            }
        }
    }
}