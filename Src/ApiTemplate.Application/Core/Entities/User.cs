using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTemplate.Application.Core.Entities
{
    public class User
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
