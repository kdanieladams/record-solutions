using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecordSolutions.Models;

namespace RecordSolutions.ViewModels
{
    public enum ViewFilter
    {
        id,
        category,
        title,
        date,
        startDate,
        endDate,
        user,
        numUsers,
        numRecords,
        refNum,
        email
    }

    public class MgmtIndexViewModel
    {
        public string UserName { get; set; }
        public string UserAvatarUrl { get; set; }
        public int NumUsers { get; set; }
        public int NumMessages { get; set; }
        public int NumRecords { get; set; }
        public int NumUsersOnline { get; set; }
        public List<Notification> Notices { get; set; }
        public List<Message> Messages { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Record> Records { get; set; }
    }
}