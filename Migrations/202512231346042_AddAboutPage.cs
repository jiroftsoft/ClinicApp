namespace ClinicApp.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddAboutPage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AboutPages",
                c => new
                    {
                        AboutPageId = c.Int(nullable: false, identity: true),
                        ClinicName = c.String(nullable: false, maxLength: 200),
                        ClinicDescription = c.String(nullable: false, storeType: "ntext"),
                        EstablishedYear = c.String(maxLength: 50),
                        MissionValuesJson = c.String(storeType: "ntext"),
                        LicensesJson = c.String(storeType: "ntext"),
                        RegulatoryBody = c.String(maxLength: 500),
                        MedicalTeamDescription = c.String(maxLength: 1000),
                        InfrastructureDescription = c.String(maxLength: 1000),
                        EthicalCommitmentsJson = c.String(storeType: "ntext"),
                        HeroImageUrl = c.String(maxLength: 500),
                        BackgroundImageUrl = c.String(maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        MetaTitle = c.String(maxLength: 500),
                        MetaDescription = c.String(maxLength: 1000),
                        Slug = c.String(maxLength: 200),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedByUserId = c.String(maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                        UpdatedAt = c.DateTime(),
                        UpdatedByUserId = c.String(maxLength: 128),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_AboutPage_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.AboutPageId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.DeletedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedByUserId)
                .Index(t => t.ClinicName, name: "IX_AboutPage_ClinicName")
                .Index(t => t.IsActive, name: "IX_AboutPage_IsActive")
                .Index(t => t.Slug, name: "IX_AboutPage_Slug")
                .Index(t => t.IsDeleted, name: "IX_AboutPage_IsDeleted")
                .Index(t => t.DeletedByUserId)
                .Index(t => t.CreatedByUserId)
                .Index(t => t.UpdatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AboutPages", "UpdatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AboutPages", "DeletedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AboutPages", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.AboutPages", new[] { "UpdatedByUserId" });
            DropIndex("dbo.AboutPages", new[] { "CreatedByUserId" });
            DropIndex("dbo.AboutPages", new[] { "DeletedByUserId" });
            DropIndex("dbo.AboutPages", "IX_AboutPage_IsDeleted");
            DropIndex("dbo.AboutPages", "IX_AboutPage_Slug");
            DropIndex("dbo.AboutPages", "IX_AboutPage_IsActive");
            DropIndex("dbo.AboutPages", "IX_AboutPage_ClinicName");
            DropTable("dbo.AboutPages",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_AboutPage_IsDeletedFilter", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
