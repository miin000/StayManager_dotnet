using System;

namespace staymanager_pj.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string IdentityNumber { get; set; }

        public string Position { get; set; }

        public decimal Salary { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public UserRole Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public Employee()
        {
            FullName = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            IdentityNumber = string.Empty;
            Position = string.Empty;
            Username = string.Empty;
            PasswordHash = string.Empty;
            Role = UserRole.Staff;
            IsActive = true;
            CreatedAt = DateTime.Now;
        }
    }
}
