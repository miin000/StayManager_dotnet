using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class InvoiceService
    {
        public List<Invoice> GetAll()
        {
            using (var db = new AppDbContext())
            {
                return db.Invoices.OrderByDescending(x => x.CreatedAt).ToList();
            }
        }

        public Invoice GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                return db.Invoices.Find(id);
            }
        }

        public List<InvoiceItem> GetItems(int invoiceId)
        {
            using (var db = new AppDbContext())
            {
                return db.InvoiceItems.Where(x => x.InvoiceId == invoiceId).ToList();
            }
        }

        public void Save(Invoice invoice)
        {
            invoice.CalculateTotal();
            using (var db = new AppDbContext())
            {
                if (invoice.Id == 0)
                {
                    if (string.IsNullOrWhiteSpace(invoice.InvoiceCode))
                    {
                        invoice.InvoiceCode = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    db.Invoices.Add(invoice);
                }
                else
                {
                    var existing = db.Invoices.Find(invoice.Id);
                    if (existing == null) return;
                    existing.RoomCharge = invoice.RoomCharge;
                    existing.ServiceCharge = invoice.ServiceCharge;
                    existing.Discount = invoice.Discount;
                    existing.TotalAmount = invoice.TotalAmount;
                    existing.Status = invoice.Status;
                    existing.PaymentMethod = invoice.PaymentMethod;
                    existing.PaidAt = invoice.PaidAt;
                    existing.ConfirmedByEmployeeId = invoice.ConfirmedByEmployeeId;
                }
                db.SaveChanges();
            }
        }

        public void AddItem(int invoiceId, InvoiceItem item)
        {
            item.InvoiceId = invoiceId;
            item.CalculateTotal();
            using (var db = new AppDbContext())
            {
                db.InvoiceItems.Add(item);
                var invoice = db.Invoices.Find(invoiceId);
                if (invoice != null)
                {
                    invoice.ServiceCharge += item.TotalPrice;
                    invoice.CalculateTotal();
                }
                db.SaveChanges();
            }
        }

        public void ConfirmPayment(int invoiceId, PaymentMethod method, int employeeId = 0)
        {
            using (var db = new AppDbContext())
            {
                var invoice = db.Invoices.Find(invoiceId);
                if (invoice == null) return;
                invoice.ConfirmPayment(method, employeeId);
                db.SaveChanges();
            }
        }
    }
}

