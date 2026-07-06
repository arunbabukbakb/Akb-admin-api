using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Rolemodified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "RoleCode",
                table: "Roles",
                newName: "UserType");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Status", "UserType" },
                values: new object[] { false, 0 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Status", "UserType" },
                values: new object[] { false, 1 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Status", "UserType" },
                values: new object[] { false, 2 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 16, 3, 29, 19, DateTimeKind.Utc).AddTicks(6379), "xfsMRAfSvdNUDY9YaBZ17zgyrVx0Y502t5i/BaDy5qfsbmEYOJnX/qwWZfxfhiyb" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 16, 3, 29, 27, DateTimeKind.Utc).AddTicks(1493), "j0+NFgGzmMXKjiT/yFPqKWl3Of4fCKNFcQ/dN6vgu/IK4jK14RHEbaPaDiRggqtb" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 16, 3, 29, 34, DateTimeKind.Utc).AddTicks(9886), "rFlrune5HHeS0HrEz36JsgAS/V1bZ8jfdrCdLIi1kJNWJie3PuG+b2MaDov91rb4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "UserType",
                table: "Roles",
                newName: "RoleCode");

            migrationBuilder.AddColumn<int>(
                name: "ParentCode",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ParentCode", "RoleCode" },
                values: new object[] { 0, 1 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ParentCode", "RoleCode" },
                values: new object[] { 0, 2 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ParentCode", "RoleCode" },
                values: new object[] { 0, 3 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 15, 41, 32, 580, DateTimeKind.Utc).AddTicks(6343), "JqT+wrKXXcopFxsNwpnLTfi0E8hy0hVhIkHkTpsLadyanac+TCjyE51jssuWOO+g" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 15, 41, 32, 587, DateTimeKind.Utc).AddTicks(9184), "lM4WUF5Q3KE1UkMQ2lEHRSbCzj0FfM1JwVwxywNtvpjo0thMwY21fkqJQOy8tTww" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedDate", "Password" },
                values: new object[] { new DateTime(2026, 7, 2, 15, 41, 32, 595, DateTimeKind.Utc).AddTicks(1176), "CSJuLGd3scgGBRoQ4P7TvHLDpvSsWx3RYaZeydQzNzK1bVr6bnTfnrbRRm4gxr3g" });
        }
    }
}
