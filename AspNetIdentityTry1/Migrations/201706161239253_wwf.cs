namespace AspNetIdentityTry1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wwf : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Items", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Items", new[] { "User_Id" });
            DropTable("dbo.Items");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Items", "User_Id");
            AddForeignKey("dbo.Items", "User_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
