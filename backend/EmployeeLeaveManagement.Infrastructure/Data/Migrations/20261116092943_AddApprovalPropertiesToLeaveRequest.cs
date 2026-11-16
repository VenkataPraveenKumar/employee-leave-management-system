using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeLeaveManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalPropertiesToLeaveRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActionedOn",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverComments",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionedOn",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApproverComments",
                table: "LeaveRequests");
        }
    }
}
