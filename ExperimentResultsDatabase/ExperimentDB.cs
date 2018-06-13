using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExperimentLib;
using GestureRecognitionLib.CHnMM;

namespace ExperimentResultsDatabase
{
    public class ExperimentDB: DbContext
    {
        public DbSet<ClassificationResult> ClassificationResults { get; set; }
        public DbSet<ClassificationSubsetResult> ClassificationSubsetResults { get; set; }
        public DbSet<CrossValidationResult<CHnMMParameter,ClassificationSubsetResult>> CrossValidationClassification { get; set; }
        public DbSet<ExperimentDescription> ExperimentDescriptions { get; set; }
        public DbSet<ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>> CV_ClassificationExperiment { get; set; }
        public DbSet<CHnMMParameter> CHnMMParameterSets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=ACERLAPTOP;Initial Catalog=ExperimentResults;Integrated Security=True");
            }
            base.OnConfiguring(optionsBuilder);
        }
    }

    public static class DBFunctions
    {
        public static void SaveResultsToDatabase(ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>> expResult)
        {
            var now = DateTime.Now;

            using (var db = new ExperimentDB())
            {
                //find existing parameterSets
                foreach(var result in expResult.Results)
                {
                    var paramSet = result.Parameter;
                    var dbParamSet = GetOrAddCHnMMParameterSet(db, paramSet);
                    result.Parameter = dbParamSet;
                }

                db.CV_ClassificationExperiment.Add(expResult);

                db.SaveChanges();
            }
        }

        public static CHnMMParameter GetOrAddCHnMMParameterSet(CHnMMParameter set)
        {
            using (var db = new ExperimentDB())
            {
                return GetOrAddCHnMMParameterSet(db, set);
            }
        }

        private static CHnMMParameter GetOrAddCHnMMParameterSet(ExperimentDB db, CHnMMParameter set)
        {
            var dbSet = db.CHnMMParameterSets.FirstOrDefault(ps => ps.IsIdentical(set));
            if (dbSet == null)
            {
                db.CHnMMParameterSets.Add(set);
                return set;
            }
            else
            {
                return dbSet;
            }
        }
    }

}
