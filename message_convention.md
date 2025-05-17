|type|http status code|code|message_vie|message_eng|
|--|--|--|--|--|
|error|404|1001|Không tìm thấy người dùng có email này|No user found with this email|
|error|404|1002|Không tìm thấy email|Email not found|
|error|500|1003|Cập nhật thông tin người dùng thất bại|Failed to update user information|
|error|500|1004|Tạo hoặc cập nhật thông tin người dùng khi đăng nhập bằng gmail thất bại|
|error|500|1005|Cập nhật hoặc tạo user thành công nhưng không trả về (Login bằng gmail)|
|error|404|1006|Email này đã được sử dụng bởi một tài khoản khác|
|error|404|1007|Email hoặc password không hợp lệ|
|error|404|1008|Tài khoản của bạn đã bị ban|
|error|404|1009|Không tìm thấy tài khoản với id này|
|error|500|1010|Có lỗi xảy ra khi tạo LoginHistory||
|error|500|1011|Có lỗi xảy ra khi lấy id lớn nhất trong bảng LoginHistory||
|error|500|1012|Có lỗi xảy ra khi đếm số record trong bảng LoginHistory theo field userId||
|error|404|1013|Tài khoản này đã được xác thực trước đó||
|error|404|1014|Thời hạn xác thực email đã hết hạn hoặc thông tin xác thực không hợp lệ||
|error|500|1015|Có lỗi xảy ra trong quá trình xác thực email người dùng||
|error|500|1014|Gửi email thất bại||

|success|200|2001| Đăng nhập thành công | Login successful |
|success|200|2002| Truy xuất tài khoản thành công | Account retrieved successfully |
|success|201|2003| Tạo LoginHistory thành công | |
|success|201|2004| Xác thực email người dùng thành công | |