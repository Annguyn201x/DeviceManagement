# 🐟 HỆ THỐNG QUẢN LÝ THIẾT BỊ GIÁM SÁT MÔI TRƯỜNG NƯỚC
## (Management Device - IoT Water Quality Monitoring)

---

## 📋 Mục Lục

1. [Tổng Quan Dự Án](#1-tổng-quan-dự-án)
2. [Công Nghệ Sử Dụng](#2-công-nghệ-sử-dụng)
3. [Kiến Trúc Hệ Thống](#3-kiến-trúc-hệ-thống)
4. [Cơ Sở Dữ Liệu MySQL](#4-cơ-sở-dữ-liệu-mysql)
5. [Cấu Trúc Thư Mục](#5-cấu-trúc-thư-mục)
6. [Chi Tiết Các Lớp (Class)](#6-chi-tiết-các-lớp-class)
7. [Luồng Hoạt Động (Flow)](#7-luồng-hoạt-động-flow)
8. [Phần Cứng ESP32 (Firmware)](#8-phần-cứng-esp32-firmware)
9. [Hướng Dẫn Cài Đặt & Chạy](#9-hướng-dẫn-cài-đặt--chạy)
10. [Quy Tắc Nghiệp Vụ](#10-quy-tắc-nghiệp-vụ)

---

## 1. Tổng Quan Dự Án

Hệ thống **Management Device** là một ứng dụng IoT hoàn chỉnh bao gồm:

- **Ứng dụng Desktop (Windows Forms .NET 9)**: Quản lý người dùng, hồ nuôi, thiết bị đo và hiển thị dữ liệu môi trường nước (pH, TDS, nhiệt độ).
- **Firmware ESP32 (PlatformIO/Arduino)**: Đọc cảm biến pH, TDS, nhiệt độ DS18B20 và gửi dữ liệu qua Serial.

### Mô hình nghiệp vụ:
- Một **Hồ nuôi** có ít nhất một **Thiết bị** (máy đo)
- Một **Hồ nuôi** có thể có nhiều **Thiết bị**
- Một **User** có thể quản lý nhiều **Hồ nuôi**
- Mỗi **Thiết bị** gửi data theo interval khác nhau

---

## 2. Công Nghệ Sử Dụng

| Thành phần | Công nghệ | Phiên bản |
|---|---|---|
| **Framework** | .NET 9 Windows Forms | `net9.0-windows` |
| **Ngôn ngữ** | C# (preview) | LangVersion: preview |
| **CSDL** | MySQL | via `MySql.Data` v26.7.0 |
| **UI Library** | Guna.UI2.WinForms | v2.0.4.8 |
| **UI Library** | ReaLTaiizor | v3.8.2.1 |
| **Firmware** | Arduino (ESP32) | PlatformIO `espressif32` |
| **Cảm biến** | OneWire + DallasTemperature | v2.3.7, v4.0.6 |
| **Bảo mật** | SHA-256 Hash Password | `System.Security.Cryptography` |

---

## 3. Kiến Trúc Hệ Thống

```
┌──────────────────────────────────────────────────────────────┐
│                    TẦNG GIAO DIỆN (UI/Forms)                │
│  Home ──► login_form ──► Menuu (Dashboard)                  │
│            Sign_form      ├── ADD_IP_device                 │
│                           ├── Manager_device                │
│                           └── Tool_manerger                 │
├──────────────────────────────────────────────────────────────┤
│                   TẦNG NGHIỆP VỤ (Business Logic)           │
│  user_backend (validate user)    UIHelpers (GDI+ drawing)   │
│  CurrentUser (session)           Hash (SHA-256)              │
├──────────────────────────────────────────────────────────────┤
│              TẦNG TRUY VẤN DỮ LIỆU (Data Access)           │
│  ┌─── Interface ────┐    ┌─── Implementation ────────────┐  │
│  │ ILogin_query      │◄──│ Login_query                   │  │
│  │ ISign_query       │◄──│ Sign_query                    │  │
│  │ IDashboard        │◄──│ Dashboard_query                │  │
│  └───────────────────┘    │   └── business_query_device   │  │
│                           └──────────────────────────────┘  │
├──────────────────────────────────────────────────────────────┤
│                 TẦNG DỮ LIỆU (MySQL Database)               │
│           Database: device                                   │
│  Tables: user | ho_nuoi | thiet_bi | thong_so               │
├──────────────────────────────────────────────────────────────┤
│                TẦNG PHẦN CỨNG (ESP32 Firmware)               │
│  Cảm biến: pH Sensor | TDS Sensor | DS18B20 (Nhiệt độ)     │
│  Giao tiếp: Serial 115200 baud                              │
└──────────────────────────────────────────────────────────────┘
```

### Sơ đồ kế thừa lớp:

```
Information_device          Sign_query
       ▲                       ▲
       │                       │
     Device               user_backend

business_query_device (abstract)
       ▲
       │
  Dashboard_query ──implements──► IDashboard

Login_query ──implements──► ILogin_query
```

---

## 4. Cơ Sở Dữ Liệu MySQL

### Kết nối:
```
Server   = localhost
Database = device
Uid      = root
Pwd      = hoangduc2006b@
```

### Bảng `user` – Quản lý tài khoản

| Cột | Kiểu dữ liệu | Mô tả |
|---|---|---|
| `user_name` | VARCHAR | Tên đăng nhập (Primary Key) |
| `password` | VARCHAR | Mật khẩu đã băm SHA-256 |
| `Role` | VARCHAR | Vai trò: `"root"` hoặc `"member"` |

### Bảng `ho_nuoi` – Quản lý hồ nuôi

| Cột | Kiểu dữ liệu | Mô tả |
|---|---|---|
| `id` | INT (AUTO_INCREMENT) | Mã hồ (Primary Key) |
| `ten_ho` | VARCHAR | Tên hồ nuôi |

### Bảng `thiet_bi` – Quản lý thiết bị

| Cột | Kiểu dữ liệu | Mô tả |
|---|---|---|
| `ma_may` | VARCHAR | Mã máy (Primary Key) |
| `ho_nuoi_id` | INT (FK → ho_nuoi.id) | ID hồ nuôi được gán |
| `current_ip` | VARCHAR | Địa chỉ IP hiện tại |
| `trang_thai` | BOOLEAN | Trạng thái: `true` = hoạt động, `false` = ngắt |

### Bảng `thong_so` – Dữ liệu đo lường

| Cột | Kiểu dữ liệu | Mô tả |
|---|---|---|
| `id` | INT (AUTO_INCREMENT) | ID bản ghi (Primary Key) |
| `ma_may` | VARCHAR (FK → thiet_bi.ma_may) | Mã máy đo |
| `nong_do_ph` | DECIMAL | Nồng độ pH |
| `tds` | INT | Tổng chất rắn hòa tan (ppm) |
| `nhiet_do` | DECIMAL | Nhiệt độ (°C) |
| `thoi_gian_do` | TIMESTAMP | Thời gian đo |

### Script tạo Database:

```sql
CREATE DATABASE IF NOT EXISTS device;
USE device;

CREATE TABLE user (
    user_name VARCHAR(100) PRIMARY KEY,
    password  VARCHAR(255) NOT NULL,
    Role      VARCHAR(20)  NOT NULL DEFAULT 'member'
);

CREATE TABLE ho_nuoi (
    id     INT AUTO_INCREMENT PRIMARY KEY,
    ten_ho VARCHAR(200) NOT NULL
);

CREATE TABLE thiet_bi (
    ma_may     VARCHAR(100) PRIMARY KEY,
    ho_nuoi_id INT NOT NULL,
    current_ip VARCHAR(45)  DEFAULT '0.0.0.0',
    trang_thai BOOLEAN      DEFAULT TRUE,
    FOREIGN KEY (ho_nuoi_id) REFERENCES ho_nuoi(id) ON DELETE CASCADE
);

CREATE TABLE thong_so (
    id           INT AUTO_INCREMENT PRIMARY KEY,
    ma_may       VARCHAR(100) NOT NULL,
    nong_do_ph   DECIMAL(5,2),
    tds          INT,
    nhiet_do     DECIMAL(5,2),
    thoi_gian_do TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ma_may) REFERENCES thiet_bi(ma_may) ON DELETE CASCADE
);
```

---

## 5. Cấu Trúc Thư Mục

```
manegment_device/
├── manegment_device.sln              # Solution file
├── all image/                        # Hình ảnh dùng trong UI
│   ├── Screenshot *.png              # Ảnh chụp màn hình
│   ├── images.jpg / images.png       # Icon mắt (show/hide password)
│   └── Solid_yellow.jpg              # Nền vàng
├── manegment_device/                 # Project chính (.NET 9 WinForms)
│   ├── Program.cs                    # Entry point → mở Home
│   │
│   ├── ── Models ──────────────────
│   ├── CurrentUser.cs                # Session người dùng (static)
│   ├── Device.cs                     # Model thiết bị (kế thừa Information_device)
│   ├── Information_device.cs         # Model thông số đo (pH, TDS, nhiệt độ)
│   ├── lake.cs                       # Model hồ nuôi
│   ├── Search_History.cs             # Placeholder - lịch sử tìm kiếm
│   │
│   ├── ── Interfaces ──────────────
│   ├── IDashboard.cs                 # Interface dashboard queries
│   ├── ILogin_query.cs               # Interface login queries
│   ├── ISign_query.cs                # Interface sign-up queries
│   │
│   ├── ── Data Access ─────────────
│   ├── Login_query.cs                # CRUD bảng user + xác thực
│   ├── Sign_query.cs                 # Kiểm tra trùng & insert user
│   ├── business_query_device.cs      # CRUD bảng ho_nuoi + thiet_bi (abstract)
│   ├── Dashboard_query.cs            # SELECT thiet_bi + thong_so (kế thừa business)
│   │
│   ├── ── Business Logic ──────────
│   ├── user_backend.cs               # Validate user (kế thừa Sign_query)
│   ├── Hash.cs                       # SHA-256 hash + verify
│   ├── UIHelpers.cs                  # GDI+ bo góc, card, circle
│   │
│   ├── ── Forms (UI) ──────────────
│   ├── Home.cs / .Designer.cs        # Màn hình chào (Login / Sign Up)
│   ├── login_form.cs / .Designer.cs  # Form đăng nhập
│   ├── Sign_form.cs / .Designer.cs   # Form đăng ký (có fade-in animation)
│   ├── Menuu.cs / .Designer.cs       # Dashboard chính (device list + search)
│   ├── ADD_IP_device.cs / .Designer  # Form thêm thiết bị mới
│   ├── Manager_device.cs / .Designer # Form quản lý thiết bị
│   └── Tool_manerger.cs / .Designer  # Form công cụ quản trị
│
├── test/                             # Firmware ESP32 (PlatformIO)
│   ├── platformio.ini                # Config board esp32dev
│   ├── src/
│   │   ├── main.cpp                  # Code chính: đọc pH, TDS, nhiệt độ
│   │   └── pH-Sensor                 # Code standalone đọc cảm biến pH
│   └── include/
│       └── README                    # Hướng dẫn PlatformIO
└── flow work - main (2).pdf          # Sơ đồ luồng hoạt động
```

---

## 6. Chi Tiết Các Lớp (Class)

### 6.1 Models (Đối tượng dữ liệu)

#### `CurrentUser` – Phiên đăng nhập
```csharp
public class CurrentUser
{
    public static string Username { get; set; }  // Tên đăng nhập
    public static string Role { get; set; }      // "root" hoặc "member"
    public static void Logout();                  // Xóa session
}
```

#### `Information_device` – Thông số đo lường
```csharp
public class Information_device
{
    public int id;              // ID bản ghi
    public float Nong_do_pH;    // Nồng độ pH (0-14)
    public float Tds;           // TDS (ppm)
    public float Nhiet_do;      // Nhiệt độ (°C)
    public TimeSpan ThoiGian;   // Thời gian đo
}
```

#### `Device` – Thiết bị (kế thừa `Information_device`)
```csharp
public class Device : Information_device
{
    public string Ma_may;       // Mã máy (VD: "ESP32-001")
    public int Ho_nuoi_id;      // FK → hồ nuôi
    public string Current_IP;   // IP hiện tại (mặc định "0.0.0.0")
    public bool Trang_thai;     // true = hoạt động, false = ngắt
}
```

#### `lake` – Hồ nuôi
```csharp
public class lake
{
    public int Id;              // ID hồ
    public string Ten_ho;       // Tên hồ
}
```

### 6.2 Interfaces (Giao diện trừu tượng)

#### `ILogin_query`
```csharp
public interface ILogin_query
{
    bool checking_user(string user_name, string password);  // Xác thực
    string GetUserRole(string user_name);                   // Lấy vai trò
}
```

#### `ISign_query`
```csharp
public interface ISign_query
{
    bool checking_user_sign(string value);                           // Kiểm tra trùng
    bool Insert_user(string user_name, string password, string Role); // Tạo tài khoản
}
```

#### `IDashboard`
```csharp
public interface IDashboard
{
    List<Device> GetAllDevices();                                   // Danh sách máy
    Information_device? GetLatestReading(string maMay);             // Đọc mới nhất
    List<Information_device> GetHistory(string maMay, int take=25); // Lịch sử đo
}
```

### 6.3 Data Access Layer (Tầng truy vấn)

#### `Login_query` – Quản lý User
| Phương thức | Mô tả |
|---|---|
| `checking_user(user, pass)` | SELECT COUNT từ bảng `user` |
| `GetUserRole(user)` | Lấy Role theo username |
| `Delete_user(username)` | Xóa tài khoản |
| `GetAllUsers()` | Lấy toàn bộ user (DataTable) |
| `UpdateUserRole(user, role)` | Cập nhật quyền |

#### `Sign_query` – Đăng ký (static methods)
| Phương thức | Mô tả |
|---|---|
| `checking_user_sign(value)` | Kiểm tra username tồn tại |
| `Insert_user(user, pass, role)` | INSERT vào bảng `user` |

#### `business_query_device` (abstract) – CRUD Hồ & Thiết bị
| Phương thức | Mô tả |
|---|---|
| `insert_lake(ten_ho)` | Thêm hồ nuôi mới |
| `ADD_lake_device(tenHo, maMay, ip)` | Tạo hồ + thiết bị (Transaction) |
| `Add_Device_To_Existing_Lake(maMay, hoNuoiId, ip)` | Thêm máy vào hồ có sẵn |
| `Delete_lake(tenHo)` | Xóa hồ nuôi |
| `delete_device(maMay)` | Xóa thiết bị |
| `update_device(maMay, hoNuoiId, ip, trangThai)` | Cập nhật thiết bị |

#### `Dashboard_query` (kế thừa `business_query_device`, implement `IDashboard`)
| Phương thức | Mô tả |
|---|---|
| `GetAllDevices()` | SELECT toàn bộ bảng `thiet_bi` |
| `GetLatestReading(maMay)` | SELECT 1 bản ghi mới nhất từ `thong_so` |
| `GetHistory(maMay, take)` | SELECT N bản ghi gần nhất, đảo ngược (cũ→mới) |

### 6.4 Business Logic

#### `user_backend` (kế thừa `Sign_query`)
- **Validate `Create_User`**: Không rỗng + Kiểm tra trùng tên qua DB
- **Validate `Create_Password`**: >5 ký tự + có ít nhất 1 ký tự đặc biệt `[^\w]` + tự hash SHA-256
- **Role**: Mặc định `"member"`

#### `Hash` (static)
- **`HashPassword(string)`**: SHA-256 → hex lowercase
- **`VerifyPassword(string, string)`**: So khớp hash

#### `UIHelpers` (static)
- **`RoundedRect`**: Tạo GraphicsPath bo góc
- **`ApplyRoundedRegion`**: Clip control thành rounded rectangle
- **`ApplyCircleRegion`**: Clip control thành hình tròn
- **`PaintRoundedCard`**: Vẽ card nền với viền mịn (AntiAlias)

### 6.5 Forms (Giao diện)

| Form | Mô tả | Chức năng chính |
|---|---|---|
| **Home** | Màn hình chào mừng | 2 nút: Đăng Nhập / Đăng Ký |
| **login_form** | Đăng nhập | Nhập user/pass → Hash → Kiểm tra DB → Mở Menuu |
| **Sign_form** | Đăng ký | Validate realtime + Fade-in animation + Tạo tài khoản |
| **Menuu** | Dashboard chính | Hiển thị danh sách thiết bị dạng card, tìm kiếm, thêm thiết bị |
| **ADD_IP_device** | Thêm thiết bị | Form thêm máy/IP mới (placeholder) |
| **Manager_device** | Quản lý thiết bị | Form chỉnh sửa/xóa thiết bị (placeholder) |
| **Tool_manerger** | Công cụ admin | Form quản trị nâng cao (placeholder) |

---

## 7. Luồng Hoạt Động (Flow)

### 7.1 Luồng Đăng Nhập

```
[Khởi động App]
       │
       ▼
   ┌──────┐
   │ Home │ ──── Click "Đăng Nhập" ────►  login_form
   └──────┘                                    │
       │                                       │ Nhập username + password
       │                                       │ Hash password (SHA-256)
       │                                       │ Gọi Login_query.checking_user()
       │                                       │
       │                              ┌────────┴────────┐
       │                              │                 │
       │                        Thành công          Thất bại
       │                              │                 │
       │                     Mở Menuu (Dashboard)  Hiện thông báo lỗi
       │
       └──── Click "Đăng Ký" ────► Sign_form
                                       │
                                  Fade-in effect
                                       │
                           Validate realtime:
                           ├─ Username: rỗng? trùng?
                           ├─ Password: >5 ký tự? ký tự đặc biệt?
                           └─ Confirm password: khớp?
                                       │
                               Insert vào DB (Role="member")
                                       │
                              Quay về login_form
```

### 7.2 Luồng Dashboard (Menuu)

```
┌───────────────────────────────────────────────┐
│                  Menuu (Dashboard)             │
│                                               │
│  ┌─ Nav Bar ────────────────────────────────┐ │
│  │ [Logo] [Home] [Devices] [Admin Username] │ │
│  └──────────────────────────────────────────┘ │
│                                               │
│  ┌─ Search ─────────────────────────────────┐ │
│  │ 🔍 Tìm kiếm theo tên máy hoặc IP...    │ │
│  └──────────────────────────────────────────┘ │
│                                               │
│  ┌─ Device List (FlowLayoutPanel) ──────────┐ │
│  │ ┌─────────────────────────────────┐      │ │
│  │ │ ESP32-001                       │      │ │
│  │ │ IP: 192.168.1.100              │ Click │ │
│  │ │ ● Đang hoạt động               │──────┼─┼──► Manager_device
│  │ └─────────────────────────────────┘      │ │
│  │ ┌─────────────────────────────────┐      │ │
│  │ │ ESP32-002                       │      │ │
│  │ │ IP: 0.0.0.0                    │      │ │
│  │ │ ● Ngắt kết nối                 │      │ │
│  │ └─────────────────────────────────┘      │ │
│  └──────────────────────────────────────────┘ │
│                                               │
│            [+] Thêm thiết bị ─────────────────┼──► ADD_IP_device
└───────────────────────────────────────────────┘
```

### 7.3 Luồng Thêm Thiết Bị (Transaction)

```
[ADD_IP_device Form]
       │
       ▼
   Nhập: Tên Hồ + Mã Máy + IP
       │
       ▼
  ┌────────────────────────────────────────────┐
  │           MySQL Transaction                │
  │                                            │
  │  BEGIN TRANSACTION                         │
  │    1. INSERT INTO ho_nuoi (ten_ho)         │
  │    2. SELECT LAST_INSERT_ID() → newLakeId  │
  │    3. INSERT INTO thiet_bi                 │
  │       (ma_may, ho_nuoi_id, current_ip)     │
  │  COMMIT                                    │
  │                                            │
  │  Nếu lỗi → ROLLBACK                       │
  └────────────────────────────────────────────┘
       │
       ▼
  Quay về Dashboard → Refresh danh sách
```

---

## 8. Phần Cứng ESP32 (Firmware)

### 8.1 Cấu hình phần cứng & Môi trường nạp

| Thông số | Giá trị |
|---|---|
| **Board** | ESP32 DevKit (ESP32-WROOM-32) |
| **Framework** | Arduino |
| **Upload/Monitor** | 115200 baud, COM14 |
| **Thư viện** | OneWire v2.3.7, DallasTemperature v4.0.6, PubSubClient v2.8, Preferences |
| **Giao thức** | MQTT (TCP port 1883) qua Wi-Fi |

#### 2 Môi trường nạp (PlatformIO):
- **`esp32_prototype`** (`test/src/main.cpp`): Dành cho **máy thô** (cảm biến pH/TDS/DS18B20 rời cắm dây), đo định kỳ mỗi 4 giây qua timer `millis()`.
- **`esp32_integrated`** (`test/src/main_integrated.cpp`): Dành cho **bo mạch tích hợp hoàn chỉnh** (sau khi đặt mạch về), tích hợp IC op-amp MCP6002, ngắt RTC DS3231 hẹn giờ trên GPIO33 (`ext0`), LCD ST7789, GPS và chế độ Deep Sleep tiết kiệm pin.

#### Quy chuẩn Topic MQTT:
- `water/device/{DEVICE_ID}/telemetry` (Publish): Gửi dữ liệu cảm biến dạng JSON.
- `water/device/{DEVICE_ID}/calibrate` (Subscribe): Lắng nghe lệnh hiệu chuẩn (`phOffset`, `tdsFactor`, `tempOffset`) và lưu vào Flash NVS.
- `water/device/{DEVICE_ID}/status` (LWT): Báo trạng thái kết nối (`online` / `offline`).

### 8.2 Sơ đồ chân (Pin Mapping)

| Chân ESP32 | Cảm biến | Chức năng |
|---|---|---|
| **GPIO 34** | pH Sensor (Analog) | Đọc ADC nồng độ pH |
| **GPIO 35** | TDS Sensor (Analog) | Đọc ADC TDS |
| **GPIO 4** | DS18B20 (OneWire) | Đo nhiệt độ nước |
| **GPIO 26** | TDS Power Control | Bật/tắt nguồn module TDS |

### 8.3 Thuật toán đo

#### Đo pH (2-point calibration):
```
Calibration points:
  pH 7.0 → V_PH7 = 3.6566V
  pH 4.0 → V_PH4 = 4.3770V

Slope = (6.85 - 3.5) / (V_PH7 - V_PH4)

Quy trình:
  1. Đọc 40 mẫu ADC, delay 5ms giữa mỗi mẫu
  2. Tính trung bình ADC
  3. Chuyển ADC → Voltage (V_REF=3.3V, ADC=4095)
  4. Quy ngược voltage divider (tỉ lệ 20k/30k = 0.6667)
  5. pH = 6.85 + Slope × (vActual - V_PH7)
```

#### Đo TDS (bù nhiệt độ):
```
Quy trình:
  1. Bật nguồn TDS (GPIO 26 HIGH)
  2. Chờ 500ms ổn định
  3. Đọc 20 mẫu ADC, delay 5ms
  4. Tắt nguồn TDS (GPIO 26 LOW)
  5. Chuyển ADC → Voltage
  6. Bù nhiệt: coeff = 1 + 0.02 × (temp - 25)
  7. vComp = voltage / coeff
  8. TDS = (133.42 × vComp³ - 255.86 × vComp² + 857.39 × vComp) × 0.5
```

#### Đo nhiệt độ:
```
  1. Gọi sensors.requestTemperatures()
  2. Đọc getTempCByIndex(0)
  3. Kiểm tra != DEVICE_DISCONNECTED_C
```

### 8.4 Chu kỳ đo (Loop)

```
Mỗi 3 giây (non-blocking millis()):
  ┌─────────────┐
  │ Đọc Nhiệt độ │ (DS18B20)
  └──────┬──────┘
         ▼
  ┌─────────────┐
  │   Đọc TDS   │ (bật nguồn → đọc → tắt nguồn)
  └──────┬──────┘
         ▼
  ┌──────────────┐
  │ Chờ 1s       │ (để nước hết điện tích dư từ TDS)
  └──────┬───────┘
         ▼
  ┌─────────────┐
  │   Đọc pH    │
  └──────┬──────┘
         ▼
  ┌───────────────────────────────────────┐
  │ Serial.print:                         │
  │ "Nhiệt độ: 25.0 °C | pH: 7.02 |     │
  │  TDS: 350 ppm"                        │
  └───────────────────────────────────────┘
```

---

## 9. Hướng Dẫn Cài Đặt & Chạy

### 9.1 Yêu cầu hệ thống
- **OS**: Windows 10/11
- **.NET SDK**: 9.0 trở lên
- **MySQL Server**: 8.0+ (đang chạy trên localhost)
- **IDE**: Visual Studio 2022 / JetBrains Rider
- **PlatformIO**: VS Code + PlatformIO Extension (cho ESP32)

### 9.2 Thiết lập Database

```bash
# 1. Đăng nhập MySQL
mysql -u root -p

# 2. Chạy script tạo database (xem mục 4)
SOURCE create_database.sql;
```

### 9.3 Chạy ứng dụng Desktop

```bash
# 1. Clone repository
git clone <repository_url>

# 2. Restore packages
cd manegment_device
dotnet restore

# 3. Build và chạy
dotnet run --project manegment_device
```

### 9.4 Upload Firmware ESP32

```bash
# 1. Mở folder 'test/' bằng VS Code (có PlatformIO)
# 2. Kết nối ESP32 qua USB (COM14)
# 3. Build & Upload
pio run --target upload

# 4. Mở Serial Monitor
pio device monitor
```

---

## 10. Quy Tắc Nghiệp Vụ

### Xác thực người dùng:
- Mật khẩu được **băm SHA-256** trước khi lưu vào DB
- Khi đăng nhập: hash input → so sánh với hash trong DB
- Vai trò: `"root"` (quản trị) hoặc `"member"` (thành viên)

### Validate đăng ký:
- **Username**: Không được rỗng, không được trùng trong DB (kiểm tra realtime)
- **Password**: >5 ký tự, phải chứa ít nhất 1 ký tự đặc biệt `[^\w]`
- **Confirm Password**: Phải khớp với Password đã nhập

### Quản lý thiết bị:
- Thêm hồ + thiết bị sử dụng **MySQL Transaction** (đảm bảo ACID)
- Nếu tạo hồ thành công nhưng tạo thiết bị thất bại → **ROLLBACK** toàn bộ
- Mỗi thiết bị phải thuộc một hồ nuôi (`ho_nuoi_id` bắt buộc)

### Hiển thị Dashboard:
- Danh sách thiết bị tải từ DB và render thành **Card UI** động
- Tìm kiếm **realtime** theo tên máy hoặc IP (LINQ client-side)
- Trạng thái hiển thị: 🟢 Đang hoạt động / 🔴 Ngắt kết nối

---

> **Ghi chú**: Một số Form (`ADD_IP_device`, `Manager_device`, `Tool_manerger`) hiện chỉ có khung (placeholder), cần hoàn thiện logic nghiệp vụ bên trong.
