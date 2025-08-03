using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace gestionMissionBack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class notifSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Article",
                columns: table => new
                {
                    ArticleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Volume = table.Column<double>(type: "float", nullable: false),
                    PhotoUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Article__9C6270C8AD0F3EFB", x => x.ArticleID);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    CityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.CityID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Availability = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxCapacity = table.Column<double>(type: "float", nullable: false),
                    PhotoUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vehicle__476B54B22EA98523", x => x.VehicleID);
                });

            migrationBuilder.CreateTable(
                name: "Site",
                columns: table => new
                {
                    SiteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CityID = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Site", x => x.SiteID);
                    table.ForeignKey(
                        name: "FK_Site_City",
                        column: x => x.CityID,
                        principalTable: "City",
                        principalColumn: "CityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CurrentDriverStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CCACD0A71248", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mission",
                columns: table => new
                {
                    MissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequesterID = table.Column<int>(type: "int", nullable: false),
                    DriverID = table.Column<int>(type: "int", nullable: false),
                    SystemDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Service = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Receiver = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Mission__66DFB854D8A8E8BD", x => x.MissionID);
                    table.ForeignKey(
                        name: "FK__Mission__DriverID",
                        column: x => x.DriverID,
                        principalTable: "User",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__Mission__RequesterID",
                        column: x => x.RequesterID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "System"),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Unread"),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E3219D351A6", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__49C3F6B7",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "UserConnection",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConnection", x => x.ConnectionId);
                    table.ForeignKey(
                        name: "FK_UserConnection_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleReservation",
                columns: table => new
                {
                    ReservationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterID = table.Column<int>(type: "int", nullable: false),
                    VehicleID = table.Column<int>(type: "int", nullable: false),
                    RequiresDriver = table.Column<bool>(type: "bit", nullable: false),
                    Departure = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VehicleR__B7EE5F04DE7C200E", x => x.ReservationID);
                    table.ForeignKey(
                        name: "FK__VehicleRe__Reque__403A8C7D",
                        column: x => x.RequesterID,
                        principalTable: "User",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK__VehicleRe__Vehic__412EB0B6",
                        column: x => x.VehicleID,
                        principalTable: "Vehicle",
                        principalColumn: "VehicleID");
                });

            migrationBuilder.CreateTable(
                name: "Circuit",
                columns: table => new
                {
                    CircuitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionID = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureSiteID = table.Column<int>(type: "int", nullable: false),
                    ArrivalSiteID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Circuit__7D21697020F37C3D", x => x.CircuitID);
                    table.ForeignKey(
                        name: "FK_Circuit_ArrivalSite",
                        column: x => x.ArrivalSiteID,
                        principalTable: "Site",
                        principalColumn: "SiteID");
                    table.ForeignKey(
                        name: "FK_Circuit_DepartureSite",
                        column: x => x.DepartureSiteID,
                        principalTable: "Site",
                        principalColumn: "SiteID");
                    table.ForeignKey(
                        name: "FK__Circuit__Mission__5535A963",
                        column: x => x.MissionID,
                        principalTable: "Mission",
                        principalColumn: "MissionID");
                });

            migrationBuilder.CreateTable(
                name: "Incident",
                columns: table => new
                {
                    IncidentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IncidentDocsUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Incident__3D80539215473664", x => x.IncidentID);
                    table.ForeignKey(
                        name: "FK__Incident__Missio__4E88ABD4",
                        column: x => x.MissionID,
                        principalTable: "Mission",
                        principalColumn: "MissionID");
                });

            migrationBuilder.CreateTable(
                name: "MissionCost",
                columns: table => new
                {
                    CostID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissionID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptPhotoUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MissionC__8285231E0764B8E9", x => x.CostID);
                    table.ForeignKey(
                        name: "FK__MissionCo__Missi__52593CB8",
                        column: x => x.MissionID,
                        principalTable: "Mission",
                        principalColumn: "MissionID");
                });

            migrationBuilder.CreateTable(
                name: "Task",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SiteID = table.Column<int>(type: "int", nullable: false),
                    MissionID = table.Column<int>(type: "int", nullable: true),
                    IsFirstTask = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Task__7C6949D18E3FD33F", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_Task_Site",
                        column: x => x.SiteID,
                        principalTable: "Site",
                        principalColumn: "SiteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__Task__MissionID__787EE5A0",
                        column: x => x.MissionID,
                        principalTable: "Mission",
                        principalColumn: "MissionID");
                });

            migrationBuilder.CreateTable(
                name: "Route",
                columns: table => new
                {
                    RouteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CircuitID = table.Column<int>(type: "int", nullable: false),
                    DepartureSiteID = table.Column<int>(type: "int", nullable: false),
                    ArrivalSiteID = table.Column<int>(type: "int", nullable: false),
                    DistanceKM = table.Column<double>(type: "float", nullable: true),
                    Ordre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Route__80979AAD76DC2E82", x => x.RouteID);
                    table.ForeignKey(
                        name: "FK_Route_ArrivalSite",
                        column: x => x.ArrivalSiteID,
                        principalTable: "Site",
                        principalColumn: "SiteID");
                    table.ForeignKey(
                        name: "FK_Route_DepartureSite",
                        column: x => x.DepartureSiteID,
                        principalTable: "Site",
                        principalColumn: "SiteID");
                    table.ForeignKey(
                        name: "FK__Route__CircuitID__5812160E",
                        column: x => x.CircuitID,
                        principalTable: "Circuit",
                        principalColumn: "CircuitID");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    DocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__1ABEEF6F3F9BB20D", x => x.DocumentID);
                    table.ForeignKey(
                        name: "FK__Document__TaskID__619B8048",
                        column: x => x.TaskID,
                        principalTable: "Task",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskArticle",
                columns: table => new
                {
                    ArticlesArticleId = table.Column<int>(type: "int", nullable: false),
                    TasksTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskArticle", x => new { x.ArticlesArticleId, x.TasksTaskId });
                    table.ForeignKey(
                        name: "FK_TaskArticle_Article_ArticlesArticleId",
                        column: x => x.ArticlesArticleId,
                        principalTable: "Article",
                        principalColumn: "ArticleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskArticle_Task_TasksTaskId",
                        column: x => x.TasksTaskId,
                        principalTable: "Task",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "Code", "Libelle", "Name" },
                values: new object[,]
                {
                    { 1, "ADMN", "", "Admin" },
                    { 2, "DRVR", "Driver", "Driver" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Circuit_ArrivalSiteID",
                table: "Circuit",
                column: "ArrivalSiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Circuit_DepartureSiteID",
                table: "Circuit",
                column: "DepartureSiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Circuit_MissionID",
                table: "Circuit",
                column: "MissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Document_TaskID",
                table: "Document",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_MissionID",
                table: "Incident",
                column: "MissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_DriverID",
                table: "Mission",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_RequesterID",
                table: "Mission",
                column: "RequesterID");

            migrationBuilder.CreateIndex(
                name: "IX_MissionCost_MissionID",
                table: "MissionCost",
                column: "MissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserID",
                table: "Notification",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Route_ArrivalSiteID",
                table: "Route",
                column: "ArrivalSiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Route_CircuitID",
                table: "Route",
                column: "CircuitID");

            migrationBuilder.CreateIndex(
                name: "IX_Route_DepartureSiteID",
                table: "Route",
                column: "DepartureSiteID");

            migrationBuilder.CreateIndex(
                name: "IX_Site_CityID",
                table: "Site",
                column: "CityID");

            migrationBuilder.CreateIndex(
                name: "IX_Task_MissionID",
                table: "Task",
                column: "MissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Task_SiteID",
                table: "Task",
                column: "SiteID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskArticle_TasksTaskId",
                table: "TaskArticle",
                column: "TasksTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D10534D7BE49D1",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConnection_UserId",
                table: "UserConnection",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Vehicle__026BC15C17EB5336",
                table: "Vehicle",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleReservation_RequesterID",
                table: "VehicleReservation",
                column: "RequesterID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleReservation_VehicleID",
                table: "VehicleReservation",
                column: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Incident");

            migrationBuilder.DropTable(
                name: "MissionCost");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Route");

            migrationBuilder.DropTable(
                name: "TaskArticle");

            migrationBuilder.DropTable(
                name: "UserConnection");

            migrationBuilder.DropTable(
                name: "VehicleReservation");

            migrationBuilder.DropTable(
                name: "Circuit");

            migrationBuilder.DropTable(
                name: "Article");

            migrationBuilder.DropTable(
                name: "Task");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "Site");

            migrationBuilder.DropTable(
                name: "Mission");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
