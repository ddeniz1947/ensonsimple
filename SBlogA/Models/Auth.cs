﻿using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SBlogA.Models
{
    public class Auth
    {
        private const string UserKey = "SBlogA.Auth.UserKey";
        public static User User
        {
            get
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                    return null;

                var user = HttpContext.Current.Items[UserKey] as User;
                if(user == null)
                {
                    user = Database.Session.Query<User>().FirstOrDefault(u => u.Username == HttpContext.Current.User.Identity.Name);

                    if (user == null)
                        return null;
                    HttpContext.Current.Items[UserKey] = user;
                }
                return user;
            }
        }
    }
}