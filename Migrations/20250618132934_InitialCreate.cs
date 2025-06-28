using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AP_Project.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Building = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomNumber = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashedPassword = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinalExamDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_CourseCodes_CodeId",
                        column: x => x.CodeId,
                        principalTable: "CourseCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    AdminId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    HireYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instructors_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prerequisites",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrerequisiteCourseCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prerequisites", x => new { x.CourseId, x.PrerequisiteCourseCodeId });
                    table.ForeignKey(
                        name: "FK_Prerequisites_CourseCodes_PrerequisiteCourseCodeId",
                        column: x => x.PrerequisiteCourseCodeId,
                        principalTable: "CourseCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prerequisites_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Semester = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimeSlotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sections_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sections_TimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Takes",
                columns: table => new
                {
                    StudentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Takes", x => new { x.StudentUserId, x.SectionId });
                    table.ForeignKey(
                        name: "FK_Takes_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Takes_Students_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teaches",
                columns: table => new
                {
                    InstructorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teaches", x => new { x.InstructorUserId, x.SectionId });
                    table.ForeignKey(
                        name: "FK_Teaches_Instructors_InstructorUserId",
                        column: x => x.InstructorUserId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teaches_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "admin" },
                    { 2, "instructor" },
                    { 3, "student" }
                });

            migrationBuilder.InsertData(
                table: "TimeSlots",
                columns: new[] { "Id", "Day", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { 101, "Saturday", new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 102, "Saturday", new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 30, 0, 0) },
                    { 103, "Saturday", new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 14, 30, 0, 0) },
                    { 104, "Saturday", new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 16, 0, 0, 0) },
                    { 201, "Sunday", new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 202, "Sunday", new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 30, 0, 0) },
                    { 203, "Sunday", new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 14, 30, 0, 0) },
                    { 204, "Sunday", new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 16, 0, 0, 0) },
                    { 301, "Monday", new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 302, "Monday", new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 30, 0, 0) },
                    { 303, "Monday", new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 14, 30, 0, 0) },
                    { 304, "Monday", new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 16, 0, 0, 0) },
                    { 401, "Tuesday", new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 402, "Tuesday", new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 30, 0, 0) },
                    { 403, "Tuesday", new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 14, 30, 0, 0) },
                    { 404, "Tuesday", new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 16, 0, 0, 0) },
                    { 501, "Wednesday", new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 9, 0, 0, 0) },
                    { 502, "Wednesday", new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 10, 30, 0, 0) },
                    { 503, "Wednesday", new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 14, 30, 0, 0) },
                    { 504, "Wednesday", new TimeSpan(0, 17, 30, 0, 0), new TimeSpan(0, 16, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CodeId",
                table: "Courses",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Prerequisites_PrerequisiteCourseCodeId",
                table: "Prerequisites",
                column: "PrerequisiteCourseCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassroomId",
                table: "Sections",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_CourseId",
                table: "Sections",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_TimeSlotId",
                table: "Sections",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Takes_SectionId",
                table: "Takes",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teaches_SectionId",
                table: "Teaches",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Prerequisites");

            migrationBuilder.DropTable(
                name: "Takes");

            migrationBuilder.DropTable(
                name: "Teaches");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "CourseCodes");
        }
    }
}
