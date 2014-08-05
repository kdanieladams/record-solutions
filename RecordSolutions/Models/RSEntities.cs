using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace RecordSolutions.Models
{
    public class RSEntities : DbContext
    {
        public RSEntities() : base()
        { 
        }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<FileExtension> FileExtensions { get; set; }
    }
}