using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class BasicLearningContentGetResponseDTO
    {   
        public long LearningContentId { get; set; } 
        public LearningContentType? ContentType { get; set; }
        public ChallengeType? ChallengeType { get; set; }  
        public string? Title { get; set; } 
        public string? Content { get; set; } 
        public string? CorrectAnswer { get; set; } 
        public TimeSpan? TimeLimit { get; set; }  
        public decimal? CompleteCriteria { get; set; }   
        public int DisplayOrder { get; set; } 
        public int LikesCount { get; set; } = 0;  
        public bool IsActive { get; set; } = true;  
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;      
        public DateTime? UpdatedAt { get; set; }       
        public long CreatedBy { get; set; }             
        public long SubModuleId { get; set; }
        public long? CourseId { get; set; }
    }
}
