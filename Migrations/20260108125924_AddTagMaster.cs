using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlarmMonitor.Migrations
{
    /// <inheritdoc />
    public partial class AddTagMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlarmEvents",
                columns: table => new
                {
                    EventID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: true),
                    SourceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourcePath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    SourceID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TicksTimeStamp = table.Column<long>(type: "bigint", nullable: true),
                    EventTimeStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ConditionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SubConditionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AlarmClass = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    Acked = table.Column<bool>(type: "bit", nullable: true),
                    EffDisabled = table.Column<bool>(type: "bit", nullable: true),
                    Disabled = table.Column<bool>(type: "bit", nullable: true),
                    EffSuppressed = table.Column<bool>(type: "bit", nullable: true),
                    Suppressed = table.Column<bool>(type: "bit", nullable: true),
                    PersonID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChangeMask = table.Column<int>(type: "int", nullable: true),
                    InputValue = table.Column<double>(type: "float", nullable: true),
                    LimitValue = table.Column<double>(type: "float", nullable: true),
                    Quality = table.Column<int>(type: "int", nullable: true),
                    EventAssociationID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserComment = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ComputerID = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Tag1Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Tag2Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Tag3Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Tag4Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AutoUnshelveTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupPath = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    MessageID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmEvents", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "TagMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HighLimit = table.Column<float>(type: "real", nullable: false),
                    LowLimit = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagMasters", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmEvents");

            migrationBuilder.DropTable(
                name: "TagMasters");
        }
    }
}
