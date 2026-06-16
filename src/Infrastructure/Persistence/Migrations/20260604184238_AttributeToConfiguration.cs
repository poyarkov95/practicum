using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventApplication.Migrations
{
    /// <inheritdoc />
    public partial class AttributeToConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalSeats",
                table: "Events",
                type: "integer",
                nullable: false,
                comment: "Общее количество мест",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Общее количество мест на событии");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Название события",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Заголовок события");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата и время начала события",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата начала события");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата и время окончания события",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата окончания события");

            migrationBuilder.AlterColumn<int>(
                name: "AvailableSeats",
                table: "Events",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Текущее количество свободных мест");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalSeats",
                table: "Events",
                type: "integer",
                nullable: false,
                comment: "Общее количество мест на событии",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Общее количество мест");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Заголовок события",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Название события");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата начала события",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата и время начала события");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата окончания события",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата и время окончания события");

            migrationBuilder.AlterColumn<int>(
                name: "AvailableSeats",
                table: "Events",
                type: "integer",
                nullable: false,
                comment: "Текущее количество свободных мест",
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
