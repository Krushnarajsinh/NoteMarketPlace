using MyNoteMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
   
    public class AdminProfileController : Controller
    {
        readonly private Datebase1Entities context = new Datebase1Entities();
        // GET: AdminProfile
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile()
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get logged in user profile
            var userprofile = context.Admin.Where(x => x.AdminID == user.ID).FirstOrDefault();

            // create AdminProfileViewModel
            AdminProfileModel adminProfileViewModel = new AdminProfileModel();
            if (userprofile != null)
            {
                adminProfileViewModel.FirstName = user.FirstName;
                adminProfileViewModel.LastName = user.LastName;
                adminProfileViewModel.Email = user.EmailID;
                adminProfileViewModel.PhoneNumberCountryCode = userprofile.PhoneNumberCountryCode;
                adminProfileViewModel.PhoneNumber = userprofile.Phone_number;

                if (userprofile.SecondaryEmailID != null)
                {
                    adminProfileViewModel.SecondaryEmail = userprofile.SecondaryEmailID;
                }

                if (userprofile.Profile_Picture != null)
                {
                    adminProfileViewModel.ProfilePictureUrl = userprofile.Profile_Picture;
                }

                // get country code list
                adminProfileViewModel.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
            }
            else {
                adminProfileViewModel.FirstName = user.FirstName;
                adminProfileViewModel.LastName = user.LastName;
                adminProfileViewModel.Email = user.EmailID;
                adminProfileViewModel.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();

            }
            return View(adminProfileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile(AdminProfileModel obj)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
            if (ModelState.IsValid)
            {

                // get  logged in user profile
                var userprofile = context.Admin.Where(x => x.AdminID == user.ID).FirstOrDefault();

                if (userprofile != null)
                {
                    // check if secondary email is already exists in User or UserProfile table or not
                    // if email already exists then give error
                    bool secondaryemailalreadyexistsinusers = context.Users.Where(x => x.EmailID == obj.SecondaryEmail).Any();
                    bool secondaryemailalreadyexistsinuserprofile = context.Admin.Where(x => x.SecondaryEmailID == obj.Email && x.ID != user.ID).Any();
                    if (secondaryemailalreadyexistsinusers || secondaryemailalreadyexistsinuserprofile)
                    {
                        ModelState.AddModelError("SecondaryEmail", "This email address is already exists");
                        obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                        return View(obj);
                    }

                    // update user's data            
                    user.FirstName = obj.FirstName.Trim();
                    user.LastName = obj.LastName.Trim();
                    // update userprofile's data
                    userprofile.SecondaryEmailID = obj.SecondaryEmail.Trim();
                    userprofile.PhoneNumberCountryCode = obj.PhoneNumberCountryCode.Trim();
                    userprofile.Phone_number = obj.PhoneNumber.Trim();

                    // user upploaded profile picture and there is also previous profile picture then delete previous profile picture
                    if (userprofile.Profile_Picture != null && obj.ProfilePicture != null)
                    {
                        string path = Server.MapPath(userprofile.Profile_Picture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save new profile picture and update data in userprofile table
                    if (obj.ProfilePicture != null)
                    {
                        // get extension
                        string fileextension = System.IO.Path.GetExtension(obj.ProfilePicture.FileName);
                        // set new name of file
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        // set where to save picture
                        string profilepicturepath = "~/Members/" + userprofile.ID + "/";
                        // create directory if not exists
                        CreateDirectoryIfMissing(profilepicturepath);
                        // get physical path and save profile picture there
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.ProfilePicture.SaveAs(path);
                        // save path in database
                        userprofile.Profile_Picture = profilepicturepath + newfilename;
                    }

                    // update userprofile's data
                    userprofile.ModifiedDate = DateTime.Now;
                    userprofile.ModifiedBy = user.ID;

                    // update data and save data in database
                    context.Entry(user).State = EntityState.Modified;
                    context.Entry(userprofile).State = EntityState.Modified;
                    context.SaveChanges();

                }
                else {
                       Admin admin_profile = new Admin();

                        admin_profile.AdminID = user.ID;


                    // check if secondary email is already exists in User or UserProfile table or not
                    // if email already exists then give error
                    bool secondaryemailalreadyexistsinusers = context.Users.Where(x => x.EmailID == obj.SecondaryEmail).Any();
                    bool secondaryemailalreadyexistsinuserprofile = context.Admin.Where(x => x.SecondaryEmailID == obj.Email && x.AdminID != user.ID).Any();
                    if (secondaryemailalreadyexistsinusers || secondaryemailalreadyexistsinuserprofile)
                    {
                        ModelState.AddModelError("SecondaryEmail", "This email address is already exists");
                        obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                        return View(obj);
                    }

                    // update user's data            
                    user.FirstName = obj.FirstName.Trim();
                    user.LastName = obj.LastName.Trim();
                    admin_profile.FirstName = obj.FirstName.Trim();
                    admin_profile.LastName = obj.LastName.Trim();
                    admin_profile.EmailID = obj.Email.Trim();
                    // update userprofile's data
                    admin_profile.SecondaryEmailID = obj.SecondaryEmail.Trim();
                    admin_profile.PhoneNumberCountryCode = obj.PhoneNumberCountryCode.Trim();
                    admin_profile.Phone_number = obj.PhoneNumber.Trim();
                    admin_profile.CreatedDate = DateTime.Now;
                    admin_profile.CreatedBy = user.ID;

                    // user upploaded profile picture and there is also previous profile picture then delete previous profile picture
                    if (admin_profile.Profile_Picture != null && obj.ProfilePicture != null)
                    {
                        string path = Server.MapPath(admin_profile.Profile_Picture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save new profile picture and update data in userprofile table
                    if (obj.ProfilePicture != null)
                    {
                        // get extension
                        string fileextension = System.IO.Path.GetExtension(obj.ProfilePicture.FileName);
                        // set new name of file
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        // set where to save picture
                        string profilepicturepath = "~/Members/" + admin_profile.ID + "/";
                        // create directory if not exists
                        CreateDirectoryIfMissing(profilepicturepath);
                        // get physical path and save profile picture there
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.ProfilePicture.SaveAs(path);
                        // save path in database
                        admin_profile.Profile_Picture = profilepicturepath + newfilename;
                    }

                    context.Admin.Add(admin_profile);
                    context.SaveChanges();

                    // update userprofile's data
                   

                    // update data and save data in database
                    context.Entry(user).State = EntityState.Modified;
                  
                   // context.Entry(admin_profile).State = EntityState.Modified;
                    context.SaveChanges();


                }
                return RedirectToAction("Dashboard", "Admin");
            }
            else {
                obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();

                return View(obj);
            }
        }

        // Create Directory
        private void CreateDirectoryIfMissing(string folderpath)
        {
            // check if directory is exists or not
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));

            // if directory is not exists then create directory
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }
    }
}