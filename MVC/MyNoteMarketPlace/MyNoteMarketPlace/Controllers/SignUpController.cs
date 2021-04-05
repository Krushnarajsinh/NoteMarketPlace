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
        public SignUpController() {
          
        }
        // GET: SignUp
        public ActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(UsersModel model)
        {
           
            


                if (ModelState.IsValid)
                {

                    var isExist = IsEmailExist(model.EmailID);
                    if (isExist)
                    {
                        ModelState.AddModelError("EmailID", "This EmailID already exist");
                        return View(model);
                    }
                    Users user = new Users()
                    {
                        RoleID = 3,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailID = model.EmailID,
                        Password =model.Password,
                        IsEmailVerified = model.IsEmailVerified,
                        SecretCode = Guid.NewGuid(),
                        IsActive = true,

                    };
                    context.Users.Add(user);
               

                    context.SaveChanges();
                
               
                  SendVerificationLinkEmail(model.EmailID, model.FirstName, user.SecretCode.ToString());

                TempData["Success"] = "Your account has been successfully created.";
               
            }
            ModelState.Clear();
            /* return View();*/
            return RedirectToAction("SignUp");

        }

        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using (var context = new Datebase1Entities())
            {
                var v = context.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }
        [Route("EmailVerification/{code}")]
        public ActionResult EmailVerification(string code) {
            Users obj = context.Users.Where(x => x.SecretCode.ToString() == code).FirstOrDefault();
            ViewBag.name = obj.FirstName;
            return View(obj);
           
        }
        [Route("Verify/{code}")]
        public ActionResult Verify(string code)
        {
            Users obj = context.Users.Where(x => x.SecretCode.ToString() == code).FirstOrDefault();
            obj.IsEmailVerified = true;
            context.SaveChanges();
            return RedirectToAction("Login");
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string username, string SecretCode)
        {
            var verifyUrl = "/EmailVerification/" + SecretCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("rathodkrushnaraj8055@gmail.com");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "rathod8055"; // Replace with actual password
            string subject = "Note Marketplace - Email Verification";

          
            var newlink= "https://localhost:44386/" + "SignUp/EmailVerification?ID=" + SecretCode;


            string body = "Hello " + username + "," +
           "<br/><br/>Thank you for signing up with us. Please click on below link to verify your email address and to do login." +
           "<br/><br/><a href='" + newlink + "'>" + "Click Here For Confirmation" + "</a> " +
           "<br/><br/>Regards,<br/>Notes Marketplace";


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }
        [HttpGet]
        [Route("Login")]
        public ActionResult Login() {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Login")]
        public ActionResult Login(UserLogin obj, string ReturnUrl = "")
        {
            //string message = "";
            using (Datebase1Entities dbobj = new Datebase1Entities())
            {
                var v = dbobj.Users.Where(a => a.EmailID == obj.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (v.IsEmailVerified == true)
                    {
                        if (string.Compare(obj.Password, v.Password) == 0)
                        {
                            int timeout = obj.RememberMe ? 525600 : 20; // 525600 min = 1 year
                            var ticket = new FormsAuthenticationTicket(obj.EmailID, obj.RememberMe, timeout);
                            string encrypted = FormsAuthentication.Encrypt(ticket);
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                            cookie.Expires = DateTime.Now.AddMinutes(timeout);
                            cookie.HttpOnly = true;
                            Response.Cookies.Add(cookie);


                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                //05-03-2021 Updated third parameter , new { userid = v.UID }
                                return RedirectToAction("DashBoard", "SellYourNotes");
                            }
                        }
                        else
                        {
                            //message = "Invalid Password";
                            ModelState.AddModelError("Password", "Invalid Password");
                            return View(obj);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Email is not verified");
                        return View(obj);
                    }
                }
                else
                {
                    //message = "Invalid Email";
                    ModelState.AddModelError("Email", "Invalid Email");
                    return View(obj);
                }
            }
            //ViewBag.Message = message;
            //return View();
        }

        [Authorize]
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
                var isExist = IsEmailExist(model.EmailID);
                if (isExist == false)
                {
                    ModelState.AddModelError("EmailID", "Email Address Does not exist");
                    return View(model);
                }
                Users user = context.Users.Where(x => x.EmailID == model.EmailID).FirstOrDefault();
                string pass = Membership.GeneratePassword(6, 2);  //Rendom Password IS Generated
                user.Password = pass;
                context.SaveChanges();
                SendPassword(user.EmailID, pass);
                TempData["Success"] = "New Password Has Been Sent To Your EmailID";
            }
            return RedirectToAction("ForgotPassword");
        }

        [NonAction]
        public void SendPassword(string emailID, string pass)
        {
            var fromEmail = new MailAddress("rathodkrushnaraj8055@gmail.com");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "rathod8055"; // Replace with actual password
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
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }


        [Authorize]
        [Route("ChangePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Route("ChangePassword")]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var emailid = User.Identity.Name.ToString();
            Users obj = context.Users.Where(x => x.EmailID == emailid).FirstOrDefault();

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
                    ModelState.AddModelError("OldPassword", "OldPassword Is Incorrect");
                }
            }
            return View();
        }



    }
}