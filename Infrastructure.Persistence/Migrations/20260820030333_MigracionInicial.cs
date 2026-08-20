using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Extras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentificadorCliente = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "General"),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TipoDePago = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Descuento = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    MontoRecibido = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comandas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentificadorCliente = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "General"),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comandas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comandas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Folio = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EsReimpresion = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaItems_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComandaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComandaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    NotasCocina = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComandaItems_Comandas_ComandaId",
                        column: x => x.ComandaId,
                        principalTable: "Comandas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentaItemExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaItemExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaItemExtras_VentaItems_VentaItemId",
                        column: x => x.VentaItemId,
                        principalTable: "VentaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComandaItemExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComandaItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandaItemExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComandaItemExtras_ComandaItems_ComandaItemId",
                        column: x => x.ComandaItemId,
                        principalTable: "ComandaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComandaItemExtras_ComandaItemId",
                table: "ComandaItemExtras",
                column: "ComandaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ComandaItems_ComandaId",
                table: "ComandaItems",
                column: "ComandaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comandas_VentaId",
                table: "Comandas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Folio",
                table: "Tickets",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VentaId",
                table: "Tickets",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaItemExtras_VentaItemId",
                table: "VentaItemExtras",
                column: "VentaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaItems_VentaId",
                table: "VentaItems",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComandaItemExtras");

            migrationBuilder.DropTable(
                name: "Extras");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "VentaItemExtras");

            migrationBuilder.DropTable(
                name: "ComandaItems");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "VentaItems");

            migrationBuilder.DropTable(
                name: "Comandas");

            migrationBuilder.DropTable(
                name: "Ventas");
        }
    }
}
