using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNoteMarketPlace.Models;

namespace MyNoteMarketPlace.DbOperations
{
    public class Create
    {
        public int AddUsers(UsersModel model) {
            using (var context=new Datebase1Entities()) {

                Users user = new Users()
                {
                    
                  FirstName =model.FirstName,
                    LastName=model.LastName,
                    EmailID=model.EmailID,
                    Password=model.Password,
                    SecretCode=model.SecretCode

                };
                context.Users.Add(user);

                context.SaveChanges();

                return user.ID;

            }
        }
    }
}