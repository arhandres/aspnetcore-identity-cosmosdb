using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.DataAccess;
using Company.Model;
using Company.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebSite.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public IActionResult CreateTestUser()
        {
            var userRepository = HttpContext.RequestServices.GetService(typeof(IUserRepository)) as IUserRepository;

            var userName = "arhandres@hotmail.com";
            var password = "Password11;";

            var user = new User()
            {
                Id = 1,
                UserName = userName,
                PasswordHash = ApplicationPasswordHasher.CreateHashPassword(password)
            };

            var success = userRepository.CreateUser(user);

            return Json(new
            {
                Created = success
            });
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}