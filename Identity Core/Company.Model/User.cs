using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Model
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; }

        public string LastName { get; set; }
    }
}
