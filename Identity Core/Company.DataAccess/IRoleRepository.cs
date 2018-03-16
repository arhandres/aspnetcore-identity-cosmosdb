using Company.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.DataAccess
{
    public interface IRoleRepository : IRoleStore<Role>
    {
    }
}
