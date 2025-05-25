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
    [Table("learning_contents")]
    public class LearningContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        [Column("learning_content_id")]
        public long LearningContentId { get; set; }

        [Column("content_type")]
        public LearningContentType? ContentType { get; set; }

        [Column("challenge_type")]
        public ChallengeType? ChallengeType { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("correct_answer")]
        public string? CorrectAnswer { get; set;}

        [Column("time_limit")]
        public TimeSpan? TimeLimit { get; set; }

        [Column("complete_criteria", TypeName = "decimal(4,1)")]
        public decimal? CompleteCriteria { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("likes_count")]
        public int LikesCount {  get; set; } = 0;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get;set; }

        // Navigation properties
        // N - 1
        [Column("created_by")]
        [ForeignKey("LearningContentCreator")]
        public long CreatedBy { get; set; }
        public virtual User LearningContentCreator { get; set; } = null!;

        [Column("sub_module_id")]
        [ForeignKey("ContentSubModule")]
        public long SubModuleId { get; set; }
        public virtual SubModule ContentSubModule { get; set; } = null!;

        // 1 - N
        [InverseProperty("LearningContent")]
        public virtual ICollection<ChallengeItem> ChallengeItems { get; set; } = new List<ChallengeItem>();

        [InverseProperty("LearningContent")]
        public virtual ICollection<UserLearningProgress> UserLearningProgresses { get; set; } = new List<UserLearningProgress>();

    }
}

/*
 table learning_contents {
  learning_content_id long [primary key, increment] ==
    sub_module_id long [ref: > sub_modules.sub_module_id]
    content_type varchar // "read", "challenge", "quiz" - enum ==
    challenge_type varchar // --"puzzle", "matching", "timeline", "interactive", v.v. - enum ==
    title VARCHAR(255) ==
    content TEXT ==
    correct_answer text ==
    time_limit long //-- Thời gian giới hạn (giây) cho challenge ==
    complete_criteria decimal (1.1) //-- Điểm tối thiểu để 1 bài quiz được đánh giá là hoàn thành ==
    display_order int ==
    likes_count INT [default: 0] ==
    created_at timestamp ==
    updated_at timestamp ==
    created_by long [ref:> users.id] ==
}

 */
