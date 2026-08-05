# Spec: Tool khắc phục lỗi "Remove PIN" bị vô hiệu hoá trên Windows

## 1. Bối cảnh & mục tiêu

Windows đôi khi vô hiệu hoá (làm xám) nút **Remove** trong `Settings → Accounts → Sign-in options → PIN (Windows Hello)`, khiến người dùng không thể xoá PIN đăng nhập dù máy dùng cá nhân, không cần bảo mật cao. Nguyên nhân phổ biến: Group Policy / Registry policy bắt buộc Windows Hello (thường thấy trên máy nối domain/Azure AD, nhưng đôi khi cũng dính trên máy cá nhân do cấu hình sai).

Mục tiêu: xây dựng tool Windows Desktop **chẩn đoán nguyên nhân** và **xử lý theo đúng con đường chính thống** — không bypass, không đụng vào SAM database, không vượt qua BitLocker.

**Nguyên tắc cốt lõi — KHÔNG được làm:**
- Không sửa trực tiếp SAM (Security Account Manager) database
- Không tự động bypass hoặc vượt qua BitLocker dưới bất kỳ hình thức nào
- Không tự động suspend BitLocker mà không có xác nhận rõ ràng từ người dùng

## 2. Tech stack

- **Ngôn ngữ:** C#
- **UI Framework:** WinForms + **Guna.UI2** (dùng cho toàn bộ control: buttons, panels, progress bar, notification...)
- **.NET target:** Guna.UI2 yêu cầu tối thiểu .NET Framework 4.5+ (khuyến nghị 4.7.2 hoặc .NET 6/8) — **không tương thích .NET Framework 3.5**, cần lưu ý nếu project khác của bạn đang dùng 3.5
- **Quyền chạy:** yêu cầu Administrator (UAC elevate qua app manifest `requireAdministrator`)

## 3. Yêu cầu chạy nền (background / system tray)

