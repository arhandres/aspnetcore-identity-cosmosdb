using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Model
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        [JsonProperty(PropertyName = "Partition")]
        public string Tenan { get; set; }

        public User()
        {
            this.Tenan = "b76e3dff-0bec-439e-9aa2-16bd2066132a";
        }
    }
}
