using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DAL.Entities
{
    public class DbUser : IdentityUser
    {
        public virtual UserProfile UserProfile { get;set; }
    }
}
