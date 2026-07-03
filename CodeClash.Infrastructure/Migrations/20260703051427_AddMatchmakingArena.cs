using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeClash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingArena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchArenas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerOneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerTwoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerOneRatingBefore = table.Column<int>(type: "int", nullable: false),
                    PlayerTwoRatingBefore = table.Column<int>(type: "int", nullable: false),
                    PlayerOneRatingAfter = table.Column<int>(type: "int", nullable: true),
                    PlayerTwoRatingAfter = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WinnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchArenas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchArenas_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchArenas_Users_PlayerOneId",
                        column: x => x.PlayerOneId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchArenas_Users_PlayerTwoId",
                        column: x => x.PlayerTwoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchmakingQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Elo = table.Column<int>(type: "int", nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSearching = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchmakingQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchmakingQueues_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProblemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RatingBefore = table.Column<int>(type: "int", nullable: false),
                    RatingAfter = table.Column<int>(type: "int", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchHistories_MatchArenas_MatchId",
                        column: x => x.MatchId,
                        principalTable: "MatchArenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchHistories_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchHistories_Users_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatchSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchArenaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestCasesPassed = table.Column<int>(type: "int", nullable: false),
                    TotalTestCases = table.Column<int>(type: "int", nullable: false),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    MemoryUsedMb = table.Column<long>(type: "bigint", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchSubmissions_MatchArenas_MatchArenaId",
                        column: x => x.MatchArenaId,
                        principalTable: "MatchArenas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchSubmissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_P1_Status",
                table: "MatchArenas",
                columns: new[] { "PlayerOneId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_P2_Status",
                table: "MatchArenas",
                columns: new[] { "PlayerTwoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_PlayerOneId",
                table: "MatchArenas",
                column: "PlayerOneId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_PlayerTwoId",
                table: "MatchArenas",
                column: "PlayerTwoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_ProblemId",
                table: "MatchArenas",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_Status",
                table: "MatchArenas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MatchArenas_WinnerId",
                table: "MatchArenas",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_MatchId",
                table: "MatchHistories",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_OpponentId",
                table: "MatchHistories",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_ProblemId",
                table: "MatchHistories",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_UserId_PlayedAt",
                table: "MatchHistories",
                columns: new[] { "UserId", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_UserId_Result",
                table: "MatchHistories",
                columns: new[] { "UserId", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_Searching_Difficulty_Elo",
                table: "MatchmakingQueues",
                columns: new[] { "IsSearching", "Difficulty", "Elo" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchmakingQueues_UserId_IsSearching",
                table: "MatchmakingQueues",
                columns: new[] { "UserId", "IsSearching" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSubmissions_ArenaId_IsAccepted",
                table: "MatchSubmissions",
                columns: new[] { "MatchArenaId", "IsAccepted" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSubmissions_ArenaId_UserId",
                table: "MatchSubmissions",
                columns: new[] { "MatchArenaId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchSubmissions_UserId",
                table: "MatchSubmissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchHistories");

            migrationBuilder.DropTable(
                name: "MatchmakingQueues");

            migrationBuilder.DropTable(
                name: "MatchSubmissions");

            migrationBuilder.DropTable(
                name: "MatchArenas");
        }
    }
}
