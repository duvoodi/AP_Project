using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AP_Project.Migrations
{
    /// <inheritdoc />
    public partial class MakeTeachesSectionOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teaches_Instructors_InstructorUserId",
                table: "Teaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teaches",
                table: "Teaches");

            migrationBuilder.DropIndex(
                name: "IX_Teaches_SectionId",
                table: "Teaches");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstructorUserId",
                table: "Teaches",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teaches",
                table: "Teaches",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_InstructorUserId",
                table: "Teaches",
                column: "InstructorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teaches_Instructors_InstructorUserId",
                table: "Teaches",
                column: "InstructorUserId",
                principalTable: "Instructors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teaches_Instructors_InstructorUserId",
                table: "Teaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Teaches",
                table: "Teaches");

            migrationBuilder.DropIndex(
                name: "IX_Teaches_InstructorUserId",
                table: "Teaches");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstructorUserId",
                table: "Teaches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teaches",
                table: "Teaches",
                columns: new[] { "InstructorUserId", "SectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_SectionId",
                table: "Teaches",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teaches_Instructors_InstructorUserId",
                table: "Teaches",
                column: "InstructorUserId",
                principalTable: "Instructors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
