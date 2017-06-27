namespace AspNetIdentityTry1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sample1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Priority = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Items", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Items", new[] { "User_Id" });
            DropTable("dbo.Items");
        }
    }
}
