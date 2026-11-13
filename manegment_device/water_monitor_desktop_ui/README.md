# Water Monitor Desktop · WinForms

Ứng dụng desktop Windows viết bằng C# WinForms để quản lý thiết bị giám sát chất lượng nước qua MQTT trong mạng nội bộ. Không cần website công khai hoặc máy chủ web.

## Chức năng giao diện

- Tổng quan với các chỉ số và biểu đồ minh họa.
- Lịch sử đo với bộ chọn thiết bị mẫu.
- Quản lý thiết bị: thêm thiết bị, kiểm tra dữ liệu bắt buộc và mã trùng.
- Hiệu chỉnh: nhập độ lệch pH, hệ số TDS và độ lệch nhiệt độ; hệ số TDS phải lớn hơn 0.
- Cài đặt có vị trí dành cho cấu hình broker.

Các giá trị hiện là dữ liệu mẫu. Chưa có backend, cơ sở dữ liệu hoặc kết nối MQTT; nút gửi chỉ kiểm tra dữ liệu rồi báo rõ rằng lệnh chưa được gửi. Tích hợp giao thức sẽ thực hiện khi backend được bổ sung.

## Mở và chạy

Cài .NET 8 SDK trên Windows, sau đó chạy ở thư mục gốc:

```powershell
dotnet run --project .\WinFormsApp\WaterMonitor.WinForms.csproj
```

Trong VS Code, cài **C# Dev Kit** (VS Code sẽ gợi ý extension này), mở thư mục dự án, chọn cấu hình **Water Monitor WinForms** ở tab Run and Debug rồi nhấn nút xanh hoặc `F5`. VS Code sẽ build project trước khi mở cửa sổ ứng dụng.

Để tạo file thực thi Windows:

```powershell
dotnet publish .\WinFormsApp\WaterMonitor.WinForms.csproj -c Release -r win-x64 --self-contained false
```

`WinFormsApp/` là ứng dụng hiện hành. Các thư mục và file Flutter ở gốc được giữ lại làm bản prototype cũ, không tham gia build WinForms.

Xem chú thích lớp, màn hình và các điểm cần thay bằng backend trong [`WinFormsApp/CODE_NOTES.txt`](WinFormsApp/CODE_NOTES.txt).
