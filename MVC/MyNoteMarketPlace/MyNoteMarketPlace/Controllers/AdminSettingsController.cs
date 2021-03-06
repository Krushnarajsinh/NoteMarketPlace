using MyNoteMarketPlace.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNoteMarketPlace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminSettingsController : Controller
    {
        Datebase1Entities context = new Datebase1Entities();
        // GET: AdminSettings
        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator/Add/{id}")]
        public ActionResult AddAdministrator()
        {
            // create object of AddAdministratorViewModel
            AddAdministratorModel addAdministratorModel = new AddAdministratorModel
            {
                CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList(),
                PhoneNumberCountryCode = "+91"
            };

            return View(addAdministratorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator/Add/{id}")]
        public ActionResult AddAdministrator(AddAdministratorModel obj)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists or not
                bool emailalreadyexists = context.Users.Where(x => x.EmailID == obj.Email).Any();
                if (emailalreadyexists)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                    return View(obj);
                }

                // get logged in superadmin
                var superadmin =context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // create object of user
                // default password for admin is Admin@123
                // admin can change password after login
                Users user = new Users
                {
                    FirstName = obj.FirstName.Trim(),
                    LastName = obj.LastName.Trim(),
                    RoleID = 2,
                    EmailID = obj.Email.Trim(),
                    Password = "Admin@123",
                    IsEmailVerified = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = superadmin.ID,
                    IsActive = true
                };

                // save user in database
                context.Users.Add(user);
                context.SaveChanges();

                // get saved admin id
                var addedadmin = context.Users.Find(user.ID);

                // crate admin object
                Admin admin = new Admin
                {
                    AdminID = addedadmin.ID,
                    PhoneNumberCountryCode= obj.PhoneNumberCountryCode.Trim(),
                    Phone_number = obj.PhoneNumber.Trim(),
                   
                    CreatedDate = DateTime.Now,
                    CreatedBy = superadmin.ID
                };

                // save userprofile object in database
                context.Admin.Add(admin);
                context.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator/Edit/{id}")]
        public ActionResult EditAdministrator(int id)
        {
            // get logged in user and userprofile
            var user = context.Users.Where(x => x.ID == id).FirstOrDefault();
            var admin = context.Admin.Where(x => x.AdminID == id).FirstOrDefault();

            // check if user or userprofile is null or not
            if (user == null || admin == null)
            {
                return HttpNotFound();
            }

            // create object of AddAdministratorViewModel
            AddAdministratorModel addAdministratorViewModel = new AddAdministratorModel
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.EmailID,
                PhoneNumberCountryCode = admin.PhoneNumberCountryCode,
                PhoneNumber = admin.Phone_number,
                CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList()
            };

            return View(addAdministratorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator/Edit/{id}")]
        public ActionResult EditAdministrator(AddAdministratorModel obj)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists or not
                bool emailalreadyexists = context.Users.Where(x => x.EmailID == obj.Email && x.ID != obj.ID).Any();
                if (emailalreadyexists)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                    return View(obj);
                }

                // get user, userprofile
                var user = context.Users.Where(x => x.ID == obj.ID).FirstOrDefault();
                var admin = context.Admin.Where(x => x.AdminID== obj.ID).FirstOrDefault();

                // get logged in superadmin
                var superadmin = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // check if user or userprofile is null or not
                if (user == null || admin == null)
                {
                    return HttpNotFound();
                }

                // update user data
                user.FirstName = obj.FirstName.Trim();
                user.LastName = obj.LastName.Trim();
                user.EmailID = obj.Email.Trim();
                user.ModifiedDate = DateTime.Now;
                user.ModifiedBy = superadmin.ID;

                // save user in database
                context.Entry(user).State = EntityState.Modified;
                context.SaveChanges();

                // update userprofile data
                admin.PhoneNumberCountryCode = obj.PhoneNumberCountryCode;
                admin.Phone_number = obj.PhoneNumber;
                admin.ModifiedDate = DateTime.Now;
                admin.ModifiedBy = superadmin.ID;

                // save userprofile in database
                context.Entry(admin).State = EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("ManageAdministrator");
            }
            else
            {
                obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }
        }

        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator")]
        public ActionResult ManageAdministrator(string search, string sort, int? page )
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get role id for admin
            int adminid = context.UserRoles.Where(x => x.Name.ToLower() == "admin").Select(x => x.ID).FirstOrDefault();

            // get list of admin
            IEnumerable<ManageAdministratorModel> admins = from user in context.Users
                                                               join profile in context.Admin on user.ID equals profile.AdminID
                                                               where user.RoleID == adminid
                                                               select new ManageAdministratorModel
                                                               {
                                                                   ID = user.ID,
                                                                   FirstName = user.FirstName,
                                                                   LastName = user.LastName,
                                                                   Email = user.EmailID,
                                                                   PhoneNumber = profile.Phone_number,
                                                                   DateAdded = user.CreatedDate.Value,
                                                                   Active = user.IsActive == true ? "YES" : "NO"
                                                               };

            // if search is not empty then get search result from admins
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                admins = admins.Where(x => x.FirstName.ToLower().Contains(search) ||
                                           x.LastName.ToLower().Contains(search) ||
                                           x.Email.ToLower().Contains(search) ||
                                           x.PhoneNumber.Contains(search) ||
                                           x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                           x.Active.ToLower().Contains(search)
                                     ).ToList();
            }

            // sort results
            admins = SortTableAdministrator(sort, admins);

            var result = new List<ManageAdministratorModel>();
            result = admins.ToList();

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 5));
           
        }

        // sorting for administrator
        private IEnumerable<ManageAdministratorModel> SortTableAdministrator(string sort, IEnumerable<ManageAdministratorModel> table)
        {
            switch (sort)
            {
                case "FirstName_Asc":
                    {
                        table = table.OrderBy(x => x.FirstName);
                        break;
                    }
                case "FirstName_Desc":
                    {
                        table = table.OrderByDescending(x => x.FirstName);
                        break;
                    }
                case "LastName_Asc":
                    {
                        table = table.OrderBy(x => x.LastName);
                        break;
                    }
                case "LastName_Desc":
                    {
                        table = table.OrderByDescending(x => x.LastName);
                        break;
                    }
                case "Email_Asc":
                    {
                        table = table.OrderBy(x => x.Email);
                        break;
                    }
                case "Email_Desc":
                    {
                        table = table.OrderByDescending(x => x.Email);
                        break;
                    }
                case "Phone_Asc":
                    {
                        table = table.OrderBy(x => x.PhoneNumber);
                        break;
                    }
                case "Phone_Desc":
                    {
                        table = table.OrderByDescending(x => x.PhoneNumber);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table.ToList();
        }

        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageAdministrator/Delete/{id}")]
        public ActionResult DeleteAdministrator(int id)
        {
            // get logged in superadmin
            var superadmin = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get admin user
            var admin = context.Users.Where(x => x.ID == id).FirstOrDefault();

            // check if admin is null or not
            if (admin == null)
            {
                return HttpNotFound();
            }

            // update admin data
            admin.ModifiedDate = DateTime.Now;
            admin.ModifiedBy = superadmin.ID;
            admin.IsActive = false;

            // save admin object in database
            context.Entry(admin).State = EntityState.Modified;
            context.SaveChanges();

            return RedirectToAction("ManageAdministrator");
        }

        /* Country */

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCountry/Add")]
        public ActionResult AddCountry(AddCountryModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // create country object
                Countries country = new Countries
                {
                    Name = obj.CountryName.Trim(),
                    CountryCode = obj.CountryCode.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true
                };

                // save country in database
                context.Countries.Add(country);
                context.SaveChanges();

                return RedirectToAction("ManageCountry");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCountry/Edit/{id}")]
        public ActionResult EditCountry(int id)
        {
            // get country by id
            var country = context.Countries.Where(x => x.ID == id).FirstOrDefault();

            // check if country is null or not
            if (country == null)
            {
                return HttpNotFound();
            }

            // create object of AddCountryViewModel
            AddCountryModel editCountry = new AddCountryModel
            {
                ID = country.ID,
                CountryName = country.Name,
                CountryCode = country.CountryCode
            };

            return View(editCountry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCountry/Edit/{id}")]
        public ActionResult EditCountry(AddCountryModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // get country
                var country = context.Countries.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if country is null or not
                if (country == null)
                {
                    return HttpNotFound();
                }

                // update country record
                country.Name = obj.CountryName.Trim();
                country.CountryCode = obj.CountryCode.Trim();
                country.ModifiedDate = DateTime.Now;
                country.ModifiedBy = user.ID;

                // save country in database
                context.Entry(country).State = EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("ManageCountry");
            }
            else
            {
                return View(obj);
            }
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCountry")]
        public ActionResult ManageCountry(string search, string sort, int? page )
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get country list
            IEnumerable<ManageCountryModel> countrylist = from country in context.Countries
                                                              join user in context.Users on country.CreatedBy equals user.ID
                                                              select new ManageCountryModel
                                                              {
                                                                  ID = country.ID,
                                                                  CountryName = country.Name,
                                                                  CountryCode = country.CountryCode,
                                                                  DateAdded = country.CreatedDate.Value,
                                                                  AddedBy = user.FirstName + " " + user.LastName,
                                                                  Active = country.IsActive == true ? "YES" : "NO"
                                                              };

            // if search is not empty then get search result from countrylist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                countrylist = countrylist.Where(x => x.CountryName.ToLower().Contains(search) ||
                                                     x.CountryCode.ToLower().Contains(search) ||
                                                     x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                                     x.AddedBy.ToLower().Contains(search) ||
                                                     x.Active.ToLower().Contains(search)
                                                ).ToList();
            }

            // sort results
            countrylist = SortTableCountry(sort, countrylist);

            var result = new List<ManageCountryModel>();
            result = countrylist.ToList();

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 5));
        }

        // sorting for country
        private IEnumerable<ManageCountryModel> SortTableCountry(string sort, IEnumerable<ManageCountryModel> table)
        {
            switch (sort)
            {
                case "CountryName_Asc":
                    {
                        table = table.OrderBy(x => x.CountryName);
                        break;
                    }
                case "CountryName_Desc":
                    {
                        table = table.OrderByDescending(x => x.CountryName);
                        break;
                    }
                case "CountryCode_Asc":
                    {
                        table = table.OrderBy(x => x.CountryCode);
                        break;
                    }
                case "CountryCode_Desc":
                    {
                        table = table.OrderByDescending(x => x.CountryCode);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCountry/Delete/{id}")]
        public ActionResult DeleteCountry(int id)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get country
            var country = context.Countries.Where(x => x.ID == id).FirstOrDefault();

            // check if country is null or not
            if (country == null)
            {
                return HttpNotFound();
            }

            // update country data
            country.ModifiedDate = DateTime.Now;
            country.ModifiedBy = user.ID;
            country.IsActive = false;

            // save country in database
            context.Entry(country).State = EntityState.Modified;
            context.SaveChanges();

            return RedirectToAction("ManageCountry");
        }


        /*Add Type*/
        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType/Add")]
        public ActionResult AddType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType/Add")]
        public ActionResult AddType(AddTypeModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // create object of NoteType
                NoteTypes noteType = new NoteTypes
                {
                    Name = obj.Name.Trim(),
                    Description = obj.Description.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true

                };

                // save notetype object in database
                context.NoteTypes.Add(noteType);
                context.SaveChanges();

                return RedirectToAction("ManageType");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType/Edit/{id}")]
        public ActionResult EditType(int id)
        {
            // get type object
            var type = context.NoteTypes.Where(x => x.ID == id).FirstOrDefault();

            // check if type is null or not
            if (type == null)
            {
                return HttpNotFound();
            }

            // create object of AddTypeViewModel
            AddTypeModel editType = new AddTypeModel
            {
                ID = type.ID,
                Name = type.Name,
                Description = type.Description
            };

            return View(editType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType/Edit/{id}")]
        public ActionResult EditType(AddTypeModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // get typpe object
                var type = context.NoteTypes.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if type is null or not
                if (type == null)
                {
                    return HttpNotFound();
                }

                // update type data 
                type.Name = obj.Name.Trim();
                type.Description = obj.Description.Trim();
                type.ModifiedDate = DateTime.Now;
                type.ModifiedBy = user.ID;

                // save type in database
                context.Entry(type).State = EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("ManageType");
            }
            else
            {
                return View(obj);
            }
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType")]
        public ActionResult ManageType(string search, string sort, int? page)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get type list
            IEnumerable<ManageTypeModel> typelist = from type in context.NoteTypes
                                                        join user in context.Users on type.CreatedBy equals user.ID
                                                        select new ManageTypeModel
                                                        {
                                                            ID = type.ID,
                                                            Type = type.Name,
                                                            Description = type.Description,
                                                            DateAdded = type.CreatedDate.Value,
                                                            AddedBy = user.FirstName + " " + user.LastName,
                                                            Active = type.IsActive == true ? "YES" : "NO"
                                                        };

            // if search is not empty then get search result from typelist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                typelist = typelist.Where(x => x.Type.ToLower().Contains(search) ||
                                               x.Description.ToLower().Contains(search) ||
                                               x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                               x.AddedBy.ToLower().Contains(search) ||
                                               x.Active.ToLower().Contains(search)
                                          ).ToList();
            }

            // sort results
            typelist = SortTableType(sort, typelist);

            var result = new List<ManageTypeModel>();
            result = typelist.ToList();

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 5));
        }

        // sorting for type
        private IEnumerable<ManageTypeModel> SortTableType(string sort, IEnumerable<ManageTypeModel> table)
        {
            switch (sort)
            {
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.Type);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.Type);
                        break;
                    }
                case "Description_Asc":
                    {
                        table = table.OrderBy(x => x.Description);
                        break;
                    }
                case "Description_Desc":
                    {
                        table = table.OrderByDescending(x => x.Description);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageType/Delete/{id}")]
        public ActionResult DeleteType(int id)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get type
            var type = context.NoteTypes.Where(x => x.ID == id).FirstOrDefault();

            // check if type is null or not
            if (type == null)
            {
                return HttpNotFound();
            }

            // update type data
            type.ModifiedDate = DateTime.Now;
            type.ModifiedBy = user.ID;
            type.IsActive = false;

            // save type in database
            context.Entry(type).State = EntityState.Modified;
            context.SaveChanges();

            return RedirectToAction("ManageType");
        }

        /*Add Category*/
        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory/Add")]
        public ActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory/Add")]
        public ActionResult AddCategory(AddCategoryModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // create object of NoteCategory
                NoteCategories noteCategory = new NoteCategories
                {
                    Name = obj.Name.Trim(),
                    Description = obj.Description.Trim(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.ID,
                    IsActive = true
                };

                // add category in database and save
                context.NoteCategories.Add(noteCategory);
                context.SaveChanges();

                return RedirectToAction("ManageCategory");
            }
            else
            {
                return View(obj);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory/Edit/{id}")]
        public ActionResult EditCategory(int id)
        {
            // get category
            var category = context.NoteCategories.Where(x => x.ID == id).FirstOrDefault();

            // check if category is null or not
            if (category == null)
            {
                return HttpNotFound();
            }

            // create object of addcategoryviewmodel 
            AddCategoryModel editCategory = new AddCategoryModel
            {
                ID = category.ID,
                Name = category.Name,
                Description = category.Description
            };

            return View(editCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory/Edit/{id}")]
        public ActionResult EditCategory(AddCategoryModel obj)
        {
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // get category object
                var category = context.NoteCategories.Where(x => x.ID == obj.ID).FirstOrDefault();

                // check if category is null or not
                if (category == null)
                {
                    return HttpNotFound();
                }

                // update category data
                category.Name = obj.Name.Trim();
                category.Description = obj.Description.Trim();
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = user.ID;

                // save category in database
                context.Entry(category).State = EntityState.Modified;
                context.SaveChanges();

                return RedirectToAction("ManageCategory");
            }
            else
            {
                return View(obj);
            }
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory")]
        public ActionResult ManageCategory(string search, string sort, int? page)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get category list
            IEnumerable<ManageCategoryModel> categorylist = from category in context.NoteCategories
                                                                join user in context.Users on category.CreatedBy equals user.ID
                                                                select new ManageCategoryModel
                                                                {
                                                                    ID = category.ID,
                                                                    Category = category.Name,
                                                                    Description = category.Description,
                                                                    DateAdded = category.CreatedDate.Value,
                                                                    AddedBy = user.FirstName + " " + user.LastName,
                                                                    Active = category.IsActive == true ? "YES" : "NO"
                                                                };

            // if search is not empty then get search result from categorylist
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                categorylist = categorylist.Where(x => x.Category.ToLower().Contains(search) ||
                                                       x.Description.ToLower().Contains(search) ||
                                                       x.DateAdded.ToString("dd-MM-yyyy hh:mm").Contains(search) ||
                                                       x.AddedBy.ToLower().Contains(search) ||
                                                       x.Active.ToLower().Contains(search)
                                                 ).ToList();
            }

            // sort result
            categorylist = SortTableCategory(sort, categorylist);


            var result = new List<ManageCategoryModel>();
            result = categorylist.ToList();

            return View(result.ToList().AsQueryable().ToPagedList(page ?? 1, 5));
        }

        // sorting for category
        private IEnumerable<ManageCategoryModel> SortTableCategory(string sort, IEnumerable<ManageCategoryModel> table)
        {
            switch (sort)
            {
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.Category);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.Category);
                        break;
                    }
                case "Description_Asc":
                    {
                        table = table.OrderBy(x => x.Description);
                        break;
                    }
                case "Description_Desc":
                    {
                        table = table.OrderByDescending(x => x.Description);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                case "AddedBy_Asc":
                    {
                        table = table.OrderBy(x => x.AddedBy);
                        break;
                    }
                case "AddedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedBy);
                        break;
                    }
                case "Active_Asc":
                    {
                        table = table.OrderBy(x => x.Active);
                        break;
                    }
                case "Active_Desc":
                    {
                        table = table.OrderByDescending(x => x.Active);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "Super Admin,Admin")]
        [Route("Settings/ManageCategory/Delete/{id}")]
        public ActionResult DeleteCategory(int id)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get category
            var category = context.NoteCategories.Where(x => x.ID == id).FirstOrDefault();

            // check if category is null or not
            if (category == null)
            {
                return HttpNotFound();
            }

            // update category data
            category.ModifiedDate = DateTime.Now;
            category.ModifiedBy = user.ID;
            category.IsActive = false;

            // save category in database
            context.Entry(category).State = EntityState.Modified;
            context.SaveChanges();

            return RedirectToAction("ManageCategory");
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageSystemConfiguration")]
        public ActionResult ManageSystemConfiguration()
        {
            // create object of SystemConfigurationViewModel
            SystemConfigurationModel systemConfiguration = new SystemConfigurationModel();

            // get supportemail
            var supportemail = context.SystemConfigurations.Where(x => x.Key.ToLower() == "supportemail").FirstOrDefault();
            if (supportemail != null)
            {
                systemConfiguration.SupportEmail = supportemail.Value;
            }

            // get supportcontact
            var supportcontact = context.SystemConfigurations.Where(x => x.Key.ToLower() == "supportcontact").FirstOrDefault();
            if (supportcontact != null)
            {
                systemConfiguration.SupportContact = supportcontact.Value;
            }

            // get notifyemail
            var notifyemail = context.SystemConfigurations.Where(x => x.Key.ToLower() == "notifyemail").FirstOrDefault();
            if (notifyemail != null)
            {
                systemConfiguration.NotifyEmail = notifyemail.Value;
            }

            // get facebookurl
            var facebookurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "facebookurl").FirstOrDefault();
            if (facebookurl != null)
            {
                systemConfiguration.FacebookURL = facebookurl.Value;
            }

            // get twitterurl
            var twitterurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "twitterurl").FirstOrDefault();
            if (twitterurl != null)
            {
                systemConfiguration.TwitterURL = twitterurl.Value;
            }

            // get linkedinurl
            var linkedinurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "linkedinurl").FirstOrDefault();
            if (linkedinurl != null)
            {
                systemConfiguration.LinkedInURL = linkedinurl.Value;
            }

            // get defaultprofile
            var defaultprofile = context.SystemConfigurations.Where(x => x.Key.ToLower() == "defaultprofile").FirstOrDefault();
            if (defaultprofile != null)
            {
                systemConfiguration.DefaultProfileURL = defaultprofile.Value;
            }

            // get defaultnote
            var defaultnote = context.SystemConfigurations.Where(x => x.Key.ToLower() == "defaultnote").FirstOrDefault();
            if (defaultnote != null)
            {
                systemConfiguration.DefaultNoteURL = defaultnote.Value;
            }

            return View(systemConfiguration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin")]
        [Route("Settings/ManageSystemConfiguration")]
        public ActionResult ManageSystemConfiguration(SystemConfigurationModel obj)
        {
            // check if default note picture is available or not
            if (obj.DefaultNote == null && obj.DefaultNoteURL == null)
            {
                ModelState.AddModelError("DefaultNote", "Default note picture is required");
                return View(obj);
            }

            // check if default profile picture is available or not
            if (obj.DefaultProfile == null && obj.DefaultProfileURL == null)
            {
                ModelState.AddModelError("DefaultProfile", "Default profile picture is required");
                return View(obj);
            }

            if (ModelState.IsValid)
            {
                // get logged in superadmin
                var superadmin = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // create object of SystemConfiguration
                SystemConfigurations systemConfiguration = new SystemConfigurations();

                // get supportemail
                var supportemail = context.SystemConfigurations.Where(x => x.Key.ToLower() == "supportemail").FirstOrDefault();
                // if supportemail is null then create
                if (supportemail == null)
                {
                    systemConfiguration.Key = "supportemail";
                    systemConfiguration.Value = obj.SupportEmail.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    context.SystemConfigurations.Add(systemConfiguration);
                    context.SaveChanges();
                }
                // edit supportemail
                else
                {
                    if (!supportemail.Value.Equals(obj.SupportEmail))
                    {
                        supportemail.Value = obj.SupportEmail.Trim();
                        supportemail.ModifiedDate = DateTime.Now;
                        supportemail.ModifiedBy = superadmin.ID;

                        context.Entry(supportemail).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get supportcontact
                var supportcontact = context.SystemConfigurations.Where(x => x.Key.ToLower() == "supportcontact").FirstOrDefault();
                // if supportcontact is null then create
                if (supportcontact == null)
                {
                    systemConfiguration.Key = "supportcontact";
                    systemConfiguration.Value = obj.SupportContact.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    context.SystemConfigurations.Add(systemConfiguration);
                    context.SaveChanges();
                }
                // edit supportcontact
                else
                {
                    if (!supportcontact.Value.Equals(obj.SupportContact))
                    {
                        supportcontact.Value = obj.SupportContact.Trim();
                        supportcontact.ModifiedDate = DateTime.Now;
                        supportcontact.ModifiedBy = superadmin.ID;

                        context.Entry(supportcontact).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get notifyemail
                var notifyemail = context.SystemConfigurations.Where(x => x.Key.ToLower() == "notifyemail").FirstOrDefault();
                // if notifyemail is null then create
                if (notifyemail == null)
                {
                    systemConfiguration.Key= "notifyemail";
                    systemConfiguration.Value = obj.NotifyEmail.Trim();
                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    context.SystemConfigurations.Add(systemConfiguration);
                    context.SaveChanges();
                }
                // edit notifyemail
                else
                {
                    if (!notifyemail.Value.Equals(obj.NotifyEmail))
                    {
                        notifyemail.Value = obj.NotifyEmail.Trim();
                        notifyemail.ModifiedDate = DateTime.Now;
                        notifyemail.ModifiedBy = superadmin.ID;

                        context.Entry(notifyemail).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get facebookurl
                var facebookurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "facebookurl").FirstOrDefault();
                // if facebookurl is null then create
                if (facebookurl == null)
                {
                    if (obj.FacebookURL != null)
                    {
                        systemConfiguration.Key = "facebookurl";
                        systemConfiguration.Value = obj.FacebookURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        context.SystemConfigurations.Add(systemConfiguration);
                        context.SaveChanges();
                    }
                }
                // edit facebookurl
                else
                {
                    if (!facebookurl.Value.Equals(obj.FacebookURL))
                    {
                        facebookurl.Value = obj.FacebookURL.Trim();
                        facebookurl.ModifiedDate = DateTime.Now;
                        facebookurl.ModifiedBy = superadmin.ID;

                        context.Entry(facebookurl).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get twitterurl
                var twitterurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "twitterurl").FirstOrDefault();
                // if twitterurl is null then create
                if (twitterurl == null)
                {
                    if (obj.TwitterURL != null)
                    {
                        systemConfiguration.Key= "twitterurl";
                        systemConfiguration.Value = obj.TwitterURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        context.SystemConfigurations.Add(systemConfiguration);
                        context.SaveChanges();
                    }
                }
                // edit twitterurl
                else
                {
                    if (!twitterurl.Value.Equals(obj.TwitterURL))
                    {
                        twitterurl.Value = obj.TwitterURL.Trim();
                        twitterurl.ModifiedDate = DateTime.Now;
                        twitterurl.ModifiedBy = superadmin.ID;

                        context.Entry(twitterurl).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get linkedinurl
                var linkedinurl = context.SystemConfigurations.Where(x => x.Key.ToLower() == "linkedinurl").FirstOrDefault();
                // if linkedinurl is null then create
                if (linkedinurl == null)
                {
                    if (obj.LinkedInURL != null)
                    {
                        systemConfiguration.Key = "linkedinurl";
                        systemConfiguration.Value = obj.LinkedInURL.Trim();
                        systemConfiguration.CreatedDate = DateTime.Now;
                        systemConfiguration.CreatedBy = superadmin.ID;
                        systemConfiguration.IsActive = true;

                        context.SystemConfigurations.Add(systemConfiguration);
                        context.SaveChanges();
                    }
                }
                // edit linkedinurl
                else
                {
                    if (!linkedinurl.Value.Equals(obj.LinkedInURL))
                    {
                        linkedinurl.Value = obj.LinkedInURL.Trim();
                        linkedinurl.ModifiedDate = DateTime.Now;
                        linkedinurl.ModifiedBy = superadmin.ID;

                        context.Entry(linkedinurl).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get defaultprofile
                var defaultprofile = context.SystemConfigurations.Where(x => x.Key.ToLower() == "defaultprofile").FirstOrDefault();
                // if defaultprofile is null then create
                if (defaultprofile == null)
                {
                    systemConfiguration.Key= "defaultprofile";

                    if (obj.DefaultProfile != null)
                    {
                        string fileextension = System.IO.Path.GetExtension(obj.DefaultProfile.FileName);
                        string newfilename = "profile" + fileextension;
                        string profilepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.DefaultProfile.SaveAs(path);

                        systemConfiguration.Value = profilepicturepath + newfilename;
                    }

                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    context.SystemConfigurations.Add(systemConfiguration);
                    context.SaveChanges();
                }
                // edit defaultprofile
                else
                {
                    // check if user uploaded defaultprofile
                    if (obj.DefaultProfile != null)
                    {
                        // check if there is already profile picture is available or not
                        // if available then we have to delete
                        if (defaultprofile.Value != null)
                        {
                            string previouspath = Server.MapPath(defaultprofile.Value);
                            FileInfo file = new FileInfo(previouspath);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }

                        string fileextension = System.IO.Path.GetExtension(obj.DefaultProfile.FileName);
                        string newfilename = "profile" + fileextension;
                        string profilepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        obj.DefaultProfile.SaveAs(path);

                        defaultprofile.Value = profilepicturepath + newfilename;

                        defaultprofile.ModifiedDate = DateTime.Now;
                        defaultprofile.ModifiedBy = superadmin.ID;

                        context.Entry(defaultprofile).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                // get defaultnote
                var defaultnote = context.SystemConfigurations.Where(x => x.Key.ToLower() == "defaultnote").FirstOrDefault();
                // if defaultnote is null then create
                if (defaultnote == null)
                {
                    systemConfiguration.Key= "defaultnote";

                    if (obj.DefaultNote != null)
                    {
                        string fileextension = System.IO.Path.GetExtension(obj.DefaultNote.FileName);
                        string newfilename = "note" + fileextension;
                        string notepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(notepicturepath);
                        string path = Path.Combine(Server.MapPath(notepicturepath), newfilename);
                        obj.DefaultNote.SaveAs(path);

                        systemConfiguration.Value = notepicturepath + newfilename;
                    }

                    systemConfiguration.CreatedDate = DateTime.Now;
                    systemConfiguration.CreatedBy = superadmin.ID;
                    systemConfiguration.IsActive = true;

                    context.SystemConfigurations.Add(systemConfiguration);
                    context.SaveChanges();
                }
                // edit defaultnote
                else
                {
                    // check if user uploaded defaultnote
                    if (obj.DefaultNote != null)
                    {
                        // check if there is already note picture is available or not
                        // if available then we have to delete
                        if (defaultnote.Value != null)
                        {
                            string previouspath = Server.MapPath(defaultnote.Value);
                            FileInfo file = new FileInfo(previouspath);
                            if (file.Exists)
                            {
                                file.Delete();
                            }
                        }

                        string fileextension = System.IO.Path.GetExtension(obj.DefaultNote.FileName);
                        string newfilename = "note" + fileextension;
                        string notepicturepath = "~/DefaultImage/";
                        CreateDirectoryIfMissing(notepicturepath);
                        string path = Path.Combine(Server.MapPath(notepicturepath), newfilename);
                        obj.DefaultNote.SaveAs(path);

                        defaultnote.Value = notepicturepath + newfilename;

                        defaultnote.ModifiedDate = DateTime.Now;
                        defaultnote.ModifiedBy = superadmin.ID;

                        context.Entry(defaultprofile).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

                return RedirectToAction("ManageSystemConfiguration");
            }
            else
            {
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