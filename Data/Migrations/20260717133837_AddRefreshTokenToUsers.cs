using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password", "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 733, DateTimeKind.Utc).AddTicks(7438), "6u3dcuXJXIZ4MGW7SnRVPhJI/Eu1/bFD7lrIdBjXPnT/Vm9WhtNV6Upvv6gWx32U", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password", "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 741, DateTimeKind.Utc).AddTicks(2455), "ZjiuOz0XTVr+iRMSA7CObz+OAKX+T2sRwleDEKgvQwVe6YeWFFpDbYJZjLjUVzK1", null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password", "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { new DateTime(2026, 7, 17, 13, 38, 36, 748, DateTimeKind.Utc).AddTicks(6904), "xWMeTeouj+9HpWS7OHyPtpCa+S+yxLDOyVOec2y4eeJfGzKXY5O7qccuxQQ5ppaf", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 3, 3, 40, 4, 80, DateTimeKind.Utc).AddTicks(9089), "/EhL0hOVDdc1hBePZa0ww4wrJ1gSjDVbOyfHqFWBvXEwL4RygEnY0OTi0OKGd+cB" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 3, 3, 40, 4, 88, DateTimeKind.Utc).AddTicks(9760), "utUubo/vh5pQHBsRCDUJsX1dtA0ObDPGIeku6e/OgpVqKHEQVmPQOJ3Ozo/IOFq/" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 3, 3, 40, 4, 96, DateTimeKind.Utc).AddTicks(3626), "Z8EG7QUJ4Z4UxJN9NfLxSB44s7cdqVFxLdY0rd43yTN6szo5cT1hn6zwPeICuyJH" });
        }
    }
}
