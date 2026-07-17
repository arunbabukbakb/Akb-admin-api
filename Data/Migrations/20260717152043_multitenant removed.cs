using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class multitenantremoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 15, 20, 41, 810, DateTimeKind.Utc).AddTicks(954), "i++JsRPol3tDDVEcfOBeNjPoacmjlzBHR5S3HqxcL3AQrIXAOlsgZ3IE0T6r/zRQ" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 15, 20, 41, 817, DateTimeKind.Utc).AddTicks(2473), "9riNzf9vtwkHtPP2UEkbXKRqfW4VOWFOgUZuSWROkruTIHazLItyr4CSvVUsPC9w" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 15, 20, 41, 824, DateTimeKind.Utc).AddTicks(3803), "Xs9csvd7BZh9Ob8BDNAa9IVt7aikIt5lFtbgYduZbmXHLZsdEjb5ffe+cVAV2Icu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 733, DateTimeKind.Utc).AddTicks(7438), "6u3dcuXJXIZ4MGW7SnRVPhJI/Eu1/bFD7lrIdBjXPnT/Vm9WhtNV6Upvv6gWx32U" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 741, DateTimeKind.Utc).AddTicks(2455), "ZjiuOz0XTVr+iRMSA7CObz+OAKX+T2sRwleDEKgvQwVe6YeWFFpDbYJZjLjUVzK1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 748, DateTimeKind.Utc).AddTicks(6904), "xWMeTeouj+9HpWS7OHyPtpCa+S+yxLDOyVOec2y4eeJfGzKXY5O7qccuxQQ5ppaf" });
        }
    }
}
