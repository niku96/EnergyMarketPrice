using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EMP_API.Authentification
{
    public class ChangeUserStatus
    {
        public string Username { get; set; }
        public bool IsDeleted { get; set; }
    }
}
