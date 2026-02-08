using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithAllEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompanyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissioningMarkets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissioningMarkets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AiPrompt = table.Column<string>(type: "text", nullable: true),
                    RuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldworkMarkets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldworkMarkets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ParentModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Modules_ParentModuleId",
                        column: x => x.ParentModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VariableName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuestionText = table.Column<string>(type: "text", nullable: true),
                    Classification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDummy = table.Column<bool>(type: "boolean", nullable: false),
                    QuestionTitle = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Methodology = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataQualityTag = table.Column<string>(type: "text", nullable: true),
                    RowSortOrder = table.Column<int>(type: "integer", nullable: true),
                    ColumnSortOrder = table.Column<int>(type: "integer", nullable: true),
                    AnswerMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerMax = table.Column<int>(type: "integer", nullable: true),
                    QuestionFormatDetails = table.Column<string>(type: "text", nullable: true),
                    ScraperNotes = table.Column<string>(type: "text", nullable: true),
                    CustomNotes = table.Column<string>(type: "text", nullable: true),
                    MetricGroup = table.Column<string>(type: "text", nullable: true),
                    TableTitle = table.Column<string>(type: "text", nullable: true),
                    QuestionRationale = table.Column<string>(type: "text", nullable: true),
                    SingleOrMulticode = table.Column<string>(type: "text", nullable: true),
                    ManagedListReferences = table.Column<string>(type: "text", nullable: true),
                    IsTranslatable = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsQuestionActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsQuestionOutOfUse = table.Column<bool>(type: "boolean", nullable: false),
                    AnswerRestrictionMin = table.Column<int>(type: "integer", nullable: true),
                    AnswerRestrictionMax = table.Column<int>(type: "integer", nullable: true),
                    RestrictionDataType = table.Column<string>(type: "text", nullable: true),
                    RestrictedToClient = table.Column<string>(type: "text", nullable: true),
                    AnswerTypeCode = table.Column<string>(type: "text", nullable: true),
                    IsAnswerRequired = table.Column<bool>(type: "boolean", nullable: false),
                    ScalePoint = table.Column<string>(type: "text", nullable: true),
                    ScaleType = table.Column<string>(type: "text", nullable: true),
                    DisplayType = table.Column<string>(type: "text", nullable: true),
                    InstructionText = table.Column<string>(type: "text", nullable: true),
                    ParentQuestionId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionFacet = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionBankItems_QuestionBankItems_ParentQuestionId",
                        column: x => x.ParentQuestionId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConfigurationQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationAnswers_ConfigurationQuestions_ConfigurationQu~",
                        column: x => x.ConfigurationQuestionId,
                        principalTable: "ConfigurationQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductConfigQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigurationQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductConfigQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductConfigQuestions_ConfigurationQuestions_Configuration~",
                        column: x => x.ConfigurationQuestionId,
                        principalTable: "ConfigurationQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductConfigQuestions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTemplates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleQuestions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleQuestions_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AnswerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AnswerLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    IsFixed = table.Column<bool>(type: "boolean", nullable: false),
                    IsExclusive = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CustomProperty = table.Column<string>(type: "text", nullable: true),
                    Facets = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswers_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DependencyRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ConfigurationQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeringAnswerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Classification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuestionBank = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatusReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependencyRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DependencyRules_ConfigurationAnswers_TriggeringAnswerId",
                        column: x => x.TriggeringAnswerId,
                        principalTable: "ConfigurationAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DependencyRules_ConfigurationQuestions_ConfigurationQuestio~",
                        column: x => x.ConfigurationQuestionId,
                        principalTable: "ConfigurationQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductConfigQuestionDisplayRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductConfigQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeringConfigurationQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeringAnswerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayCondition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductConfigQuestionDisplayRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductConfigQuestionDisplayRules_ConfigurationAnswers_Trig~",
                        column: x => x.TriggeringAnswerId,
                        principalTable: "ConfigurationAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductConfigQuestionDisplayRules_ConfigurationQuestions_Tr~",
                        column: x => x.TriggeringConfigurationQuestionId,
                        principalTable: "ConfigurationQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductConfigQuestionDisplayRules_ProductConfigQuestions_Pr~",
                        column: x => x.ProductConfigQuestionId,
                        principalTable: "ProductConfigQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTemplateLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IncludeByDefault = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionBankItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTemplateLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductTemplateLines_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductTemplateLines_ProductTemplates_ProductTemplateId",
                        column: x => x.ProductTemplateId,
                        principalTable: "ProductTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTemplateLines_QuestionBankItems_QuestionBankItemId",
                        column: x => x.QuestionBankItemId,
                        principalTable: "QuestionBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccountName",
                table: "Clients",
                column: "AccountName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissioningMarkets_IsoCode",
                table: "CommissioningMarkets",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationAnswers_ConfigurationQuestionId",
                table: "ConfigurationAnswers",
                column: "ConfigurationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyRules_ConfigurationQuestionId",
                table: "DependencyRules",
                column: "ConfigurationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DependencyRules_TriggeringAnswerId",
                table: "DependencyRules",
                column: "TriggeringAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldworkMarkets_IsoCode",
                table: "FieldworkMarkets",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleQuestions_ModuleId_QuestionBankItemId",
                table: "ModuleQuestions",
                columns: new[] { "ModuleId", "QuestionBankItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleQuestions_QuestionBankItemId",
                table: "ModuleQuestions",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_ParentModuleId",
                table: "Modules",
                column: "ParentModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_VariableName",
                table: "Modules",
                column: "VariableName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigQuestionDisplayRules_ProductConfigQuestionId",
                table: "ProductConfigQuestionDisplayRules",
                column: "ProductConfigQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigQuestionDisplayRules_TriggeringAnswerId",
                table: "ProductConfigQuestionDisplayRules",
                column: "TriggeringAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigQuestionDisplayRules_TriggeringConfigurationQu~",
                table: "ProductConfigQuestionDisplayRules",
                column: "TriggeringConfigurationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigQuestions_ConfigurationQuestionId",
                table: "ProductConfigQuestions",
                column: "ConfigurationQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductConfigQuestions_ProductId_ConfigurationQuestionId",
                table: "ProductConfigQuestions",
                columns: new[] { "ProductId", "ConfigurationQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTemplateLines_ModuleId",
                table: "ProductTemplateLines",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTemplateLines_ProductTemplateId",
                table: "ProductTemplateLines",
                column: "ProductTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTemplateLines_QuestionBankItemId",
                table: "ProductTemplateLines",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTemplates_ProductId_Name_Version",
                table: "ProductTemplates",
                columns: new[] { "ProductId", "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswers_QuestionBankItemId",
                table: "QuestionAnswers",
                column: "QuestionBankItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ParentQuestionId",
                table: "QuestionBankItems",
                column: "ParentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_VariableName_Version",
                table: "QuestionBankItems",
                columns: new[] { "VariableName", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "CommissioningMarkets");

            migrationBuilder.DropTable(
                name: "DependencyRules");

            migrationBuilder.DropTable(
                name: "FieldworkMarkets");

            migrationBuilder.DropTable(
                name: "ModuleQuestions");

            migrationBuilder.DropTable(
                name: "ProductConfigQuestionDisplayRules");

            migrationBuilder.DropTable(
                name: "ProductTemplateLines");

            migrationBuilder.DropTable(
                name: "QuestionAnswers");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "ConfigurationAnswers");

            migrationBuilder.DropTable(
                name: "ProductConfigQuestions");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "ProductTemplates");

            migrationBuilder.DropTable(
                name: "QuestionBankItems");

            migrationBuilder.DropTable(
                name: "ConfigurationQuestions");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
