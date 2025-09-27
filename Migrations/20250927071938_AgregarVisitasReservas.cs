using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalinmobiliario1.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVisitasReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InmuebleId1",
                table: "Visitas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InmuebleId1",
                table: "Reservas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_InmuebleId1",
                table: "Visitas",
                column: "InmuebleId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_InmuebleId1",
                table: "Reservas",
                column: "InmuebleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Inmuebles_InmuebleId1",
                table: "Reservas",
                column: "InmuebleId1",
                principalTable: "Inmuebles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitas_Inmuebles_InmuebleId1",
                table: "Visitas",
                column: "InmuebleId1",
                principalTable: "Inmuebles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Inmuebles_InmuebleId1",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitas_Inmuebles_InmuebleId1",
                table: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Visitas_InmuebleId1",
                table: "Visitas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_InmuebleId1",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "InmuebleId1",
                table: "Visitas");

            migrationBuilder.DropColumn(
                name: "InmuebleId1",
                table: "Reservas");
        }
    }
}
