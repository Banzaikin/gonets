using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSystemIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== Удаляем колонку SystemId из всех таблиц =====
            migrationBuilder.Sql("""
                ALTER TABLE "IncomingMessages" DROP COLUMN IF EXISTS "SystemId";
                ALTER TABLE "OutgoingMessages" DROP COLUMN IF EXISTS "SystemId";
                ALTER TABLE "SentMessages" DROP COLUMN IF EXISTS "SystemId";
            """);

            // Обновление пароля (оставьте как есть)
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ncLB.J/zacfzT.VgBMHrie2NGYW1kE.Syxdqg4hF.9kkZUkNKvSEm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ===== Восстанавливаем колонку при откате =====
            migrationBuilder.Sql("""
                ALTER TABLE "IncomingMessages" ADD COLUMN IF NOT EXISTS "SystemId" text;
                ALTER TABLE "OutgoingMessages" ADD COLUMN IF NOT EXISTS "SystemId" text;
                ALTER TABLE "SentMessages" ADD COLUMN IF NOT EXISTS "SystemId" text;
            """);

            // Откат пароля
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$M6dFl4rrJQTrfBBqWgLjXOIqv0w8uWZzREL7NiuRNDXBlWe4ayoZW");
        }
    }
}