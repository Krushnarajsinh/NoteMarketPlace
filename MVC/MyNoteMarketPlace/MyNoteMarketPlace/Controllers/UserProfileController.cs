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
    public class UserProfileController : Controller
    {
        readonly private Datebase1Entities _dbcontext = new Datebase1Entities();
        // GET: UserProfile
        [HttpGet]
        [Authorize]
       [Route("Profile")]
        public ActionResult UserProfile()
        {
            // get logged in user 
            var user = _dbcontext.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get logged in user's profile if exists
            var userprofile = _dbcontext.UserProfile.Where(x => x.User_ID == user.ID).FirstOrDefault();

            // create object of userprofileviewmodel
            UserProfileModel userprofilemodel = new UserProfileModel();

            // if userprofile is not null then store it's value in userprofileviewmodel object for edit profile
            if (userprofile != null)
            {
                userprofilemodel.CountryList = _dbcontext.Countries.Where(x => x.IsActive == true).ToList();
                userprofilemodel.GenderList = _dbcontext.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                userprofilemodel.UserID = user.ID;
                userprofilemodel.Email = user.EmailID;
                userprofilemodel.FirstName = user.FirstName;
                userprofilemodel.LastName = user.LastName;
                userprofilemodel.DOB = userprofile.DOB;
                userprofilemodel.PhoneNumberCountryCode = userprofile.Phone_number_Country_Code;
                userprofilemodel.PhoneNumber = userprofile.Phone_number;
                userprofilemodel.Gender = userprofile.Gender;
                userprofilemodel.AddressLine1 = userprofile.Address_Line_1;
                userprofilemodel.AddressLine2 = userprofile.Address_Line_2;
                userprofilemodel.City = userprofile.City;
                userprofilemodel.State = userprofile.State;
                userprofilemodel.ZipCode = userprofile.Zip_Code;
                userprofilemodel.Country = userprofile.Country;
                userprofilemodel.University = userprofile.University;
                userprofilemodel.College = userprofile.College;
                userprofilemodel.ProfilePictureUrl = userprofile.Profile_Picture;
            }
            //if userprofile is null then create it
            else
            {
                userprofilemodel.CountryList = _dbcontext.Countries.Where(x => x.IsActive == true).ToList();
                userprofilemodel.GenderList = _dbcontext.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                userprofilemodel.UserID = user.ID;
                userprofilemodel.Email = user.EmailID;
                userprofilemodel.FirstName = user.FirstName;
                userprofilemodel.LastName = user.LastName;
            }

            // return userprofileviewmodel
            return View(userprofilemodel);
        }

        [HttpPost]
        [Authorize]
       [Route("Profile")]
        public ActionResult UserProfile(UserProfileModel userprofileviewmodel)
        {
            // get logged in user
            var user = _dbcontext.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // check if ModelState is valid or not
            if (ModelState.IsValid)
            {
                // check if profile picture is not null
                if (userprofileviewmodel.ProfilePicture != null)
                {
                    // get image size and check that it is not more than 10 MB
                    var profilepicsize = userprofileviewmodel.ProfilePicture.ContentLength;
                    if (profilepicsize > 10 * 1024 * 1024)
                    {
                        // if image size is more than 10 MB show error
                        ModelState.AddModelError("ProfilePicture", "Image size limit is 10 MB");
                        userprofileviewmodel.CountryList = _dbcontext.Countries.Where(x => x.IsActive == true).ToList();
                        userprofileviewmodel.GenderList = _dbcontext.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                        return View(userprofileviewmodel);
                    }
                }



                // get logged in user's profile if exists
                var profile = _dbcontext.UserProfile.Where(x => x.User_ID == user.ID).FirstOrDefault();

                // if profile is not null then edit userprofile
                if (profile != null)
                {
                    profile.DOB = userprofileviewmodel.DOB;
                    profile.Gender = userprofileviewmodel.Gender;
                    profile.Phone_number_Country_Code = userprofileviewmodel.PhoneNumberCountryCode;
                    profile.Phone_number = userprofileviewmodel.PhoneNumber;
                    profile.Address_Line_1 = userprofileviewmodel.AddressLine1;
                    if (!String.IsNullOrEmpty(userprofileviewmodel.AddressLine2))
                    {
                        profile.Address_Line_2 = userprofileviewmodel.AddressLine2;
                    }
                    profile.City = userprofileviewmodel.City;
                    profile.State = userprofileviewmodel.State;
                    profile.Zip_Code = userprofileviewmodel.ZipCode;
                    profile.Country = userprofileviewmodel.Country;
                    profile.University = userprofileviewmodel.University;
                    profile.College = userprofileviewmodel.College;
                    profile.ModifiedDate = DateTime.Now;
                    profile.ModifiedBy = user.ID;

                    // check if loggedin user's profile picture is not null and user upload new profile picture then delete old one
                    if (userprofileviewmodel.ProfilePicture != null && profile.Profile_Picture != null)
                    {
                        string path = Server.MapPath(profile.Profile_Picture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // if user upload profile picture then save it and store path in database
                    if (userprofileviewmodel.ProfilePicture != null)
                    {
                        string filename = System.IO.Path.GetFileName(userprofileviewmodel.ProfilePicture.FileName);
                        string fileextension = System.IO.Path.GetExtension(userprofileviewmodel.ProfilePicture.FileName);
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        string profilepicturepath = "~/Members/" + profile.User_ID + "/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        profile.Profile_Picture = profilepicturepath + newfilename;
                        userprofileviewmodel.ProfilePicture.SaveAs(path);
                    }

                    // update logged in user's profile and save
                    _dbcontext.Entry(profile).State = EntityState.Modified;
                    _dbcontext.SaveChanges();

                    // update first name and lastname and save
                    user.FirstName = userprofileviewmodel.FirstName;
                    user.LastName = userprofileviewmodel.LastName;
                    _dbcontext.Entry(user).State = EntityState.Modified;
                    _dbcontext.SaveChanges();

                }
                // if profile is null then create user profile
                else
                {
                    // create new userprofile object
                    UserProfile userprofile = new UserProfile();

                    userprofile.User_ID = user.ID;
                    userprofile.DOB = userprofileviewmodel.DOB;
                    userprofile.Gender = userprofileviewmodel.Gender;
                    userprofile.Phone_number_Country_Code = userprofileviewmodel.PhoneNumberCountryCode;
                    userprofile.Phone_number = userprofileviewmodel.PhoneNumber;
                    userprofile.Address_Line_1 = userprofileviewmodel.AddressLine1;
                    userprofile.Address_Line_2 = userprofileviewmodel.AddressLine2;
                    userprofile.City = userprofileviewmodel.City;
                    userprofile.State = userprofileviewmodel.State;
                    userprofile.Zip_Code = userprofileviewmodel.ZipCode;
                    userprofile.Country = userprofileviewmodel.Country;
                    userprofile.University = userprofileviewmodel.University;
                    userprofile.College = userprofileviewmodel.College;
                    userprofile.CreatedDate = DateTime.Now;
                    userprofile.CreatedBy = user.ID;

                    // if profile picture is not null then save it and store path in database with filename is timestamp
                    if (userprofileviewmodel.ProfilePicture != null)
                    {
                        string filename = System.IO.Path.GetFileName(userprofileviewmodel.ProfilePicture.FileName);
                        string fileextension = System.IO.Path.GetExtension(userprofileviewmodel.ProfilePicture.FileName);
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        string profilepicturepath = "~/Members/" + userprofile.User_ID + "/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        userprofile.Profile_Picture = profilepicturepath + newfilename;
                        userprofileviewmodel.ProfilePicture.SaveAs(path);
                    }

                    // save logged in user's profile
                    _dbcontext.UserProfile.Add(userprofile);
                    _dbcontext.SaveChanges();

                    // save firstname and lastname and save
                    user.FirstName = userprofileviewmodel.FirstName;
                    user.LastName = userprofileviewmodel.LastName;
                    _dbcontext.Entry(user).State = EntityState.Modified;
                    _dbcontext.SaveChanges();
                }

                // if userprofile is created or edited then redirect to search page
                return RedirectToAction("Search", "SearchNotes");
            }
            // if ModelState is invalid then redirect to userProfile page
            else
            {
                userprofileviewmodel.CountryList = _dbcontext.Countries.Where(x => x.IsActive == true).ToList();
                userprofileviewmodel.GenderList = _dbcontext.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                return View(userprofileviewmodel);
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

        // get profile image for navigation bar
        [Authorize]
        public ActionResult GetPhoto()
        {
            // get user
            var user = _dbcontext.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
            // get user profile
            var profile = _dbcontext.UserProfile.Where(x => x.User_ID == user.ID).FirstOrDefault();
            //take byte variable
            byte[] photo;
            // if profile and profile picture is not null then show user's profile picture
            if (profile != null)
            {
                if (profile.Profile_Picture != null)
                {
                    string Path = Server.MapPath(profile.Profile_Picture);
                    photo = System.IO.File.ReadAllBytes(Path);
                }
                else
                {
                    string Path = Server.MapPath("~/DefaultImage/profile.png");
                    photo = System.IO.File.ReadAllBytes(Path);
                }
            }
            // else show default profile picture
            else
            {
                string Path = Server.MapPath("~/DefaultImage/profile.png");
                photo = System.IO.File.ReadAllBytes(Path);
            }
            // return image
            return File(photo, "image/jpeg");
        }

    }
}