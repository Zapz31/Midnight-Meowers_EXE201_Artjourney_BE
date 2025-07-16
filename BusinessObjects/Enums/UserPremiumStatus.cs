using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum UserPremiumStatus
    {
        // Chưa đăng ký premium
        FreeTier,

        // Đã kích hoạt premium
        PremiumActive,

        //Premium đã hết hạn, mất quyền tham gia thử thách
        PremiumExpired,

        // Premium bị tạm ngưng
        PremiumSuspended
    }
}
