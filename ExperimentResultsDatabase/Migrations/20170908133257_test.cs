using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ExperimentResultsDatabase.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHnMMParameterSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    areaPointDistance = table.Column<double>(type: "float", nullable: false),
                    distEstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hitProbability = table.Column<double>(type: "float", nullable: false),
                    isTranslationInvariant = table.Column<bool>(type: "bit", nullable: false),
                    minRadiusArea = table.Column<double>(type: "float", nullable: false),
                    nAreaForStrokeMap = table.Column<int>(type: "int", nullable: false),
                    toleranceFactorArea = table.Column<double>(type: "float", nullable: false),
                    useAdaptiveTolerance = table.Column<bool>(type: "bit", nullable: false),
                    useContinuousAreas = table.Column<bool>(type: "bit", nullable: false),
                    useEllipsoid = table.Column<bool>(type: "bit", nullable: false),
                    useFixAreaNumber = table.Column<bool>(type: "bit", nullable: false),
                    useSmallestCircle = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHnMMParameterSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperimentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Finished = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedMethod = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CV_ClassificationExperiment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExperimentDescriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CV_ClassificationExperiment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CV_ClassificationExperiment_ExperimentDescriptions_ExperimentDescriptionId",
                        column: x => x.ExperimentDescriptionId,
                        principalTable: "ExperimentDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrossValidationClassification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExperimentResultsCrossValidationResultCHnMMParameterClassificationSubsetResultId = table.Column<int>(name: "ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id", type: "int", nullable: true),
                    ParameterId = table.Column<int>(type: "int", nullable: true),
                    RecognitionTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossValidationClassification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experiment_Id",
                        column: x => x.ExperimentResultsCrossValidationResultCHnMMParameterClassificationSubsetResultId,
                        principalTable: "CV_ClassificationExperiment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrossValidationClassification_CHnMMParameterSets_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "CHnMMParameterSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassificationSubsetResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CrossValidationResultCHnMMParameterClassificationSubsetResultId = table.Column<int>(name: "CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id", type: "int", nullable: true),
                    RecognitionTime = table.Column<int>(type: "int", nullable: false),
                    TrainTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationSubsetResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrossValidationClassification_Id",
                        column: x => x.CrossValidationResultCHnMMParameterClassificationSubsetResultId,
                        principalTable: "CrossValidationClassification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassificationResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClassificationSubsetResultId = table.Column<int>(type: "int", nullable: true),
                    ClassifiedGesture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedGesture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedTraceID = table.Column<long>(type: "bigint", nullable: false),
                    RecognitionTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassificationResults_ClassificationSubsetResults_ClassificationSubsetResultId",
                        column: x => x.ClassificationSubsetResultId,
                        principalTable: "ClassificationSubsetResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationResults_ClassificationSubsetResultId",
                table: "ClassificationResults",
                column: "ClassificationSubsetResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationSubsetResults_CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id",
                table: "ClassificationSubsetResults",
                column: "CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>Id");

            migrationBuilder.CreateIndex(
                name: "IX_CrossValidationClassification_ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id",
                table: "CrossValidationClassification",
                column: "ExperimentResults<CrossValidationResult<CHnMMParameter, ClassificationSubsetResult>>Id");

            migrationBuilder.CreateIndex(
                name: "IX_CrossValidationClassification_ParameterId",
                table: "CrossValidationClassification",
                column: "ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_CV_ClassificationExperiment_ExperimentDescriptionId",
                table: "CV_ClassificationExperiment",
                column: "ExperimentDescriptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificationResults");

            migrationBuilder.DropTable(
                name: "ClassificationSubsetResults");

            migrationBuilder.DropTable(
                name: "CrossValidationClassification");

            migrationBuilder.DropTable(
                name: "CV_ClassificationExperiment");

            migrationBuilder.DropTable(
                name: "CHnMMParameterSets");

            migrationBuilder.DropTable(
                name: "ExperimentDescriptions");
        }
    }
}
