using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RecordSolutions.Models
{
    public class GeneralSettings 
    {
        // Constructors
        public GeneralSettings(string defaultAvUrl, string gravatarUrl, List<FileExtension> allowedImgTypes)
        {
            EffectiveDate = DateTime.Now;
            DefaultAvatarUrl = defaultAvUrl;
            GravatarUrl = gravatarUrl;
            AllowedImageTypes = allowedImgTypes;
        }
        public GeneralSettings() { }

        // Properties
        [Key]
        public int SettingsId { get; set; }

        [DisplayName("Default Avatar URL")]
        public string DefaultAvatarUrl { get; set; }

        [DisplayName("Gravatar Base-URL")]
        public string GravatarUrl { get; set; }

        [DisplayName("Effective Date")]
        [Column(TypeName = "DateTime2")]
        public DateTime EffectiveDate { get; set; }
        
        // Virtual properties
        [DisplayName("Allowed image extensions")]
        public virtual List<FileExtension> AllowedImageTypes { get; set; }
    }

    public class FileExtension
    {
        // Properties
        [Key]
        public int Id { get; set; }

        [DisplayName("Extension string")]
        public string Ext { get; set; }

        // Method overrides
        public override string ToString()
        {
            return String.Format("{0}", Ext);
        }
    }
}