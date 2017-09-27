namespace AspNetIdentityTry1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Setups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false),
                        AllowUserChangeItem = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Setups");
        }
    }
}
