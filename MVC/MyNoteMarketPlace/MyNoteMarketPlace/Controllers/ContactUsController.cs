using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{

    public class ContactUsController : Controller
    {
        Datebase1Entities context = new Datebase1Entities();
        // GET: ContactUs
        [Route("ContactUs")]
        public ActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [Route("ContactUs")]
        public ActionResult ContactUs(ContactUsModel model)
        {
            if (ModelState.IsValid)
            {
                ContactUs user = new ContactUs()
                {
                    FullName = model.FullName,
                    EmailID = model.EmailID,
                    Subjects = model.Subjects,
                    Comments = model.Comments,
                    IsActive=true
                };

                context.ContactUs.Add(user);
                context.SaveChanges();
                SendEmailToAdmin(user);
            }

            ModelState.Clear();
            return RedirectToAction("ContactUs");
        }

        [NonAction]
        public void SendEmailToAdmin(ContactUs obj)
        {
            var fromEmail = new MailAddress(obj.EmailID); //Email of user
            var toEmail = new MailAddress("rathodkrushnaraj8055@gmail.com"); //Email of admin
            var fromEmailPassword = "rathod8055"; // Replace with actual password
            string subject = obj.FullName + " - Query";

            string body = "Hello," +
                "<br/><br/>" + obj.Comments +
                "<br/><br/>Regards, " + obj.FullName;

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
    }
}