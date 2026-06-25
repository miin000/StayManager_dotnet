
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Models
{
    public enum UserRole
    {
        Admin = 1,
        Staff = 2,
        Customer = 3
    }

    public enum RoomStatus
    {
        Available = 1,      // Phòng trống
        Occupied = 2,       // Đang sử dụng
        Reserved = 3,       // Đã đặt
        Cleaning = 4,       // Đang dọn
        Maintenance = 5     // Đang bảo trì
    }

    public enum ReservationStatus
    {
        Pending = 1,        // Chờ xác nhận
        Confirmed = 2,      // Đã xác nhận
        CheckedIn = 3,      // Đã nhận phòng
        CheckedOut = 4,     // Đã trả phòng
        Cancelled = 5       // Đã hủy
    }

    public enum InvoiceStatus
    {
        Unpaid = 1,         // Chưa thanh toán
        Paid = 2,           // Đã thanh toán
        Cancelled = 3       // Đã hủy
    }

    public enum PaymentMethod
    {
        None = 0,
        Cash = 1,
        BankTransfer = 2
    }

    public enum InventoryStatus
    {
        Available = 1,      // Có sẵn
        LowStock = 2,       // Sắp hết
        OutOfStock = 3      // Hết hàng
    }
}

