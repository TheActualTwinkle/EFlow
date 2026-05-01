using System;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFlow.Booking.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260502021500_UseGroupJoinTables")]
    public partial class UseGroupJoinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subject_groups",
                columns: table => new
                {
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_groups", x => new { x.subject_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_subject_groups_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subject_groups_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submission_slot_allowed_groups",
                columns: table => new
                {
                    submission_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submission_slot_allowed_groups", x => new { x.submission_slot_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_submission_slot_allowed_groups_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submission_slot_allowed_groups_submission_slots_slot_id",
                        column: x => x.submission_slot_id,
                        principalTable: "submission_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO subject_groups (subject_id, group_id)
                SELECT subject.id, group_id
                FROM subjects AS subject
                CROSS JOIN LATERAL unnest(subject.group_ids) AS group_id
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO submission_slot_allowed_groups (submission_slot_id, group_id)
                SELECT slot.id, group_id
                FROM submission_slots AS slot
                CROSS JOIN LATERAL unnest(slot.allowed_group_ids) AS group_id
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_subject_groups_group_id",
                table: "subject_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_submission_slot_allowed_groups_group_id",
                table: "submission_slot_allowed_groups",
                column: "group_id");

            migrationBuilder.DropColumn(
                name: "group_ids",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "allowed_group_ids",
                table: "submission_slots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid[]>(
                name: "group_ids",
                table: "subjects",
                type: "uuid[]",
                nullable: false,
                defaultValue: Array.Empty<Guid>());

            migrationBuilder.AddColumn<Guid[]>(
                name: "allowed_group_ids",
                table: "submission_slots",
                type: "uuid[]",
                nullable: false,
                defaultValue: Array.Empty<Guid>());

            migrationBuilder.Sql(
                """
                UPDATE subjects AS subject
                SET group_ids = grouped.group_ids
                FROM (
                    SELECT subject_id, array_agg(group_id ORDER BY group_id) AS group_ids
                    FROM subject_groups
                    GROUP BY subject_id
                ) AS grouped
                WHERE subject.id = grouped.subject_id;
                """);

            migrationBuilder.Sql(
                """
                UPDATE submission_slots AS slot
                SET allowed_group_ids = grouped.group_ids
                FROM (
                    SELECT submission_slot_id, array_agg(group_id ORDER BY group_id) AS group_ids
                    FROM submission_slot_allowed_groups
                    GROUP BY submission_slot_id
                ) AS grouped
                WHERE slot.id = grouped.submission_slot_id;
                """);

            migrationBuilder.DropTable(
                name: "subject_groups");

            migrationBuilder.DropTable(
                name: "submission_slot_allowed_groups");
        }
    }
}
