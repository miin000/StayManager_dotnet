using System;

namespace staymanager_pj.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string IdentityNumber { get; set; }
        // CCCD / CMND / Passport

        public string Address { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public Customer()
        {
            FullName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            IdentityNumber = string.Empty;
            Address = string.Empty;
            Username = string.Empty;
            PasswordHash = string.Empty;
            CreatedAt = DateTime.Now;
        }
    }
}

