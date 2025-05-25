using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("challenge_items")]
    public class ChallengeItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("challenge_item_id")]
        public long UserId { get; set; }

        [Column("item_type")]
        public ChallengeItemTypes? ItemTypes { get; set; }

        [Column("item_content")]
        public string? ItemContent { get; set;}

        [Column("item_order")]
        public int? ItemOrder { get; set; }

        [Column("hint")]
        public string? Hint { get; set; }

        [Column("additional_data")]
        public string? AdditionalData { get; set;}

        //Navigation properties 
        // N - 1
        [Column("learning_content_id")]
        [ForeignKey("LearningContent")]
        public long LearningContentId { get; set; }
        public virtual LearningContent LearningContent { get; set; } = null!;
    }
}

/*
 table challenge_items {
  challenge_item_id long [primary key, increment] ==
  learning_content_id long [ref: > learning_contents.learning_content_id]
  item_type VARCHAR(50) //-- Loại item: "image", "text", "input", "draggable", v.v. - enum ==
  item_content text //-- Nội dung (có thể là URL ảnh, text, JSON data,...) ==
  item_order INT ==
  hint TEXT //-- Gợi ý (nếu có) ==
  additional_data JSON //-- Lưu trữ dữ liệu bổ sung dưới dạng JSON ==
}
 */
