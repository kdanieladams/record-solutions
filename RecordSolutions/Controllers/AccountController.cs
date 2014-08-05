using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using RecordSolutions.Models;
using RecordSolutions.ActionResults;

namespace RecordSolutions.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // Database context
        private RSEntities modelDb = new RSEntities();

        // Helper Controller
        HelperController helper = new HelperController();
        
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                UserProfile user = modelDb.UserProfiles.Single(u => u.UserName == model.UserName);
                user.LastLoginDate = DateTime.Now;
                modelDb.SaveChanges();

                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            // Get latest GeneralSettings object
            GeneralSettings settings = modelDb.GeneralSettings
                .OrderByDescending(g => g.EffectiveDate).FirstOrDefault();

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    if (modelDb.UserProfiles.SingleOrDefault(u => u.Email == model.Email) == null)
                    {
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password,
                        new { AvatarUrl = settings.DefaultAvatarUrl, Email = model.Email });

                        WebSecurity.Login(model.UserName, model.Password);

                        // Add system-log-event message
                        Message msg = new Message("Successfully created user.",
                            String.Format("User Name: {0}", model.UserName),
                            modelDb.UserProfiles.Single(u => u.UserName == model.UserName),
                            modelDb.Categories.Single(c => c.Name == "Success"));
                        modelDb.Messages.Add(msg);
                        modelDb.SaveChanges();

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Account with this email address already exists.");
                    };
                    
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));

                    // Add system-log-event message
                    Message msg = new Message("Error creating user.",
                        String.Format("User Name: {0}<br/>Error Status Code: {1}", model.UserName, e.StatusCode),
                        modelDb.UserProfiles.Single(u => u.UserName == model.UserName),
                        modelDb.Categories.Single(c => c.Name == "Warning"));
                    modelDb.Messages.Add(msg);
                    modelDb.SaveChanges();
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message, ManageModel model)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.UpdateProfileSuccess ? "Your profile has been updated."
                : message == ManageMessageId.UpdateProfileFailure ? "Unable to update your profile"
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");

            UserProfile user = modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name);

            if (model.AvatarUrl == null || model.Email == null)
            {
                model = new ManageModel
                {
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl
                };
            }

            return View(model);
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(ManageModel model, HttpPostedFileBase uploadAv)
        {
            if (ModelState.IsValid)
            {
                string avatarUrl = String.Empty;
                UserProfile user = modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name);

                // Set the new Email address
                user.Email = model.Email;

                if (model.Gravatar)
                {
                    // Handle gravatar functionality
                    avatarUrl = helper.GravatarUrl(user.Email);
                }
                else
                { 
                    // Save the posted file, get URL
                    avatarUrl = helper.UploadAvatar(uploadAv, user.UserName);
                }

                // Set the new Avatar URL
                user.AvatarUrl = avatarUrl;

                // Save changes
                modelDb.SaveChanges();

                return RedirectToAction("Manage", new { Message = ManageMessageId.UpdateProfileSuccess });
            };
            return View(model);
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    { 
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // TODO: Remove this logic path.  We don't need OAuth, so this situation should not occur.
                //
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Manage");
        }

        //
        // GET: /Account/Records
        [Authorize(Roles="Customer,Administrator")]
        public ActionResult Records()
        {
            UserProfile user = modelDb.UserProfiles.SingleOrDefault(u => u.UserName == User.Identity.Name);

            if (user == null)
                throw new HttpException(404, "User not found.");
            
            return View(user);
        }

        //
        // GET: /Account/RecordDetail/{id}
        [Authorize(Roles = "Customer,Administrator")]
        public ActionResult RecordDetail(int id)
        {
            Record record = modelDb.Records.Find(id);
            Message msg = null;
            UserProfile user = modelDb.UserProfiles.SingleOrDefault(u => u.UserName == User.Identity.Name);

            try
            {
                if (record == null)
                    return HttpNotFound();

                ViewBag.UserAvatarUrl = user.AvatarUrl;

                if (record.PermittedUsers.Any(u => u.UserId == user.UserId))
                    return View(record);
                else
                    throw new HttpException(403, "Attempted unauthorized access to record.");
            }
            catch (HttpException ex)
            { 
                // Add system-log-event message
                msg = new Message("Attempted unauthorized record access",
                    String.Format("Record Reference #: <a href='{0}'>{1}</a><br/>Error: {2}<br/>{3}",
                        Url.Action("RecordDetail", "Mgmt", new { id = record.RecordId }), record.ReferenceNumber,
                        ex.ErrorCode, ex.Message),
                    modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                    modelDb.Categories.Single(c => c.Name == "Warning"));
                modelDb.Messages.Add(msg);
                modelDb.SaveChanges();

                return RedirectToAction("Records", "Account");
            }
        }

        //
        // POST: /Account/RecordDetail/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer,Administrator")]
        public ActionResult RecordDetail(int id, string cmt, string title)
        {
            Record record = modelDb.Records.Find(id);
            UserProfile user = modelDb.UserProfiles.SingleOrDefault(u => u.UserName == User.Identity.Name);
            Category cmtCat = modelDb.Categories.SingleOrDefault(c => c.Name == "Comment");

            try 
            {
                Comment comment = new Comment(title, cmt, DateTime.Now, user, record, cmtCat);

                modelDb.Comments.Add(comment);
                modelDb.SaveChanges();
            }
            catch 
            { 
                //Haven't run into any error's yet...we'll cross that bridge when we get there.
            }

            ViewBag.UserAvatarUrl = user.AvatarUrl;

            return View(record);
        }

        //
        // GET: /Account/RecordDownload/{id}
        [Authorize(Roles = "Customer,Administrator")]
        public ActionResult RecordDownload(int id, string url)
        {
            Record record = modelDb.Records.Find(id);   // The record we're looking for
            Message msg = null;                         // System log message

            if (record.PermittedUsers.Any(u => u.UserName == User.Identity.Name))
            {
                // Find the file we're looking for
                for (int i = 0; i < record.FileUrls.Count; i++)
                {
                    if (record.FileUrls[i].Url == url)
                    {
                        // Add system-log-event message
                        msg = new Message("Authorized record access",
                            String.Format("Record Reference #: <a href='{0}'>{1}</a>",
                            Url.Action("RecordDetail", "Mgmt", new { id = record.RecordId }), record.ReferenceNumber),
                            modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                            modelDb.Categories.Single(c => c.Name == "Success"));
                        modelDb.Messages.Add(msg);
                        modelDb.SaveChanges();

                        // Return the file
                        return new DownloadResult
                        {
                            VirtualPath = String.Format("~/App_Data/Uploads{0}", record.FileUrls[i].Url),
                            FileDownloadName = Path.GetFileName(record.FileUrls[i].Url)
                        };
                    }
                }
            }

            // Add system-log-event message
            msg = new Message("Attempted unauthorized record access",
                String.Format("Record Reference #: <a href='{0}'>{1}</a>",
                Url.Action("RecordDetail", "Mgmt", new { id = record.RecordId }), record.ReferenceNumber),
                modelDb.UserProfiles.Single(u => u.UserName == User.Identity.Name),
                modelDb.Categories.Single(c => c.Name == "Warning"));
            modelDb.Messages.Add(msg);
            modelDb.SaveChanges();

            // Redirect to the home page, because this user is not allowed
            // to download this file.
            return RedirectToAction("Index", "Home");            
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            UpdateProfileSuccess,
            UpdateProfileFailure,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
