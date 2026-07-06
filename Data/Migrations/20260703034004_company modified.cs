using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class companymodified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternatePhone",
                table: "Company");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternatePhone",
                table: "Company",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 1,
                column: "AlternatePhone",
                value: null);

            migrationBuilder.UpdateData(
                table: "Company",
                keyColumn: "Id",
                keyValue: 2,
                column: "AlternatePhone",
                value: null);

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
    }
}
