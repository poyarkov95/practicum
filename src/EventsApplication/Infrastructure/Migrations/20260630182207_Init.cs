using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Название события"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Описание события"),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время начала события"),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время окончания события"),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false, comment: "Общее количество мест"),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
