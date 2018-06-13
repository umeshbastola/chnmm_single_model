﻿// <auto-generated />
using ExperimentResultsDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace ExperimentResultsDatabase.Migrations
{
    [DbContext(typeof(ExperimentDB))]
    [Migration("20170911134918_test3")]
    partial class test3
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ExperimentLib.ClassificationResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ClassificationSubsetResultId");

                    b.Property<string>("ClassifiedGesture");

                    b.Property<string>("ExecutedGesture");

                    b.Property<long>("ExecutedTraceID");

                    b.Property<int>("RecognitionTime");

                    b.HasKey("Id");

                    b.HasIndex("ClassificationSubsetResultId");

                    b.ToTable("ClassificationResults");
                });

            modelBuilder.Entity("ExperimentLib.ClassificationSubsetResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id");

                    b.Property<int>("RecognitionTime");

                    b.Property<int>("SubsetIndex");

                    b.Property<int>("TrainTime");

                    b.HasKey("Id");

                    b.HasIndex("CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id");

                    b.ToTable("ClassificationSubsetResults");
                });

            modelBuilder.Entity("ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id");

                    b.Property<int?>("ParameterId");

                    b.Property<int>("RecognitionTime");

                    b.HasKey("Id");

                    b.HasIndex("ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id");

                    b.HasIndex("ParameterId");

                    b.ToTable("CrossValidationClassification");
                });

            modelBuilder.Entity("ExperimentLib.ExperimentDescription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("ExperimentType");

                    b.Property<string>("UsedMethod");

                    b.HasKey("Id");

                    b.ToTable("ExperimentDescriptions");
                });

            modelBuilder.Entity("ExperimentLib.ExperimentResults<ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ExperimentDescriptionId");

                    b.HasKey("Id");

                    b.HasIndex("ExperimentDescriptionId");

                    b.ToTable("CV_ClassificationExperiment");
                });

            modelBuilder.Entity("GestureRecognitionLib.CHnMM.CHnMMParameter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("areaPointDistance");

                    b.Property<string>("distEstName");

                    b.Property<double>("hitProbability");

                    b.Property<bool>("isTranslationInvariant");

                    b.Property<double>("minRadiusArea");

                    b.Property<int>("nAreaForStrokeMap");

                    b.Property<double>("toleranceFactorArea");

                    b.Property<bool>("useAdaptiveTolerance");

                    b.Property<bool>("useContinuousAreas");

                    b.Property<bool>("useEllipsoid");

                    b.Property<bool>("useFixAreaNumber");

                    b.Property<bool>("useSmallestCircle");

                    b.HasKey("Id");

                    b.ToTable("CHnMMParameterSets");
                });

            modelBuilder.Entity("ExperimentLib.ClassificationResult", b =>
                {
                    b.HasOne("ExperimentLib.ClassificationSubsetResult")
                        .WithMany("Results")
                        .HasForeignKey("ClassificationSubsetResultId");
                });

            modelBuilder.Entity("ExperimentLib.ClassificationSubsetResult", b =>
                {
                    b.HasOne("ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>")
                        .WithMany("Results")
                        .HasForeignKey("CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id");
                });

            modelBuilder.Entity("ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>", b =>
                {
                    b.HasOne("ExperimentLib.ExperimentResults<ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>>")
                        .WithMany("Results")
                        .HasForeignKey("ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id");

                    b.HasOne("GestureRecognitionLib.CHnMM.CHnMMParameter", "Parameter")
                        .WithMany()
                        .HasForeignKey("ParameterId");
                });

            modelBuilder.Entity("ExperimentLib.ExperimentResults<ExperimentLib.CrossValidationResult<GestureRecognitionLib.CHnMM.CHnMMParameter, ExperimentLib.ClassificationSubsetResult>>", b =>
                {
                    b.HasOne("ExperimentLib.ExperimentDescription", "ExperimentDescription")
                        .WithMany()
                        .HasForeignKey("ExperimentDescriptionId");
                });
#pragma warning restore 612, 618
        }
    }
}
