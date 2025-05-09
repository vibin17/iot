﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Weather.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeatherSnapshots",
                columns: table => new
                {
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Temperature = table.Column<double>(type: "double precision", nullable: false),
                    Humidity = table.Column<double>(type: "double precision", nullable: false),
                    Pressure = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.Sql(
                $"""
                    SELECT create_hypertable('"WeatherSnapshots"', 'Timestamp');
                    CREATE INDEX ix_symbol_time ON "WeatherSnapshots" ("Timestamp" DESC)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherSnapshots");
        }
    }
}
