using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class AuthService
    {
        public static Customer CurrentCustomer { get; private set; }
        public static Employee CurrentEmployee { get; private set; }

        public static bool IsCustomerLoggedIn => CurrentCustomer != null;
        public static bool IsEmployeeLoggedIn => CurrentEmployee != null;
        public static bool IsLoggedIn => IsCustomerLoggedIn || IsEmployeeLoggedIn;

        public static void Login(Customer customer)
        {
            CurrentCustomer = customer;
            CurrentEmployee = null;
        }

        public static void Login(Employee employee)
        {
            CurrentEmployee = employee;
            CurrentCustomer = null;
        }

        public static void Logout()
        {
            CurrentCustomer = null;
            CurrentEmployee = null;
        }
    }
}

