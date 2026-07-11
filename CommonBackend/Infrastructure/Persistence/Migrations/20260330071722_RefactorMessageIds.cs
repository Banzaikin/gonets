using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonBackend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMessageIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // ===== IncomingMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_IncomingMessages",
        table: "IncomingMessages");

    migrationBuilder.AddColumn<Guid>(
        name: "DbId",
        table: "IncomingMessages",
        type: "uuid",
        nullable: true);

    migrationBuilder.Sql("""
        UPDATE "IncomingMessages"
        SET "DbId" = gen_random_uuid()
        WHERE "DbId" IS NULL;
    """);

    migrationBuilder.AlterColumn<Guid>(
        name: "DbId",
        table: "IncomingMessages",
        type: "uuid",
        nullable: false,
        oldClrType: typeof(Guid),
        oldNullable: true);

    migrationBuilder.RenameColumn(
        name: "Id",
        table: "IncomingMessages",
        newName: "MessageId");

    migrationBuilder.AddPrimaryKey(
        name: "PK_IncomingMessages",
        table: "IncomingMessages",
        column: "DbId");

    migrationBuilder.CreateIndex(
        name: "IX_IncomingMessages_MessageId",
        table: "IncomingMessages",
        column: "MessageId",
        unique: true);


    // ===== OutgoingMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_OutgoingMessages",
        table: "OutgoingMessages");

    migrationBuilder.AddColumn<Guid>(
        name: "DbId",
        table: "OutgoingMessages",
        type: "uuid",
        nullable: true);

    migrationBuilder.Sql("""
        UPDATE "OutgoingMessages"
        SET "DbId" = gen_random_uuid()
        WHERE "DbId" IS NULL;
    """);

    migrationBuilder.AlterColumn<Guid>(
        name: "DbId",
        table: "OutgoingMessages",
        type: "uuid",
        nullable: false,
        oldClrType: typeof(Guid),
        oldNullable: true);

    migrationBuilder.RenameColumn(
        name: "Id",
        table: "OutgoingMessages",
        newName: "MessageId");

    migrationBuilder.AddPrimaryKey(
        name: "PK_OutgoingMessages",
        table: "OutgoingMessages",
        column: "DbId");

    migrationBuilder.CreateIndex(
        name: "IX_OutgoingMessages_MessageId",
        table: "OutgoingMessages",
        column: "MessageId",
        unique: true);


    // ===== SentMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_SentMessages",
        table: "SentMessages");

    migrationBuilder.AddColumn<Guid>(
        name: "DbId",
        table: "SentMessages",
        type: "uuid",
        nullable: true);

    migrationBuilder.Sql("""
        UPDATE "SentMessages"
        SET "DbId" = gen_random_uuid()
        WHERE "DbId" IS NULL;
    """);

    migrationBuilder.AlterColumn<Guid>(
        name: "DbId",
        table: "SentMessages",
        type: "uuid",
        nullable: false,
        oldClrType: typeof(Guid),
        oldNullable: true);

    migrationBuilder.RenameColumn(
        name: "Id",
        table: "SentMessages",
        newName: "MessageId");

    migrationBuilder.AddPrimaryKey(
        name: "PK_SentMessages",
        table: "SentMessages",
        column: "DbId");

    migrationBuilder.CreateIndex(
        name: "IX_SentMessages_MessageId",
        table: "SentMessages",
        column: "MessageId",
        unique: true);

    migrationBuilder.UpdateData(
        table: "Users",
        keyColumn: "Id",
        keyValue: 1,
        column: "PasswordHash",
        value: "$2a$11$M6dFl4rrJQTrfBBqWgLjXOIqv0w8uWZzREL7NiuRNDXBlWe4ayoZW");
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    // ===== SentMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_SentMessages",
        table: "SentMessages");

    migrationBuilder.DropIndex(
        name: "IX_SentMessages_MessageId",
        table: "SentMessages");

    migrationBuilder.RenameColumn(
        name: "MessageId",
        table: "SentMessages",
        newName: "Id");

    migrationBuilder.DropColumn(
        name: "DbId",
        table: "SentMessages");

    migrationBuilder.AddPrimaryKey(
        name: "PK_SentMessages",
        table: "SentMessages",
        column: "Id");


    // ===== OutgoingMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_OutgoingMessages",
        table: "OutgoingMessages");

    migrationBuilder.DropIndex(
        name: "IX_OutgoingMessages_MessageId",
        table: "OutgoingMessages");

    migrationBuilder.RenameColumn(
        name: "MessageId",
        table: "OutgoingMessages",
        newName: "Id");

    migrationBuilder.DropColumn(
        name: "DbId",
        table: "OutgoingMessages");

    migrationBuilder.AddPrimaryKey(
        name: "PK_OutgoingMessages",
        table: "OutgoingMessages",
        column: "Id");


    // ===== IncomingMessages =====
    migrationBuilder.DropPrimaryKey(
        name: "PK_IncomingMessages",
        table: "IncomingMessages");

    migrationBuilder.DropIndex(
        name: "IX_IncomingMessages_MessageId",
        table: "IncomingMessages");

    migrationBuilder.RenameColumn(
        name: "MessageId",
        table: "IncomingMessages",
        newName: "Id");

    migrationBuilder.DropColumn(
        name: "DbId",
        table: "IncomingMessages");

    migrationBuilder.AddPrimaryKey(
        name: "PK_IncomingMessages",
        table: "IncomingMessages",
        column: "Id");

    migrationBuilder.UpdateData(
        table: "Users",
        keyColumn: "Id",
        keyValue: 1,
        column: "PasswordHash",
        value: "$2a$11$I5mUXEwYTAafOV5PD/ic4eLTW3xxaAudOdlVuE6lS0Jego0AcbF0O");
}
    }
}
