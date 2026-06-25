using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class EmployeeService
    {
        public List<Employee> GetAll()
        {
            using (var db = new AppDbContext())
            {
                return db.Employees.OrderBy(x => x.FullName).ToList();
            }
        }

        public Employee Login(string username, string password)
        {
            using (var db = new AppDbContext())
            {
                return db.Employees.FirstOrDefault(x => x.Username == username && x.PasswordHash == password && x.IsActive);
            }
        }

        public void Save(Employee employee)
        {
            using (var db = new AppDbContext())
            {
                if (employee.Id == 0)
                {
                    db.Employees.Add(employee);
                }
                else
                {
                    var existing = db.Employees.Find(employee.Id);
                    if (existing == null) return;
                    existing.FullName = employee.FullName;
                    existing.PhoneNumber = employee.PhoneNumber;
                    existing.Email = employee.Email;
                    existing.IdentityNumber = employee.IdentityNumber;
                    existing.Position = employee.Position;
                    existing.Salary = employee.Salary;
                    existing.Username = employee.Username;
                    existing.PasswordHash = employee.PasswordHash;
                    existing.Role = employee.Role;
                    existing.IsActive = employee.IsActive;
                }
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var employee = db.Employees.Find(id);
                if (employee == null) return;
                db.Employees.Remove(employee);
                db.SaveChanges();
            }
        }
    }
}

