using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASPnet_Jobtastic.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedJobPostingTable_JobSharingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Salary = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyWebsite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    OwnerUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowedUserNames = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobSharings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobPostingId = table.Column<int>(type: "int", nullable: false),
                    SharedUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SharedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SharedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSharings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSharings_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSharings_JobPostingId",
                table: "JobSharings",
                column: "JobPostingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSharings");

            migrationBuilder.DropTable(
                name: "JobPostings");
        }
    }
}
