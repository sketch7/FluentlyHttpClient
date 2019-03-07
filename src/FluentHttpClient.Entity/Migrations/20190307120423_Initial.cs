using Microsoft.EntityFrameworkCore.Migrations;

namespace FluentHttpClient.Entity.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cache");

            migrationBuilder.CreateTable(
                name: "HttpResponses",
                schema: "cache",
                columns: table => new
                {
                    Hash = table.Column<string>(maxLength: 1000, nullable: false),
                    Name = table.Column<string>(maxLength: 70, nullable: true),
                    Url = table.Column<string>(maxLength: 255, nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Headers = table.Column<string>(maxLength: 1000, nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    ReasonPhrase = table.Column<string>(maxLength: 70, nullable: false),
                    Version = table.Column<string>(maxLength: 30, nullable: false),
                    ContentHeaders = table.Column<string>(maxLength: 1000, nullable: false),
                    RequestMessage = table.Column<string>(maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpResponses", x => x.Hash);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HttpResponses",
                schema: "cache");
        }
    }
}
