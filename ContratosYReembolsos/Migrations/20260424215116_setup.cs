using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratosYReembolsos.Migrations
{
    /// <inheritdoc />
    public partial class setup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    RequiredPermission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupingKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductosCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowInContracts = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosCategorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplatesSepulturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalFloors = table.Column<int>(type: "int", nullable: false),
                    RowsCount = table.Column<int>(type: "int", nullable: false),
                    ColsPerFace = table.Column<int>(type: "int", nullable: false),
                    IsDoubleFace = table.Column<bool>(type: "bit", nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesSepulturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposVehiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposVehiculo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ubigeos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ubigeos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductosSubcategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowInContracts = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosSubcategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosSubcategorias_ProductosCategorias_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Filiales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UbigeoId = table.Column<string>(type: "nvarchar(6)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasWakeService = table.Column<bool>(type: "bit", nullable: false),
                    HasOwnCemetery = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filiales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filiales_Ubigeos_UbigeoId",
                        column: x => x.UbigeoId,
                        principalTable: "Ubigeos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ControlType = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsAvailableForContract = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_ProductosCategorias_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_ProductosSubcategorias_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "ProductosSubcategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Agencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RUC = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agencias_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DNI = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cementerios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RUC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    UbigeoId = table.Column<string>(type: "nvarchar(6)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cementerios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cementerios_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cementerios_Ubigeos_UbigeoId",
                        column: x => x.UbigeoId,
                        principalTable: "Ubigeos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conductores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conductores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conductores_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductosTransferencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalControlNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginBranchId = table.Column<int>(type: "int", nullable: false),
                    TargetBranchId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceptionObservation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosTransferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosTransferencias_Filiales_OriginBranchId",
                        column: x => x.OriginBranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosTransferencias_Filiales_TargetBranchId",
                        column: x => x.TargetBranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehiculos_TiposVehiculo_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivosFijos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PatrimonialCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivosFijos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivosFijos_Productos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductosStock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosStock_Productos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SepulturasEstructura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CemeteryId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepulturasEstructura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SepulturasEstructura_Cementerios_CemeteryId",
                        column: x => x.CemeteryId,
                        principalTable: "Cementerios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SepulturasEstructura_TemplatesSepulturas_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "TemplatesSepulturas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductosTransferenciasDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FixedAssetId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosTransferenciasDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosTransferenciasDetalles_ActivosFijos_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductosTransferenciasDetalles_ProductosTransferencias_TransferId",
                        column: x => x.TransferId,
                        principalTable: "ProductosTransferencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductosTransferenciasDetalles_Productos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    FixedAssetId = table.Column<int>(type: "int", nullable: true),
                    ProductStockId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Concept = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    InternalControlNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ActivosFijos_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ProductosStock_ProductStockId",
                        column: x => x.ProductStockId,
                        principalTable: "ProductosStock",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Productos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SolicitorDni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitorType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeceasedDni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeceasedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BurialDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BurialTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    UbigeoId = table.Column<string>(type: "nvarchar(6)", nullable: false),
                    WakeId = table.Column<int>(type: "int", nullable: true),
                    CemeteryId = table.Column<int>(type: "int", nullable: false),
                    IntermentStructureId = table.Column<int>(type: "int", nullable: true),
                    IntermentSpaceId = table.Column<int>(type: "int", nullable: true),
                    AgencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Agencias_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Cementerios_CemeteryId",
                        column: x => x.CemeteryId,
                        principalTable: "Cementerios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contratos_Filiales_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Filiales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_SepulturasEstructura_IntermentStructureId",
                        column: x => x.IntermentStructureId,
                        principalTable: "SepulturasEstructura",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contratos_Ubigeos_UbigeoId",
                        column: x => x.UbigeoId,
                        principalTable: "Ubigeos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesMovilidadContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesMovilidadContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesMovilidadContrato_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesMovilidadContrato_TiposVehiculo_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "TiposVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesMovilidadContrato_Vehiculos_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesProductosContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    FixedAssetId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesProductosContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesProductosContrato_ActivosFijos_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "ActivosFijos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetallesProductosContrato_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesProductosContrato_Productos_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SepulturasNichos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    RowLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColumnNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StructureId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    ContractId1 = table.Column<int>(type: "int", nullable: true),
                    InhumationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepulturasNichos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SepulturasNichos_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SepulturasNichos_Contratos_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SepulturasNichos_SepulturasEstructura_StructureId",
                        column: x => x.StructureId,
                        principalTable: "SepulturasEstructura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehiculosServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ContractMovilityDetailId = table.Column<int>(type: "int", nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TripStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiculosServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Conductores_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Conductores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Contratos_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_DetallesMovilidadContrato_ContractMovilityDetailId",
                        column: x => x.ContractMovilityDetailId,
                        principalTable: "DetallesMovilidadContrato",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehiculosServicios_Vehiculos_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_PatrimonialCode",
                table: "ActivosFijos",
                column: "PatrimonialCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivosFijos_ProductId",
                table: "ActivosFijos",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencias_BranchId",
                table: "Agencias",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cementerios_BranchId",
                table: "Cementerios",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Cementerios_UbigeoId",
                table: "Cementerios",
                column: "UbigeoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conductores_BranchId",
                table: "Conductores",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_AgencyId",
                table: "Contratos",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_BranchId",
                table: "Contratos",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_CemeteryId",
                table: "Contratos",
                column: "CemeteryId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_IntermentSpaceId",
                table: "Contratos",
                column: "IntermentSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_IntermentStructureId",
                table: "Contratos",
                column: "IntermentStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_UbigeoId",
                table: "Contratos",
                column: "UbigeoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMovilidadContrato_ContractId",
                table: "DetallesMovilidadContrato",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMovilidadContrato_VehicleId",
                table: "DetallesMovilidadContrato",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesMovilidadContrato_VehicleTypeId",
                table: "DetallesMovilidadContrato",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesProductosContrato_ContractId",
                table: "DetallesProductosContrato",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesProductosContrato_FixedAssetId",
                table: "DetallesProductosContrato",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesProductosContrato_ProductId",
                table: "DetallesProductosContrato",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Filiales_UbigeoId",
                table: "Filiales",
                column: "UbigeoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_BranchId",
                table: "MovimientosInventario",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FixedAssetId",
                table: "MovimientosInventario",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductId",
                table: "MovimientosInventario",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ProductStockId",
                table: "MovimientosInventario",
                column: "ProductStockId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoryId",
                table: "Productos",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_SubCategoryId",
                table: "Productos",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosStock_ProductId_BranchId",
                table: "ProductosStock",
                columns: new[] { "ProductId", "BranchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosSubcategorias_CategoryId",
                table: "ProductosSubcategorias",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferencias_InternalControlNumber",
                table: "ProductosTransferencias",
                column: "InternalControlNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferencias_OriginBranchId",
                table: "ProductosTransferencias",
                column: "OriginBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferencias_TargetBranchId",
                table: "ProductosTransferencias",
                column: "TargetBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferenciasDetalles_FixedAssetId",
                table: "ProductosTransferenciasDetalles",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferenciasDetalles_ProductId",
                table: "ProductosTransferenciasDetalles",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosTransferenciasDetalles_TransferId",
                table: "ProductosTransferenciasDetalles",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasEstructura_CemeteryId",
                table: "SepulturasEstructura",
                column: "CemeteryId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasEstructura_TemplateId",
                table: "SepulturasEstructura",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasNichos_ContractId",
                table: "SepulturasNichos",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasNichos_ContractId1",
                table: "SepulturasNichos",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_SepulturasNichos_StructureId",
                table: "SepulturasNichos",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_BranchId",
                table: "Vehiculos",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_VehicleTypeId",
                table: "Vehiculos",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_ContractId",
                table: "VehiculosServicios",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_ContractMovilityDetailId",
                table: "VehiculosServicios",
                column: "ContractMovilityDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_DriverId",
                table: "VehiculosServicios",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiculosServicios_VehicleId",
                table: "VehiculosServicios",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_SepulturasNichos_IntermentSpaceId",
                table: "Contratos",
                column: "IntermentSpaceId",
                principalTable: "SepulturasNichos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agencias_Filiales_BranchId",
                table: "Agencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Cementerios_Filiales_BranchId",
                table: "Cementerios");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Filiales_BranchId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Cementerios_Ubigeos_UbigeoId",
                table: "Cementerios");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Ubigeos_UbigeoId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Agencias_AgencyId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Cementerios_CemeteryId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_SepulturasEstructura_Cementerios_CemeteryId",
                table: "SepulturasEstructura");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_SepulturasEstructura_IntermentStructureId",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_SepulturasNichos_SepulturasEstructura_StructureId",
                table: "SepulturasNichos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_SepulturasNichos_IntermentSpaceId",
                table: "Contratos");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DetallesProductosContrato");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ProductosTransferenciasDetalles");

            migrationBuilder.DropTable(
                name: "VehiculosServicios");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ProductosStock");

            migrationBuilder.DropTable(
                name: "ActivosFijos");

            migrationBuilder.DropTable(
                name: "ProductosTransferencias");

            migrationBuilder.DropTable(
                name: "Conductores");

            migrationBuilder.DropTable(
                name: "DetallesMovilidadContrato");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "ProductosSubcategorias");

            migrationBuilder.DropTable(
                name: "TiposVehiculo");

            migrationBuilder.DropTable(
                name: "ProductosCategorias");

            migrationBuilder.DropTable(
                name: "Filiales");

            migrationBuilder.DropTable(
                name: "Ubigeos");

            migrationBuilder.DropTable(
                name: "Agencias");

            migrationBuilder.DropTable(
                name: "Cementerios");

            migrationBuilder.DropTable(
                name: "SepulturasEstructura");

            migrationBuilder.DropTable(
                name: "TemplatesSepulturas");

            migrationBuilder.DropTable(
                name: "SepulturasNichos");

            migrationBuilder.DropTable(
                name: "Contratos");
        }
    }
}
