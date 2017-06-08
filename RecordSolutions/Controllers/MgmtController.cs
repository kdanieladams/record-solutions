using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using PagedList;
using PagedList.Mvc;
using RecordSolutions.Models;
using RecordSolutions.ViewModels;

namespace RecordSolutions.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class MgmtController : Controller
    { 
        // DbContext
        private RSEntities modelDb = new RSEntities();

        // Helper Controller
        private HelperController helper = new HelperController();

        #region Private methods and variables
        // Empty list for holding fading notifications in dashboard
        private List<Notification> notices = new List<Notification> { };

        // AddNotification()
        // ------------------
        // Adds a notification to the list of notifications
        private void AddNotification(string msg, int catId, bool insert)
        {
            if (catId <= 0)
                catId = 1;

            Notification newNotice = new Notification { 
                Category = modelDb.Categories.Find(catId),
                Message = msg
            };
            if (insert)
                notices.Insert(0, newNotice);
            else
                notices.Add(newNotice);
        }

        // InitMgmtIndexViewModel()
        // ------------------------
        // Initializes and returns a fresh instance of MgmtIndexViewModel
        private MgmtIndexViewModel InitMgmtIndexViewModel()
        {
            // Get a few messages from last 30 days
            DateTime thirtyDays = DateTime.Today.AddDays(-331);
            DateTime ninetyDays = DateTime.Today.AddDays(-390);
            IQueryable<Message> qry1 = (from message in modelDb.Messages
                                            where message.Date > thirtyDays && message.Category.Name != "Comment"
                                            orderby message.Date descending
                                            select message).Take(5);
            IQueryable<Comment> qry2 = (from comment in modelDb.Comments
                                            where comment.Date > ninetyDays
                                            orderby comment.Date descending
                                            select comment).Take(3);
            IQueryable<Record> qry3 = from record in modelDb.Records
                                          where record.StartDate > thirtyDays
                                          orderby record.StartDate descending
                                          select record;

            // Convert query results to lists
            List<Message> msgs = new List<Message> { };
            foreach (Message msg in qry1)
                msgs.Add(msg);

            List<Comment> cmts = new List<Comment> { };
            foreach (Comment cmt in qry2)
                cmts.Add(cmt);

            List<Record> recs = new List<Record> { };
            foreach (Record rec in qry3)
                recs.Add(rec);

            // Get number of users currently online
            int numUsersOnline = (int)HttpContext.Application["OnlineUsers"];
            
            // Get current user
            UserProfile user = modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name);

            // Instantiate ViewModel 
            MgmtIndexViewModel viewModel = new MgmtIndexViewModel
            {
                UserName = user.UserName,
                UserAvatarUrl = user.AvatarUrl,
                NumUsers = modelDb.UserProfiles.Count(),
                NumMessages = modelDb.Messages.Count(),
                NumRecords = modelDb.Records.Count(),
                NumUsersOnline = numUsersOnline,
                Notices = notices,
                Messages = msgs,
                Comments = cmts,
                Records = recs
            };
            return viewModel;
        }
        #endregion

        #region Query Builders
        // MessageQueryBuilder()
        // ---------------------
        // Accepts parameters to output a customizable LINQ query of Messages
        private IPagedList<Message> MessageQueryBuilder(ViewFilter? filter, string oldFilter, int? page)
        {
            IPagedList<Message> ret;
            IQueryable<Message> qry = modelDb.Messages.Where(m => m.Category.Name != "Comment");

            if (filter != null)
            {
                switch (filter)
                {
                    case ViewFilter.category:
                        qry = filter.ToString() != oldFilter ? 
                            qry.OrderBy(m => m.Category.Name) : 
                            qry.OrderByDescending(m => m.Category.Name);
                        break;
                    case ViewFilter.title:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(m => m.Title) :
                            qry.OrderByDescending(m => m.Title);
                        break;
                    case ViewFilter.user:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(m => m.User.UserName) :
                            qry.OrderByDescending(m => m.User.UserName);
                        break;
                    case ViewFilter.date:
                    default:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderByDescending(m => m.Date) :
                            qry.OrderBy(m => m.Date);
                        break;
                };
            }
            else
            {
                qry = filter.ToString() != oldFilter ?
                    qry.OrderByDescending(m => m.Date) :
                    qry.OrderBy(m => m.Date);
            }

            // Convert qry to a paged list of 25 messages per page
            ret = qry.ToList().ToPagedList(page ?? 1, 25);
                 
            return ret;
        }

        // RecordQueryBuilder()
        // ---------------------
        // Accepts parameters to output a customizable LINQ query of Records
        private IPagedList<Record> RecordQueryBuilder(ViewFilter? filter, string oldFilter, int? page)
        {
            IPagedList<Record> ret;
            IQueryable<Record> qry = modelDb.Records.Where(f => f.RecordId != 0);

            if (filter != null)
            {
                switch (filter)
                {
                    case ViewFilter.id:
                    default:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(f => f.RecordId) :
                            qry.OrderByDescending(f => f.RecordId);
                        break;
                    case ViewFilter.refNum:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(f => f.ReferenceNumber) :
                            qry.OrderByDescending(f => f.ReferenceNumber);
                        break;
                    case ViewFilter.startDate:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderByDescending(f => f.StartDate) :
                            qry.OrderBy(f => f.StartDate);
                        break;
                    case ViewFilter.endDate:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderByDescending(f => f.EndDate) :
                            qry.OrderBy(f => f.EndDate);
                        break;
                    case ViewFilter.numUsers:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderByDescending(f => f.PermittedUsers.Count()) :
                            qry.OrderBy(f => f.PermittedUsers.Count());
                        break;
                };
            }
            else
            {
                qry = filter.ToString() != oldFilter ?
                    qry.OrderBy(f => f.RecordId) :
                    qry.OrderByDescending(f => f.RecordId);
            }

            ret = qry.ToList().ToPagedList(page ?? 1, 25);

            return ret;
        }

        // UserQueryBuilder()
        // ---------------------
        // Accepts parameters to output a customizable LINQ query of Users
        private IPagedList<UserProfile> UserQueryBuilder(ViewFilter? filter, string oldFilter, int? page)
        {
            IPagedList<UserProfile> ret;
            IQueryable<UserProfile> qry = modelDb.UserProfiles.Where(u => u.UserId != 0);

            if (filter != null)
            {
                switch (filter)
                {
                    case ViewFilter.id:
                    default:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(u => u.UserId) :
                            qry.OrderByDescending(u => u.UserId);
                        break;
                    case ViewFilter.user:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(u => u.UserName) :
                            qry.OrderByDescending(u => u.UserName);
                        break;
                    case ViewFilter.numRecords:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderByDescending(u => u.Records.Count()) :
                            qry.OrderBy(u => u.Records.Count());
                        break;
                    case ViewFilter.email:
                        qry = filter.ToString() != oldFilter ?
                            qry.OrderBy(u => u.Email) :
                            qry.OrderByDescending(u => u.Email);
                        break;
                };
            }
            else
            {
                qry = filter.ToString() != oldFilter ?
                    qry.OrderBy(u => u.UserId) :
                    qry.OrderByDescending(u => u.UserId);
            }

            ret = qry.ToList().ToPagedList(page ?? 1, 25);

            return ret;
        }
        #endregion

        #region Index/Dashboard
        //
        // GET: /Mgmt/
        public ActionResult Index(string msg, int? catId)
        {
            if (msg != null && msg.Length > 0 && catId != null)
            {
                // Add the passed in message
                AddNotification(msg, (int)catId, true);
            }
            // Add a couple of useful tidbits of information to Notifications
            AddNotification("You have been granted Administrative priveledges", 1, false);
            AddNotification(String.Format("We have {0} registered users.",
                modelDb.UserProfiles.Count()), 1, false);
            AddNotification(String.Format("Currently, {0} messages are stored in the database.",
                modelDb.Messages.Count()), 1, false);
            
            MgmtIndexViewModel viewModel = InitMgmtIndexViewModel();

            ViewBag.CatColors = new List<string> { };

            foreach (Category cat in modelDb.Categories)
                ViewBag.CatColors.Add(cat.Color);
            
            return View(viewModel);
        }

        //
        // GET: /Mgmt/Dashboard
        public ActionResult Dashboard()
        {
            MgmtIndexViewModel viewModel = InitMgmtIndexViewModel();
            return View(viewModel);
        }
        #endregion

        #region Messages
        //
        // GET: /Mgmt/Messages?filter=ViewFilter.value
        public ActionResult Messages(ViewFilter? filter, string oldFilter, int? page) 
        {
            IPagedList<Message> msgs = MessageQueryBuilder(filter, oldFilter, page);
            
            ViewBag.Filter = filter.ToString();
            ViewBag.Reverse = filter.ToString() != oldFilter ? false : true;
            
            return View(msgs);
        }

        //
        // GET: /Mgmt/MessageDetail?msgId={id}
        public JsonResult MessageDetail(int msgId)
        {
            modelDb.Configuration.ProxyCreationEnabled = false;
            Message msg = modelDb.Messages
                .Include("Category")
                .Include("User")
                .Single(m => m.MessageId == msgId);
            modelDb.Configuration.ProxyCreationEnabled = true;
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Records
        //
        // GET: /Mgmt/Records
        public ActionResult Records(ViewFilter? filter, string oldFilter, int? page)
        {
            IPagedList<Record> recs = RecordQueryBuilder(filter, oldFilter, page);

            ViewBag.Filter = filter.ToString();
            ViewBag.Reverse = filter.ToString() != oldFilter ? false : true;
            
            return View(recs);
        }

        //
        // GET: /Mgmt/RecordDetail/{id}
        public ActionResult RecordDetail(int id = 0)
        {
            Models.Record record = modelDb.Records.Find(id);
            if (record == null)
            {
                return HttpNotFound();
            }
            return View(record);
        }

        //
        // GET: /Mgmt/RecordEdit/{id}
        public ActionResult RecordEdit(int id = 0)
        {
            Models.Record record = modelDb.Records.Find(id);
            if (record == null)
            {
                return HttpNotFound();
            }
            // Pass a list of available users for the PermittedUsers selectbox
            ViewBag.Users = modelDb.UserProfiles;
            return View(record);
        }
        //
        // POST: /Mgmt/RecordEdit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordEdit(Record record, FormCollection coll)
        {
            try
            {
                // Popuplate FilUrls list with current values
                record.FileUrls = modelDb.Records.Find(record.RecordId).FileUrls;

                // Save record to db (is this necessary?)
                modelDb.SaveChanges();

                // Find the record in the database instead of the passed in object
                record = modelDb.Records.Single(f => f.RecordId == record.RecordId);

                if (coll["PermittedUserIds"] != null)
                {
                    // Initialize empty list in record.PermittedUsers
                    record.PermittedUsers = new List<UserProfile> { };

                    // Convert the FormCollection value for PermittedUsers selectbox to a list
                    List<string> permittedUserIds = new List<string> { };
                    permittedUserIds.AddRange(coll["PermittedUserIds"].Split(','));

                    foreach (string userId in permittedUserIds)
                    {
                        // Convert userId string to an int
                        int userIdInt = Convert.ToInt32(userId);    

                        // Find the user referenced by permittedUserIds
                        UserProfile permittedUser = modelDb.UserProfiles.Single(u => u.UserId == userIdInt);

                        // If user does not already have access, grant them access
                        if (permittedUser.Records == null || !permittedUser.Records.Any(f => f.RecordId == record.RecordId))
                            permittedUser.Records.Add(modelDb.Records.Single(f => f.RecordId == record.RecordId));

                        // Save changes to permittedUser
                        modelDb.SaveChanges();
                    }

                    // Remove users which are currently related to record, but not selected 
                    foreach (UserProfile user in record.PermittedUsers.ToList())
                    {
                        if (!permittedUserIds.Any(u => Convert.ToInt32(u) == user.UserId))
                        {
                            record.PermittedUsers.Remove(user);
                            user.Records.Remove(record);

                            // Save changes to record and user
                            modelDb.SaveChanges();
                        }
                    }
                }
                else 
                {
                    // Remove all associations between this record and any user
                    if (record.PermittedUsers != null)
                    {
                        foreach (UserProfile user in record.PermittedUsers)
                        {
                            // Remove the record from each user in record.PermittedUsers
                            user.Records.Remove(record);
                            modelDb.Entry(user).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Remove the record from every UserProfile.Files list in the database
                        foreach (UserProfile user in modelDb.UserProfiles)
                        {
                            if (user.Records.Any(f => f.RecordId == record.RecordId))
                            {
                                user.Records.Remove(record);
                                modelDb.Entry(user).State = EntityState.Modified;
                            }
                        }
                    }

                    // Empty the record.PermittedUsers list
                    record.PermittedUsers = new List<UserProfile> { };
                    modelDb.Entry(record).State = EntityState.Modified;

                    // Save changes
                    modelDb.SaveChanges();
                };

                // Add system-log-event message
                Message msg = new Message("Successfully edited record",
                    String.Format("Record Reference #:<br/><a href='{0}'>{1}</a>",
                    Url.Action("RecordDetail", new { id = record.RecordId }), record.ReferenceNumber),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Success"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                return RedirectToAction("Index", new { 
                        msg = String.Format("Successfully edited record #{0}", record.RecordId), 
                        catId = 2
                });
            }
            catch (DataException /* dex */)
            {
                //Log the error
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(record);
        }

        //
        // GET: /Mgmt/RecordDelete/{id}
        public ActionResult RecordDelete(bool? saveChangesError = false, int id = 0)
        {
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            Models.Record record = modelDb.Records.Find(id);
            if (record == null)
            {
                return HttpNotFound();
            }
            return View(record);
        }
        //
        // POST: /Mgmt/RecordDelete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordDelete(int id)
        {
            // Find the specified record in the dbContext
            Models.Record record = modelDb.Records.Find(id);

            try
            {
                // Remove the record(s) from the hard disk
                foreach(FileUrl fileUrl in record.FileUrls)
                    System.IO.File.Delete(Server.MapPath(fileUrl.Url));

                // Delete the directory the record was stored in
                if (System.IO.Directory.Exists(Server.MapPath("~/App_Data/Uploads/" + record.ReferenceNumber)))
                    Directory.Delete(Server.MapPath("~/App_Data/Uploads/" + record.ReferenceNumber));

                // Add system-log-event message
                Message msg = new Message("Deleted record.",
                    String.Format("Record Reference #:<br/>{0}", record.ReferenceNumber),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Danger"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                // Remove the record from dbContext, save changes
                modelDb.Records.Remove(record);
                modelDb.SaveChanges();
                return RedirectToAction("Index", new { 
                    msg = String.Format("Deleted record #{0}", record.ReferenceNumber), 
                    catId = 4 
                });
            }
            catch (Exception ex)
            {
                // Add system-log-event message
                Message msg = new Message("Failed to delete record.",
                    String.Format("Record Reference #: {0}<p>{1}</p><p>{2}</p>", record.ReferenceNumber,
                        ex.Source, ex.Message),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Warning"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                return RedirectToAction("Index", new {
                    msg = String.Format("Record deletion failed for record #{0}",
                            modelDb.Records.Find(id).ReferenceNumber),
                    catId = 3
                });
            }
        }

        //
        // GET: /Mgmt/RecordCreate
        public ActionResult RecordCreate()
        {
            // Pass a list of available users for the PermittedUsers selectbox
            ViewBag.Users = modelDb.UserProfiles;
            return View();
        }
        //
        // POST: /Mgmt/RecordCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordCreate(Models.Record record, FormCollection coll)
        {
            try
            {
                // Set the record's start date to today
                record.StartDate = DateTime.Now;

                // Set the record's reference number to a new reference number
                record.ReferenceNumber = record.GenerateRefNum();

                // Setup a new directory for this new Models.Record
                Directory.CreateDirectory(Path.Combine(
                    Server.MapPath("~/App_Data/Uploads"), record.ReferenceNumber));

                // Setup and accept file upload(s) 
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    // Assign this file to hpf variable
                    HttpPostedFileBase hpf = Request.Files[i] as HttpPostedFileBase;

                    // Upload the file
                    helper.UploadRecordFile(hpf, record);
                }

                if (ModelState.IsValid)
                {
                    // Add record to context and save context
                    modelDb.Records.Add(record);
                    modelDb.SaveChanges();

                    // Handle PermittedUsers list
                    if (coll["PermittedUserIds"].Length > 0)
                    {
                        // Initialize empty list in record.PermittedUsers
                        record.PermittedUsers = new List<UserProfile> { };
                        // Convert the FormCollection value for PermittedUsers selectbox to a list
                        List<string> permittedUserIds = new List<string> { };
                        permittedUserIds.AddRange(coll["PermittedUserIds"].Split(','));

                        foreach (string userId in permittedUserIds)
                        {
                            // Convert permittedUserIds string to an int
                            int userIdInt = Convert.ToInt32(userId);
                            // Find the user referenced by permittedUserIds
                            UserProfile permittedUser = modelDb.UserProfiles.Single(u => u.UserId == userIdInt);
                            // Grant user access
                            permittedUser.Records.Add(modelDb.Records.Single(f => f.RecordId == record.RecordId));
                        }

                        modelDb.SaveChanges();
                    }

                    // Add system-log-event message
                    Message msg = new Message("Successfully created record.",
                        String.Format("Record Reference #:<br/><a href='{0}'>{1}</a>",
                            Url.Action("RecordDetail", new { id = record.RecordId }), record.ReferenceNumber),
                        modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                        modelDb.Categories.Single(c => c.Name == "Success"));
                    modelDb.Messages.Add(msg);
                    modelDb.SaveChanges();

                    return RedirectToAction("Index", new { 
                        msg = String.Format("Successfully created record #{0}", record.RecordId),
                        catId = 2
                    });
                }
            }
            catch (Exception ex)
            {
                // Add system-log-event message
                Message msg = new Message("Failed to create record.",
                    String.Format("<p>{0}</p><p>{1}</p>", 
                        ex.Source, ex.Message),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Warning"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                return RedirectToAction("Index", new
                {
                    msg = String.Format("Record creation failed."),
                    catId = 3
                });
            }
            return View(record);
        }
        #endregion

        #region UserAdmin
        //
        // GET: /Mgmt/UserAdmin
        public ActionResult UserAdmin(ViewFilter? filter, string oldFilter, int? page)
        {
            IPagedList<UserProfile> users = UserQueryBuilder(filter, oldFilter, page);

            ViewBag.Filter = filter.ToString();
            ViewBag.Reverse = filter.ToString() != oldFilter ? false : true;
            
            return View(users);
        }

        //
        // GET: /Mgmt/UserDetail/{id}
        public ActionResult UserDetail(int id = 0)
        {
            UserProfile user = modelDb.UserProfiles.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Setup the ViewBag
            string regDateQry = String.Format("select CreateDate from dbo.webpages_Membership where UserId = {0}", id);
            ViewBag.RegistrationDate = modelDb.Database.SqlQuery<DateTime>(regDateQry).SingleOrDefault();
            ViewBag.LastLoginDate = user.LastLoginDate == null ? ViewBag.RegistrationDate : (DateTime)user.LastLoginDate;

            return View(user);
        }

        //
        // GET: /Mgmt/UserEdit/{id}
        public ActionResult UserEdit(int id = 0)
        {
            UserProfile user = modelDb.UserProfiles.Find(id);

            // Get latest GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings.Include("AllowedImageTypes")
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Records = modelDb.Records;
            ViewBag.ImgExts = settings.AllowedImageTypes;
            
            return View(user);
        }

        //
        // POST: /Mgmt/UserEdit/{id}
        [HttpPost]
        public ActionResult UserEdit(UserProfile user, FormCollection coll, HttpPostedFileBase uploadAv)
        {
            try 
            {
                if (ModelState.IsValid)
                {
                    // Assign user variable to database object instead of parameter object
                    user = modelDb.UserProfiles.Single(u => u.UserId == user.UserId);

                    // Apply changes to permitted files list
                    if (coll["recordIds"] != null)
                    {
                        // Make a list of strings of the fileIds
                        List<string> recordIds = new List<string> { };
                        recordIds.AddRange(coll["recordIds"].Split(','));

                        foreach (string fileId in recordIds)
                        {
                            // Convert fileId string to int
                            int fileIdInt = Convert.ToInt32(fileId);

                            // Make sure fileIdInt is not already in user.Records
                            if (user.Records == null || !user.Records.Any(f => f.RecordId == fileIdInt))
                            {
                                // Find record by fileIdInt
                                Models.Record record = modelDb.Records.Single(f => f.RecordId == fileIdInt);
                                // Add record to user.Files list
                                user.Records.Add(record);
                                // Save changes to user
                                modelDb.SaveChanges();
                            }
                        }

                        // Remove files which are currently related but not selected to user
                        foreach (Models.Record record in user.Records.ToList())
                        {
                            if (!recordIds.Any(f => Convert.ToInt32(f) == record.RecordId))
                            {
                                user.Records.Remove(record);
                                record.PermittedUsers.Remove(user);
                                // Save changes to user and record
                                modelDb.SaveChanges();
                            }
                        }

                        
                    }
                    else 
                    {
                        // Remove all associations between this user and any record
                        if (user.Records != null)
                        {
                            foreach (Models.Record record in user.Records)
                            {
                                // Remove the user from each record in user.Records
                                record.PermittedUsers.Remove(user);
                                modelDb.Entry(record).State = EntityState.Modified;
                            }
                        }
                        else 
                        {
                            foreach (Models.Record record in modelDb.Records)
                            {
                                // Remove the user from each record in the database
                                if (record.PermittedUsers.Any(u => u.UserId == user.UserId))
                                {
                                    record.PermittedUsers.Remove(user);
                                    modelDb.Entry(record).State = EntityState.Modified;
                                }
                            }
                        }

                        // Empty the user.Records list
                        user.Records = new List<Models.Record> { };

                        // Save changes
                        modelDb.Entry(user).State = EntityState.Modified;
                        modelDb.SaveChanges();
                    }

                    // String to hold the new avatar URL
                    string avatarUrl = String.Empty;

                    if (coll["Gravatar"] == "true,false")
                    {
                        // Handle Gravatar functionality
                        avatarUrl = helper.GravatarUrl(user.Email);
                    }
                    else
                    {
                        // Handle record upload
                        avatarUrl = helper.UploadAvatar(uploadAv, user.UserName);
                    }

                    // Save changes to user
                    user.AvatarUrl = avatarUrl;
                    modelDb.SaveChanges();

                    // Add system-log-event message
                    string msgBody = String.Format("User Id: {0}<br/><a href='{1}'>{2}</a><br/>",
                        user.UserId, Url.Action("UserDetail", new { id = user.UserId }), user.UserName);

                    Message msg = new Message("Successfully edited user",
                        msgBody,
                        modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                        modelDb.Categories.Single(c => c.Name == "Success"));

                    modelDb.Messages.Add(msg);
                    modelDb.SaveChanges();

                    // Return to the landing page
                    return RedirectToAction("Index", new { 
                        msg = String.Format("Successfully edited user {0}", user.UserName),
                        catId = 2
                    });
                }
            }
            catch (DataException) 
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.Files = modelDb.Records;
            return View(user);
        }

        //
        // GET: /Mgmt/UserDelete/{id}
        public ActionResult UserDelete(bool? saveChangesError = false, int id = 0)
        {
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            UserProfile user = modelDb.UserProfiles.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            string regDateQry = String.Format("select CreateDate from dbo.webpages_Membership where UserId = {0}", id);
            ViewBag.RegistrationDate = modelDb.Database.SqlQuery<DateTime>(regDateQry).SingleOrDefault();
            ViewBag.LastLoginDate = user.LastLoginDate == null ? ViewBag.RegistrationDate : (DateTime)user.LastLoginDate;

            return View(user);
        }

        //
        // POST: /Mgmt/UserDelete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserDelete(int id)
        {
            try 
            {
                // Find the specified user in the dbContext
                UserProfile user = modelDb.UserProfiles.Find(id);

                // Add system-log-event message
                Message msg = new Message("Deleted user.",
                    String.Format("User ID #: {0}<br/>User Name: {1}<br/>Email: {2}",
                        user.UserId, user.UserName, user.Email),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Danger"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                // Remove the user from the dbContext and save changes
                modelDb.UserProfiles.Remove(user);
                modelDb.SaveChanges();
            }
            catch 
            {
                return RedirectToAction("UserDelete", new { id = id, saveChangesError = true });
            };
            return RedirectToAction("Index", new {
                msg = String.Format("Deleted user {0}", modelDb.UserProfiles.Find(id).UserName),
                catId = 4
            });
        }
        #endregion

        #region Settings
        //
        // GET: /Mgmt/Settings
        public ActionResult Settings()
        {
            // Get current GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings.Include("AllowedImageTypes")
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            return View(settings);
        }
        //
        // POST: /Mgmt/Settings
        [HttpPost]
        public ActionResult Settings(GeneralSettings newSettings, FormCollection coll)
        {
            // Set the effective date to now
            newSettings.EffectiveDate = DateTime.Now;

            // Handle the list of allowed image extensions
            if (coll["AllowedImageTypes"] != null)
            {
                // Make a list of strings of the fileIds
                List<string> imageTypes = new List<string> { };
                imageTypes.AddRange(coll["AllowedImageTypes"].Split(','));

                foreach (string ext in imageTypes)
                {
                    FileExtension imageExt = new FileExtension { Ext = ext };
                    newSettings.AllowedImageTypes.Add(imageExt);
                }
            }

            // Add the new GeneralSettings object 
            // (should be used automatically, as it will have the latest effective date)
            modelDb.GeneralSettings.Add(newSettings);
            modelDb.SaveChanges();

            // Add system-log-event message
            Message msg = new Message("Updated General Settings.",
                String.Format(""),
                modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                modelDb.Categories.Single(c => c.Name == "Success"));
            modelDb.Messages.Add(msg);
            modelDb.SaveChanges();

            return RedirectToAction("Index", new { msg = "Successfully updated settings.", catId = 2 });
        }
        #endregion
    }
}
