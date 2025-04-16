using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Enums
{
    public enum AccountStatus
    {
        /// <summary>
        /// Tài khoản đang hoạt động bình thường, người dùng có thể truy cập tất cả các tính năng
        /// </summary>
        Active,

        /// <summary>
        /// Tài khoản mới được tạo nhưng chưa xác thực email/số điện thoại
        /// </summary>
        Pending,

        /// <summary>
        /// Tài khoản đã bị vô hiệu hóa tạm thời do vi phạm nhẹ hoặc do quản trị viên xem xét
        /// Có thể kích hoạt lại sau một thời gian nhất định
        /// </summary>
        Suspended,

        /// <summary>
        /// Tài khoản đã bị cấm vĩnh viễn do vi phạm nghiêm trọng điều khoản sử dụng
        /// Không thể kích hoạt lại
        /// </summary>
        Banned,

        /// <summary>
        /// Tài khoản bị khóa tạm thời do nhập sai mật khẩu nhiều lần liên tiếp
        /// Tự động mở khóa sau một khoảng thời gian hoặc sau khi xác thực lại danh tính
        /// </summary>
        Locked,

        /// <summary>
        /// Tài khoản chưa hoàn thành hồ sơ cá nhân hoặc các bước đăng ký bắt buộc
        /// </summary>
        Incomplete,

        /// <summary>
        /// Tài khoản không còn hoạt động do người dùng tự yêu cầu tạm ngưng
        /// Có thể kích hoạt lại khi người dùng đăng nhập trở lại
        /// </summary>
        Dormant,

        /// <summary>
        /// Tài khoản đã bị xóa theo yêu cầu của người dùng nhưng dữ liệu vẫn được giữ lại
        /// trong một khoảng thời gian (thường là 30-90 ngày) trước khi xóa vĩnh viễn
        /// </summary>
        Deactivated,

        /// <summary>
        /// Tài khoản giáo viên/người bán khóa học đang chờ xét duyệt
        /// trước khi được phép đăng tải nội dung lên nền tảng
        /// </summary>
        PendingApproval,

        /// <summary>
        /// Tài khoản hết hạn vì vấn đề thanh toán (ví dụ: gói đăng ký đã hết hạn, 
        /// thẻ tín dụng hết hạn, thanh toán bị từ chối)
        /// </summary>
        PaymentIssue,

        /// <summary>
        /// Tài khoản VIP hoặc người dùng cao cấp có các đặc quyền bổ sung
        /// </summary>
        Premium,

        /// <summary>
        /// Tài khoản đang được xem xét do có báo cáo vi phạm từ người dùng khác
        /// </summary>
        UnderReview
    }
}
