using Company.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.DataAccess
{
    public interface IUserRepository : IUserStore<User>
    {
        bool CreateUser(User user);
        bool DeleteUser(string id);
        User GetUserById(string id);
    }
}
