using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public Guid OrderId { get; set; } = Guid.NewGuid();

        [Column("order_code")] // order code return from payos
        public long OrderCode { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("course_id")] // if user pay for premium, this field is null
        public long? CourseId { get; set; }

        [Column("user_premium_info_id")] // if user pay for a course, this field is null
        public long? UserPremiumInfoId { get; set; }
    }
}
