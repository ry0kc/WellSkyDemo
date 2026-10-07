using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReferralDemo.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                columns: table => new
                {
                    patient_id = table.Column<string>(type: "text", nullable: false),
                    provider = table.Column<string>(type: "text", nullable: false),
                    next_step = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referrals", x => x.patient_id);
                    table.ForeignKey(
                        name: "fk_referrals_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "patients",
                columns: new[] { "id", "name", "status" },
                values: new object[,]
                {
                    { "P001", "Alice Smith", "in_care" },
                    { "P002", "Robert Smith", "discharged" },
                    { "P003", "Maria Gonzalez", "in_care" },
                    { "P004", "James Smith", "pending_admission" }
                });

            migrationBuilder.InsertData(
                table: "referrals",
                columns: new[] { "patient_id", "due_date", "next_step", "provider" },
                values: new object[,]
                {
                    { "P001", new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Initial assessment scheduled", "Riverside Home Health" },
                    { "P003", new DateTime(2026, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Awaiting insurance authorization", "Sunrise Physical Therapy" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referrals");

            migrationBuilder.DropTable(
                name: "visits");

            migrationBuilder.DropTable(
                name: "patients");
        }
    }
}