- Tool có khả năng **thu nhỏ xuống khay hệ thống (system tray)** thay vì đóng hẳn khi bấm nút X hoặc Minimize
- Dùng `NotifyIcon` (WinForms chuẩn, tương thích tốt với Guna.UI2 cho phần UI form chính)
- Icon tray có context menu tối thiểu: **Mở**, **Quét lại (Re-scan)**, **Thoát**
- Khi có kết quả diagnostics mới hoặc cảnh báo (VD: phát hiện BitLocker đang bật), hiển thị balloon tip / notification từ tray icon
- Double-click vào tray icon → khôi phục cửa sổ chính (`Show()` + `WindowState = Normal` + `BringToFront()`)
- Tuỳ chọn (checkbox trong Settings): **"Khởi động cùng Windows"** — ghi vào `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, khởi động ở trạng thái thu nhỏ trong tray

## 4. Chống mở nhiều lần (Single-instance, kiểu Revo Uninstaller)

- Dùng **Named Mutex** (GUID cố định, riêng của app) kiểm tra ngay khi `Main()` khởi động, trước khi tạo bất kỳ Form nào
- Luồng xử lý:
  1. Khởi động process
  2. Kiểm tra Mutex đã tồn tại chưa (`Mutex.TryOpenExisting` hoặc `new Mutex(true, name, out bool createdNew)`)
  3. **Nếu đã tồn tại** (app đang chạy, kể cả đang ở tray):
     - Tìm cửa sổ instance cũ bằng `FindWindow` (theo tên class/title cố định)
     - Gọi `SetForegroundWindow` để đưa cửa sổ cũ lên trước (nếu đang ở tray thì restore trước)
     - Thoát tiến trình mới ngay lập tức, không hiển thị UI
  4. **Nếu chưa tồn tại:**
     - Tạo Mutex mới (`CreateMutex`)
     - Tiếp tục khởi động ứng dụng bình thường
- Cần xử lý cả trường hợp user bấm mở app trong khi nó đang ở tray (không phải chỉ khi đang mở cửa sổ)

## 5. Module Diagnostics (chẩn đoán)

Chạy 3 kiểm tra (có thể song song), sau đó tổng hợp kết quả:

### 5.1. Registry / Group Policy
- Đọc `HKLM\SOFTWARE\Policies\Microsoft\Windows\System` — key `AllowDomainPINLogon` và các policy liên quan Windows Hello for Business
- Đọc `HKLM\SOFTWARE\Microsoft\PolicyManager\default\Authentication` (đối với máy Windows Home không có gpedit)
- Nếu có `gpedit.msc` (Pro/Enterprise): có thể đọc thêm qua `gpresult /r` (chạy process, parse output) để xác nhận policy nào đang effective

### 5.2. Trạng thái BitLocker
- Query WMI namespace `root\CIMV2\Security\MicrosoftVolumeEncryption`, class `Win32_EncryptableVolume`
- Dùng `System.Management.ManagementObjectSearcher` để đọc `ProtectionStatus`
- Dùng method `GetSuspendCount` để biết còn bao nhiêu lần reboot nữa BitLocker tự resume (nếu đang suspend)
- Chỉ dùng để **hiển thị thông tin và cảnh báo**, không dùng để quyết định bypass gì cả

### 5.3. Loại tài khoản
- Kiểm tra tài khoản hiện tại là Local hay Microsoft Account (`System.DirectoryServices.AccountManagement`)
- Kiểm tra máy có domain-joined / Azure AD joined không (`System.DirectoryServices.ActiveDirectory`, hoặc `dsregcmd /status`)

### 5.4. Tổng hợp & hiển thị
- Gộp kết quả 3 kiểm tra thành **1 nguyên nhân cụ thể**, hiển thị bằng ngôn ngữ dễ hiểu, ví dụ:
  - "PIN bị khoá do chính sách tổ chức (Group Policy)"
  - "PIN bị khoá do giá trị Registry sai lệch"
  - "Không xác định được nguyên nhân rõ ràng — có thể do lỗi Ngc folder"
- Đề xuất hành động tương ứng, không tự động áp dụng — chờ người dùng bấm xác nhận

## 6. Module Action / Fix — theo thứ tự ưu tiên

### Ưu tiên 1: Sửa Registry / Policy
- Trước khi sửa: **backup giá trị hiện tại** ra file log/JSON (để rollback được)
- Sửa `AllowDomainPINLogon` hoặc policy tương ứng về giá trị cho phép remove PIN
- Kiểm tra lại: nút Remove PIN đã dùng được chưa
- **Nếu thành công** → ghi log, kết thúc
- **Nếu thất bại** → chuyển sang Ưu tiên 2

### Ưu tiên 2: Xoá thư mục Ngc (fallback)
- Đường dẫn: `C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Ngc`
- **Trước khi xoá, bắt buộc kiểm tra BitLocker:**
  - **Nếu BitLocker đang bật** → **KHÔNG tự động xoá**. Hiển thị cảnh báo rõ ràng, hướng dẫn người dùng 2 lựa chọn:
    1. Nhập recovery key hợp lệ của chính họ (nếu tool cần thao tác sâu hơn)
    2. Tự chạy lệnh chính thống để tạm ngưng bảo vệ: `manage-bde -protectors -disable C: -RebootCount 1` (tạm ngưng đúng 1 lần khởi động lại, tự động resume sau đó — an toàn hơn `-RebootCount 0` vì không cần nhớ bật lại tay)
    - Cung cấp nút "Resume BitLocker ngay" gọi `manage-bde -protectors -enable C:` phòng trường hợp cần bật lại thủ công
  - **Nếu BitLocker không bật** → tiếp tục xử lý bình thường
- Backup toàn bộ thư mục Ngc (copy ra `%TEMP%` hoặc dạng zip) trước khi xoá
- Xử lý quyền sở hữu (ownership) — folder thuộc `LocalService`, không phải user thường:
  - Dùng `takeown.exe` + `icacls` qua `Process.Start`, hoặc `System.Security.AccessControl`
  - Bắt `UnauthorizedAccessException` → gợi ý chạy lại trong Safe Mode nếu vẫn thất bại
- Xoá xong → khởi động lại máy để PIN biến mất hoàn toàn
- Ghi log toàn bộ thao tác

## 7. Yêu cầu UI (Guna.UI2)

- Bước 1 — **Scan/Diagnostic**: hiển thị kết quả bằng ngôn ngữ dễ hiểu, dùng `Guna2Panel` / `Guna2Badge` để phân loại mức độ (thông tin / cảnh báo)
- Bước 2 — **Đề xuất & Apply**: mỗi hành động có nút Apply riêng (không gộp "Fix All" mù quáng), dùng `Guna2Button`
- **Log chi tiết**: hiển thị trong `Guna2TextBox` (multiline, readonly) hoặc `Guna2DataGridView`, cho phép người dùng xem lại đã đổi gì
- **Nút Undo/Rollback** riêng cho phần Registry đã sửa
- Progress indicator dùng `Guna2ProgressBar` khi đang chạy diagnostics hoặc xử lý
- Toàn bộ cảnh báo liên quan BitLocker cần style nổi bật (màu warning/amber), không lẫn với thông báo thường

## 8. Testing checklist

- [ ] Local Account lẫn Microsoft Account
- [ ] Windows Home (không có gpedit) lẫn Windows Pro
- [ ] Có BitLocker bật và không bật — đảm bảo tool **dừng đúng lúc**, không cố xử lý khi phát hiện BitLocker
- [ ] Mở tool 2 lần liên tiếp — xác nhận instance thứ 2 chỉ focus cửa sổ cũ rồi thoát, không tạo tiến trình thừa
- [ ] Thu nhỏ xuống tray, mở lại từ tray, thoát hẳn từ tray — không để tiến trình treo ngầm sau khi chọn Thoát
- [ ] Rollback Registry sau khi đã Apply — xác nhận khôi phục đúng giá trị gốc

---

*Tài liệu này tổng hợp từ phiên trao đổi thiết kế tool, dùng làm brief để tiếp tục phát triển ở phiên chat khác.*
