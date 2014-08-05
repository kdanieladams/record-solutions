using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordSolutions.ActionResults
{
    public class DownloadResult : ActionResult
    {
        // Constructors
        public DownloadResult() { }
        public DownloadResult(string virtualPath)
        {
            this.VirtualPath = virtualPath;
        }

        // Properties
        public string VirtualPath { get; set; }
        public string FileDownloadName { get; set; }
        
        // Override method ExecuteResult()
        public override void ExecuteResult(ControllerContext context) {
            if (!String.IsNullOrEmpty(FileDownloadName)) {
                context.HttpContext.Response.AddHeader("content-disposition",
                  "attachment; filename=" + this.FileDownloadName);
            }

            string filePath = context.HttpContext.Server.MapPath(this.VirtualPath);
            context.HttpContext.Response.TransmitFile(filePath);
        }
    }
}