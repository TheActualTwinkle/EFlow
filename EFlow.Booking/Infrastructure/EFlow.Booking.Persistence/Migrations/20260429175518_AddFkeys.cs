using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFlow.Booking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_submission_slots_subject_id",
                table: "submission_slots",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_slots_teacher_id",
                table: "submission_slots",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_submission_slot_notification_settings_user_id",
                table: "submission_slot_notification_settings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_teacher_id",
                table: "subjects",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_group_id",
                table: "students",
                column: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_students_groups_group_id",
                table: "students",
                column: "group_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_subjects_teachers_teacher_id",
                table: "subjects",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_submission_slot_notification_settings_AspNetUsers_user_id",
                table: "submission_slot_notification_settings",
                column: "user_id",
                principalSchema: "identity",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_submission_slots_subjects_subject_id",
                table: "submission_slots",
                column: "subject_id",
                principalTable: "subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_submission_slots_teachers_teacher_id",
                table: "submission_slots",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_students_groups_group_id",
                table: "students");

            migrationBuilder.DropForeignKey(
                name: "FK_subjects_teachers_teacher_id",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_submission_slot_notification_settings_AspNetUsers_user_id",
                table: "submission_slot_notification_settings");

            migrationBuilder.DropForeignKey(
                name: "FK_submission_slots_subjects_subject_id",
                table: "submission_slots");

            migrationBuilder.DropForeignKey(
                name: "FK_submission_slots_teachers_teacher_id",
                table: "submission_slots");

            migrationBuilder.DropIndex(
                name: "IX_submission_slots_subject_id",
                table: "submission_slots");

            migrationBuilder.DropIndex(
                name: "IX_submission_slots_teacher_id",
                table: "submission_slots");

            migrationBuilder.DropIndex(
                name: "IX_submission_slot_notification_settings_user_id",
                table: "submission_slot_notification_settings");

            migrationBuilder.DropIndex(
                name: "IX_subjects_teacher_id",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_students_group_id",
                table: "students");
        }
    }
}
