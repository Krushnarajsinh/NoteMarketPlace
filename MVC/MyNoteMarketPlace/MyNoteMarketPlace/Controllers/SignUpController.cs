using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MyNoteMarketPlace.Models;
using System.Web.Security;
using System.Web.Helpers;

namespace MyNoteMarketPlace.Controllers
{
    public class SignUpController : Controller
    {
        /*  Create repository = null; */
        Datebase1Entities context = new Datebase1Entities();

        // GET: SignUp
        [AllowAnonymous]
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult SignUp(UsersModel model)
        {
            if (ModelState.IsValid)
            {

                var exist = CheckEmail(model.EmailID);
                if (exist)
                {
                    ModelState.AddModelError("EmailID", "This EmailID is already exist");
                    return View(model);
                }
                //Take Id Of Member From UserRoles Table
                var memberid = context.UserRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();
                Users me = new Users()
                {
                    RoleID = memberid,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailID = model.EmailID,
                    Password = model.Password,
                    IsEmailVerified = model.IsEmailVerified,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    SecretCode = Guid.NewGuid(),

                };
                context.Users.Add(me);


                context.SaveChanges();


                IsEmailVarified(model.EmailID, model.FirstName, me.SecretCode.ToString());

                TempData["Good"] = "Your account has been successfully created.";

            }
            ModelState.Clear();
            /* return View();*/
            return RedirectToAction("SignUp");

        }

        [NonAction]
        public bool CheckEmail(string emailID)
        {
            using (var context = new Datebase1Entities())
            {
                var check_email = context.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                //returns true if check_email is not null
                return check_email != null;
            }
        }
        [Route("EmailVerification/{code}")]
        [AllowAnonymous]
        public ActionResult EmailVerification(string code)
        {
            Users obj = context.Users.Where(x => x.SecretCode.ToString() == code).FirstOrDefault();
            ViewBag.name = obj.FirstName;
            return View(obj);

        }
        [Route("Verify/{code}")]
        [AllowAnonymous]
        public ActionResult Verify(string code)
        {
            Users obj = context.Users.Where(x => x.SecretCode.ToString() == code).FirstOrDefault();
            obj.IsEmailVerified = true;
            context.SaveChanges();
            return RedirectToAction("Login");
        }

        [NonAction]
        public void IsEmailVarified(string emailID, string username, string SecretCode)
        {
            var verifyUrl = "/EmailVerification/" + SecretCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var from = new MailAddress("rathodkrushnaraj8055@gmail.com");
            var to = new MailAddress(emailID);
            var Password = "********"; // Replace with actual password
            string subject = "Note Marketplace - Email Verification";


           /* var newlink = "https://localhost:44386/" + "SignUp/EmailVerification?ID=" + SecretCode;*/


            string body = "Hello " + username + "," +
           "<br/><br/>Thank you for signing up with us. Please click on below link to verify your email address and to do login." +
           "<br/><br/><a href='" + link + "'>" + "Click Here For Confirmation" + "</a> " +
           "<br/><br/>Regards,<br/>Notes Marketplace";


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from.Address, Password)
            };

            using (var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login()
        {
            // if user is already login then make user logout
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login(UserLogin obj)
        {
            //string message = "";
            using (Datebase1Entities entity = new Datebase1Entities())
            {
                var take = entity.Users.Where(a => a.EmailID == obj.EmailID).FirstOrDefault();
                if (take != null)
                {
                    if (take.IsEmailVerified == true)
                    {
                        if (string.Compare(obj.Password, take.Password) == 0)
                        {
                            int memberid = entity.UserRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();
                            //check if user member
                            if (take.RoleID == memberid)
                            {
                                int take_time = obj.RememberMe ? 525600 : 20;  // Here,525600 min = 1 year If CheckBox Is Marked Else 20 min
                                var locking = new FormsAuthenticationTicket(obj.EmailID, obj.RememberMe, take_time);
                                string styling = FormsAuthentication.Encrypt(locking);
                                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, styling);
                                cookie.Expires = DateTime.Now.AddMinutes(take_time);
                                cookie.HttpOnly = true;
                                Response.Cookies.Add(cookie);


                                // check if user profile exists or not
                                var is_userprofile_exist = context.UserProfile.Where(x => x.User_ID == take.ID).FirstOrDefault();

                                // if user profile is not exists then redirect to userprofile page else search page
                                if (is_userprofile_exist == null)
                                {
                                    return RedirectToAction("UserProfile", "UserProfile");
                                }
                                else
                                {
                                    return RedirectToAction("Search", "SearchNotes");
                                }
                            }
                            //for user admin or superadmin
                            else
                            {
                                //set authentication cookie
                                FormsAuthentication.SetAuthCookie(take.EmailID, obj.RememberMe);

                                // check if Admin profile exists or not
                                var is_userprofile_exist = context.Admin.Where(x => x.AdminID == take.ID).FirstOrDefault();

                                // if Admin profile is not exists then redirect to myprofile page else dashboard page
                                if (is_userprofile_exist == null)
                                {
                                    return RedirectToAction("MyProfile", "AdminProfile");
                                }
                                else
                                {
                                    return RedirectToAction("Dashboard", "Admin");
                                }
                              
                            }
                                
                        }


                        else
                        {
                            //message = "Invalid Password";
                            ModelState.AddModelError("Password", "Your Password Invalid");
                            return View(obj);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Your Email is not verified");
                        return View(obj);
                    }
                }
                else
                {
                    //message = "Invalid Email";
                    ModelState.AddModelError("Email", "This is Invalid Email");
                    return View(obj);
                }
            }
            //ViewBag.Message = message;
            //return View();
        }

     
        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [Route("Logout")]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }


        [HttpGet]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ForgotPassword")]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var isExist = CheckEmail(model.EmailID);
                if (isExist == false)
                {
                    ModelState.AddModelError("EmailID", "Email Address Does not exist");
                    return View(model);
                }
                Users user = context.Users.Where(x => x.EmailID == model.EmailID).FirstOrDefault();
                string new_password = Membership.GeneratePassword(8, 2);  //Rendom Password IS Generated
                user.Password = new_password;
                context.SaveChanges();

                CheckPassword(user.EmailID, new_password);

                TempData["Information"] = "New Password Has Been Sent To Your Email Address";
            }
            return RedirectToAction("ForgotPassword");
        }

        [NonAction]
        public void CheckPassword(string emailID, string pass)
        {
            var from = new MailAddress("rathodkrushnaraj8055@gmail.com");
            var to = new MailAddress(emailID);
            var Password = "*********"; // Replace with Original password
            string subject = "Note Marketplace - Email Verification";

            string body = "Hello," +
                "<br/><br/>We have generated a new password for you" +
                "<br/><br/>Password: " + pass +
                "<br/><br/>Regards,<br/>Notes Marketplace";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from.Address, Password)
            };

            using (var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }


        
        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [Route("ChangePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize(Roles = "SuperAdmin,Admin,Member")]
        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var take_email = User.Identity.Name.ToString();
            Users obj = context.Users.Where(x => x.EmailID == take_email).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (string.Compare(model.OldPassword, obj.Password) == 0)
                {
                    obj.Password = model.NewPassword;
                    obj.ModifiedDate = DateTime.Now;

                    context.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();

                    FormsAuthentication.SignOut();
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("OldPassword", "Your OldPassword Is Incorrect");
                }
            }
            return View();
        }



    }
}