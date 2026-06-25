using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class CustomerService
    {
        public List<Customer> GetAll()
        {
            using (var db = new AppDbContext())
            {
                return db.Customers.OrderBy(x => x.FullName).ToList();
            }
        }

        public Customer GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                return db.Customers.Find(id);
            }
        }

        public Customer FindByUsername(string username)
        {
            using (var db = new AppDbContext())
            {
                return db.Customers.FirstOrDefault(x => x.Username == username);
            }
        }

        public Customer Login(string username, string password)
        {
            using (var db = new AppDbContext())
            {
                return db.Customers.FirstOrDefault(x => x.Username == username && x.PasswordHash == password);
            }
        }

        public bool Register(Customer customer)
        {
            using (var db = new AppDbContext())
            {
                if (!string.IsNullOrWhiteSpace(customer.Username) && db.Customers.Any(x => x.Username == customer.Username))
                {
                    return false;
                }
                db.Customers.Add(customer);
                db.SaveChanges();
                return true;
            }
        }

        public void Save(Customer customer)
        {
            using (var db = new AppDbContext())
            {
                if (customer.Id == 0)
                {
                    db.Customers.Add(customer);
                }
                else
                {
                    var existing = db.Customers.Find(customer.Id);
                    if (existing == null) return;
                    existing.FullName = customer.FullName;
                    existing.PhoneNumber = customer.PhoneNumber;
                    existing.Email = customer.Email;
                    existing.IdentityNumber = customer.IdentityNumber;
                    existing.Address = customer.Address;
                    existing.Username = customer.Username;
                    existing.PasswordHash = customer.PasswordHash;
                }
                db.SaveChanges();
            }
        }
    }
}

