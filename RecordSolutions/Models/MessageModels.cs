using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RecordSolutions.Models;
using RecordSolutions.ViewModels;

namespace RecordSolutions.Models
{
    public class Message
    {
        public Message(string title, string body, DateTime date,
            UserProfile user, Category cat)
        {
            Title = title;
            Body = body;
            Date = date;
            User = user;
            Category = cat;
        }
        public Message(string title, string body,
            UserProfile user, Category cat)
        {
            Title = title;
            Body = body;
            Date = DateTime.Now;
            User = user;
            Category = cat;
        }
        public Message(){ }
        
        public int MessageId { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }

        public virtual Category Category { get; set; }
        public virtual UserProfile User {get;set;}
    }

    public class Notification
    {
        public int NotificationId { get; set; }
        public int CategoryId { get; set; }
        public string Message { get; set; }

        public virtual Category Category { get; set; }
    }

    public class Comment : Message
    {
        public Comment(string title, string body, DateTime date, 
            UserProfile user, Record record, Category cat) : base(title, body, date, user, cat)
        {
            Record = record;
        }
        public Comment() { }

        public int CommentId { get; set; }
        public int RecordId { get; set; }
        public virtual Record Record { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}