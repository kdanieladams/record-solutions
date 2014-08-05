using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using RecordSolutions.Models;

namespace RecordSolutions.Controllers
{
    public class HelperController : Controller
    {
        // DbContext
        private RSEntities modelDb = new RSEntities();

        // GravatarUrl()
        // -------------
        // Accepts a string presumed to be an email address, and returns
        // an appropriate gravatar URL string.
        public string GravatarUrl(string email)
        {
            string gravatarUrl = String.Empty;

            // Get latest GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            using (System.Security.Cryptography.MD5 md5hash = System.Security.Cryptography.MD5.Create())
            {
                // Convert user.Email string to a byte array and compute the hash. 
                byte[] data = md5hash.ComputeHash(Encoding.UTF8.GetBytes(email));

                // Create a new Stringbuilder to collect the bytes 
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Output sBuilder to a string
                string emailHash = sBuilder.ToString();

                // Construct appropriate Gravatar URL with hash
                gravatarUrl = String.Format("{0}{1}?s=200", // s=200 will return a 200px by 200px image
                    settings.GravatarUrl, emailHash);
            }

            return gravatarUrl;
        }

        // UploadAvatar()
        // --------------
        // Accepts an HttpPostedFileBase and a user name, checks to make
        // sure the file is an image, then saves the file to disk and 
        // returns a publicly accessible url to the file.
        public string UploadAvatar(HttpPostedFileBase hpf, string username)
        {
            string avatarUrl = String.Empty;    // String to hold url
            bool isImage = false;               // Track whether uploaded file is an image

            // Get latest GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            try 
            {
                // Make sure user actually uploaded a file
                if (hpf == null)
                    avatarUrl = settings.DefaultAvatarUrl;
                else
                {
                    // Make sure the uploaded file is an image
                    foreach (FileExtension type in settings.AllowedImageTypes)
                        if (Path.GetExtension(hpf.FileName) == type.Ext) isImage = true;

                    if (isImage)
                    {
                        // Determine the path on disk we want to save the file to
                        string savedFilePath = Path.Combine(HostingEnvironment.MapPath("~/Images/Avatars"),
                            String.Format("{0}-{1}", username, Path.GetFileName(hpf.FileName)));

                        // Save the file to disk
                        hpf.SaveAs(savedFilePath);

                        // Set the avatarUrl string
                        avatarUrl = "/Images/Avatars/"
                            + String.Format("{0}-{1}", username, Path.GetFileName(hpf.FileName));
                    }
                    else
                        throw new DataException("Attempt to upload incorrect file type (must be an image).");
                }
            }
            catch (DataException dex)
            {
                // ModelState.AddModelError("", dex.ToString());
                return settings.DefaultAvatarUrl;
            };

            return avatarUrl;
        }

        // UploadRecordFile()
        // ------------------
        // Accepts a file and a record to assign it to.
        public void UploadRecordFile(HttpPostedFileBase hpf, Record record)
        {
            string ret = String.Empty;
            string refNum = record.ReferenceNumber;
            
            // Make sure file's extension is *.pdf
            if (Path.GetExtension(hpf.FileName) != ".pdf")
                throw new DataException("Attempt to upload incorrect file type (must be *.pdf).");

            // Find file's path on disk
            string savedFilePath = Path.Combine(HostingEnvironment.MapPath("~/App_Data/Uploads"),
                String.Format("{0}/{1}", record.ReferenceNumber, Path.GetFileName(hpf.FileName)));

            // Set/update the file's FileUrls
            if (record.FileUrls != null)
            {
                record.FileUrls.Add(new FileUrl
                {
                    Url = String.Format("/{0}/{1}",
                        record.ReferenceNumber, Path.GetFileName(hpf.FileName))
                });
            }
            else
            {
                record.FileUrls = new List<FileUrl> 
                { 
                    new FileUrl
                    {
                        Url = String.Format("/{0}/{1}",
                            record.ReferenceNumber, Path.GetFileName(hpf.FileName))
                    }
                };
            }

            // Save the file to disk
            hpf.SaveAs(savedFilePath);
        }
    }
}
