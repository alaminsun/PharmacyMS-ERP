using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PharmacyMS.Infrastructure.Data;

#nullable disable
namespace PharmacyMS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261018060000_AddAuditEvents")]
public sealed class AddAuditEvents:Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        mb.CreateTable(name:"AuditEvents",columns:table=>new{
            Id=table.Column<long>(type:"bigint",nullable:false).Annotation("SqlServer:Identity","1, 1"),
            OccurredAtUtc=table.Column<DateTime>(type:"datetime2",nullable:false),
            ActorId=table.Column<string>(type:"nvarchar(450)",maxLength:450,nullable:false),
            ActorEmail=table.Column<string>(type:"nvarchar(256)",maxLength:256,nullable:false),
            Action=table.Column<string>(type:"nvarchar(10)",maxLength:10,nullable:false),
            Path=table.Column<string>(type:"nvarchar(240)",maxLength:240,nullable:false),
            StatusCode=table.Column<int>(type:"int",nullable:false),
            TraceId=table.Column<string>(type:"nvarchar(100)",maxLength:100,nullable:false)
        },constraints:table=>table.PrimaryKey("PK_AuditEvents",x=>x.Id));
        mb.CreateIndex(name:"IX_AuditEvents_OccurredAtUtc",table:"AuditEvents",column:"OccurredAtUtc");
        mb.CreateIndex(name:"IX_AuditEvents_ActorEmail",table:"AuditEvents",column:"ActorEmail");
    }
    protected override void Down(MigrationBuilder mb)=>mb.DropTable(name:"AuditEvents");
}
