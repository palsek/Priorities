namespace AspNetIdentityTry1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class parentNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "ParentUserName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "ParentUserName");
        }
    }
}
