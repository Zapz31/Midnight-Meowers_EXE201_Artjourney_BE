using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Courses
{
    public class CourseLearningStatisticsDTO
    {
        public TimeSpan TotalLearningTime { get; set; } = TimeSpan.Zero;
        public int TotalLearningContent { get; set; } = 0;
    }
}
