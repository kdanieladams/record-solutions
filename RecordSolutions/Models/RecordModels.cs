using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordSolutions.Models
{
    public class Record
    {
        // Constructors
        public Record(DateTime endDate)
        {
            ReferenceNumber = GenerateRefNum();
            StartDate = DateTime.Now;
            EndDate = endDate;
        }
        public Record() { }

        // Properties
        public int RecordId { get; set; }

        [DisplayName("End Date")]
        [Column(TypeName = "DateTime2")]
        public DateTime EndDate { get; set; }

        [DisplayName("Start Date")]
        [Column(TypeName = "DateTime2")]
        public DateTime StartDate { get; set; }

        [DisplayName("Reference #")]
        public string ReferenceNumber { get; set; }

        // Foreign-key properties
        [DisplayName("File URLs")]
        public virtual List<FileUrl> FileUrls { get; set; }

        [DisplayName("Permitted Users")]
        public virtual List<UserProfile> PermittedUsers { get; set; }

        public virtual List<Comment> Comments { get; set; }

        // Helpers
        public string GenerateRefNum() 
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random(RecordId);
            string refNum = new string(Enumerable.Repeat(chars, 8)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());

            return refNum;
        }
    }

    public class FileUrl
    { 
        [Key]
        public int FileUrlId { get; set; }

        public string Url { get; set; }
    }
}