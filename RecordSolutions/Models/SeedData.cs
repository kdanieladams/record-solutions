using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace RecordSolutions.Models
{
    public class SeedData : DropCreateDatabaseIfModelChanges<RSEntities>
    {
        protected override void Seed(RSEntities context)
        {
            List<Category> categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "Standard", Color = "#aaaaaa" },
                new Category { CategoryId = 2, Name = "Success", Color = "#43F14A"},
                new Category { CategoryId = 3, Name = "Warning", Color = "#ffff99" },
                new Category { CategoryId = 4, Name = "Danger", Color = "#ff9999"},
                new Category { CategoryId = 5, Name = "Comment", Color = "#1e619b"}
            };
            foreach (Category cat in categories)
                context.Categories.Add(cat);
            context.SaveChanges();

            GeneralSettings defaultGeneralSettings = new GeneralSettings(
                    "/Images/person-placeholder.jpg",
                    "http://www.gravatar.com/avatar/",
                    new List<FileExtension> 
                    { 
                        new FileExtension { Ext = ".jpg" }, 
                        new FileExtension { Ext = ".png" }, 
                        new FileExtension { Ext = ".gif" }, 
                        new FileExtension { Ext = ".jpeg" } 
                    }
                );
            context.GeneralSettings.Add(defaultGeneralSettings);
            context.SaveChanges();

            List<Record> files = new List<Record> 
            {
                new Record(new DateTime(2014, 2, 22, 00, 00, 00)),
                new Record(new DateTime(2014, 2, 15, 00, 00, 00))
            };
            for (int i = 0; i < files.Count(); i++)
            {
                files[i].FileUrls = new List<FileUrl> { 
                    new FileUrl {
                        Url = String.Format("/{0}/TestFile{1}.pdf", files[i].ReferenceNumber, i + 1) 
                    }
                };
                context.Records.Add(files[i]);
            }
            context.SaveChanges();

            UserProfile seedUser = new UserProfile
            {
                UserId = 1,
                UserName = "SystemSeedMethod",
                Email = "seed@system.com",
                AvatarUrl = "/Images/person-placeholder.jpg",
                Records = new List<Record> { }
            };
            foreach (Record record in context.Records)
                seedUser.Records.Add(record);
            context.UserProfiles.Add(seedUser);
            context.SaveChanges();

            int fileId1 = files[0].RecordId;
            int fileId2 = files[1].RecordId;

            new List<Comment>
            {
                new Comment("Excellent!", 
                    "<p>The response time on getting my files after payment was amazing.</p><p>Thanks so much!</p>",
                    new DateTime(2014, 1, 2, 14, 35, 00), 
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    context.Records.Single(f => f.RecordId == fileId1),
                    context.Categories.Single(c => c.Name == "Comment")),

                new Comment("You guys are amazing!",
                    "<p>I can't believe the service is so fast with this company!  Amazing!</p>",
                    new DateTime(2014, 1, 5, 11, 24, 00),
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    context.Records.Single(f => f.RecordId == fileId2),
                    context.Categories.Single(c => c.Name == "Comment"))

            }.ForEach(c => context.Comments.Add(c));
            context.SaveChanges();

            new List<Message> 
            { 
                new Message("Successfully updated record!",
                    "Record #35690D",
                    new DateTime(2014, 1, 24, 12, 14, 00),
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    context.Categories.Single(c => c.Name == "Success")),

                new Message("Attempted unauthorized access.",
                    "Record #35690D",
                    new DateTime(2014, 1, 24, 12, 7, 00),
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    context.Categories.Single(c => c.Name == "Warning")),

                new Message("Deleted record.",
                    "Record #83769A",
                    new DateTime(2014, 1, 25, 16, 3, 00),
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    context.Categories.Single(c => c.Name == "Danger")),

                new Message("User login",
                    String.Empty,
                    new DateTime(2014, 1, 25, 14, 3, 00),
                    context.UserProfiles.Single(u => u.UserName == "SystemSeedMethod"),
                    categories.Single(c => c.Name == "Standard"))

            }.ForEach(m => context.Messages.Add(m));
            context.SaveChanges();
        }
    }
}