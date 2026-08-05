using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace frm_pin_remover.Core
{
    internal enum AppLanguage
    {
        English,
        Vietnamese
    }

    internal static class Localization
    {
        public static event Action LanguageChanged;

        public static AppLanguage Current { get; private set; } = AppLanguage.English;

        public static void SetLanguage(AppLanguage language)
        {
            if (language == Current) return;
            Current = language;
            Save();
            LanguageChanged?.Invoke();
        }

        public static string T(string key)
        {
            if (!Map.TryGetValue(key, out var pair)) return key;
            return Current == AppLanguage.Vietnamese ? pair.Vi : pair.En;
        }

        public static string TF(string key, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, T(key), args);
        }

        static Localization()
        {
            Load();
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(AppConstants.LanguageFilePath))
                {
                    string text = File.ReadAllText(AppConstants.LanguageFilePath).Trim();
                    if (string.Equals(text, "vi", StringComparison.OrdinalIgnoreCase))
                    {
                        Current = AppLanguage.Vietnamese;
                    }
                }
            }
            catch
            {
                // Fall back to the default language if the preference file can't be read.
            }
        }

        private static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(AppConstants.LanguageFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(AppConstants.LanguageFilePath, Current == AppLanguage.Vietnamese ? "vi" : "en");
            }
            catch
            {
                // Non-fatal — the language just won't be remembered next launch.
            }
        }

        private static readonly Dictionary<string, (string En, string Vi)> Map = new Dictionary<string, (string En, string Vi)>
        {
            // Sidebar / nav
            ["nav.diagnostics"] = ("Diagnostics", "Chẩn đoán"),
            ["nav.recommendations"] = ("Recommendations", "Đề xuất và Áp dụng"),
            ["nav.log"] = ("Log", "Nhật ký"),
            ["nav.settings"] = ("Settings", "Cài đặt"),
            ["nav.faq"] = ("FAQ", "Giải đáp"),
            ["nav.feedback"] = ("Feedback", "Góp ý"),
            ["nav.donate"] = ("Donate", "Ủng hộ"),
            ["sidebar.tagline"] = ("Windows Hello PIN\nRepair Utility", "Công cụ khắc phục\nlỗi khoá PIN"),

            // Tray
            ["tray.open"] = ("Open", "Mở"),
            ["tray.rescan"] = ("Rescan", "Quét lại"),
            ["tray.exit"] = ("Exit", "Thoát"),

            // MainForm balloons / messages
            ["mainform.bitlockerWarningTitle"] = ("BitLocker warning", "Cảnh báo BitLocker"),
            ["mainform.bitlockerWarningText"] = (
                "BitLocker is on for the system drive — manual confirmation is required before any deeper action.",
                "BitLocker đang bật trên ổ hệ thống — cần xác nhận thủ công trước khi xử lý sâu."),
            ["mainform.scanCompletePrefix"] = ("Scan complete: ", "Quét xong: "),
            ["mainform.stillRunningTray"] = ("The app is still running in the system tray.", "Ứng dụng vẫn chạy trong khay hệ thống."),

            // Diagnostics panel
            ["diagnostics.rescanButton"] = ("Rescan", "Quét lại"),
            ["diagnostics.rootCauseLabel"] = ("ROOT CAUSE", "NGUYÊN NHÂN TỔNG HỢP"),

            // Actions panel
            ["actions.emptyState"] = (
                "No diagnostic data yet. Run a scan from the Diagnostics tab first.",
                "Chưa có dữ liệu chẩn đoán. Hãy chạy quét ở tab Chẩn đoán trước."),
            ["actions.apply"] = ("Apply", "Áp dụng"),
            ["actions.undo"] = ("Undo", "Hoàn tác"),
            ["actions.bitlockerBlockedMessage"] = (
                "BitLocker is ON — the tool will not delete the Ngc folder automatically. Pick one of the two options below:",
                "BitLocker đang BẬT — tool sẽ không tự xoá thư mục Ngc. Chọn 1 trong 2 cách bên dưới:"),
            ["actions.copyCommand"] = ("Copy Command", "Copy lệnh"),
            ["actions.copiedTitle"] = ("Copied", "Đã copy"),
            ["actions.copiedMessage"] = (
                "Command copied. Run it yourself in an elevated Command Prompt.",
                "Đã copy lệnh. Hãy tự chạy trong cửa sổ Command Prompt (Admin) của bạn."),
            ["actions.suspendHint"] = (
                "This suspends BitLocker for exactly one reboot and then resumes automatically — safer than turning it off entirely.",
                "Lệnh trên tạm ngưng BitLocker đúng 1 lần khởi động lại rồi tự resume — an toàn hơn tắt hẳn."),
            ["actions.resumeButton"] = ("Resume BitLocker Now", "Resume BitLocker ngay"),
            ["actions.confirmTitle"] = ("Confirm", "Xác nhận"),
            ["actions.confirmApplyRegistry"] = (
                "Back up the current registry value and apply the fix that allows removing the PIN?",
                "Backup giá trị Registry hiện tại và áp dụng fix cho phép Remove PIN?"),
            ["actions.confirmRollback"] = (
                "Restore the original registry value from the latest backup?",
                "Khôi phục giá trị Registry gốc từ bản backup gần nhất?"),
            ["actions.confirmApplyNgc"] = (
                "This will back up and then delete the Ngc folder, and requires a reboot afterwards. Continue?",
                "Thao tác này sẽ backup rồi xoá thư mục Ngc và yêu cầu khởi động lại máy. Tiếp tục?"),
            ["actions.confirmResumeBitlocker"] = (
                "Turn BitLocker protection back on right now?",
                "Bật lại bảo vệ BitLocker ngay bây giờ?"),

            // Log panel
            ["log.colTime"] = ("Time", "Thời gian"),
            ["log.colAction"] = ("Action", "Hành động"),
            ["log.colDetail"] = ("Detail", "Chi tiết"),
            ["log.colResult"] = ("Result", "Kết quả"),

            // Settings panel
            ["settings.autostart"] = ("Start with Windows (minimized to tray)", "Khởi động cùng Windows (thu nhỏ vào khay)"),
            ["settings.autostartHint"] = (
                "When enabled, the app will run in the background system tray every time you sign in to Windows.",
                "Khi bật, ứng dụng sẽ tự chạy ngầm trong khay hệ thống mỗi khi đăng nhập Windows."),
            ["settings.languageLabel"] = ("Language", "Ngôn ngữ"),

            // FAQ panel
            ["faq.q1"] = ("What does this tool do?", "Công cụ này dùng để làm gì?"),
            ["faq.a1"] = (
                "PIN Remover Fixer diagnoses why the \"Remove\" button under Windows Hello PIN is greyed out, " +
                "then applies an official fix. It never touches the SAM database and never bypasses BitLocker.",
                "PIN Remover Fixer chẩn đoán lý do nút \"Remove\" trong mục PIN của Windows Hello bị mờ đi, " +
                "sau đó áp dụng cách khắc phục chính thống. Công cụ không bao giờ đụng vào SAM database hay vượt qua BitLocker."),
            ["faq.q2"] = ("How does the diagnosis work?", "Việc chẩn đoán hoạt động như thế nào?"),
            ["faq.a2"] = (
                "It checks three things in parallel: Group Policy / registry values that enforce Windows Hello, " +
                "whether BitLocker protection is on, and whether the account is Local or Microsoft plus whether the " +
                "machine is domain- or Azure AD-joined. The results are combined into one plain-language root cause.",
                "Công cụ kiểm tra song song 3 việc: Group Policy / giá trị registry bắt buộc Windows Hello, " +
                "BitLocker có đang bật hay không, và tài khoản là Local hay Microsoft cùng việc máy có join domain " +
                "hoặc Azure AD hay không. Kết quả được tổng hợp thành 1 nguyên nhân dễ hiểu."),
            ["faq.q3"] = ("What does \"Priority 1: Fix Registry/Group Policy\" do?", "Mục \"Ưu tiên 1: Sửa chính sách Registry/Group Policy\" làm gì?"),
            ["faq.a3"] = (
                "It backs up the current registry value first, then sets AllowDomainPINLogon (or the equivalent " +
                "policy) to the value that allows removing the PIN. You can undo this at any time from the same tab.",
                "Công cụ sẽ backup giá trị registry hiện tại trước, sau đó đặt AllowDomainPINLogon (hoặc policy " +
                "tương ứng) về giá trị cho phép xoá PIN. Bạn có thể hoàn tác bất cứ lúc nào ngay trong tab này."),
            ["faq.q4"] = ("What does \"Priority 2: Delete Ngc folder\" do, and why does BitLocker matter?", "Mục \"Ưu tiên 2: Xoá thư mục Ngc\" làm gì, và vì sao BitLocker lại quan trọng?"),
            ["faq.a4"] = (
                "If the registry fix doesn't resolve it, the tool can back up and delete the Ngc folder that stores " +
                "the PIN configuration, forcing Windows to rebuild it after a reboot. If BitLocker is on, the tool " +
                "refuses to touch that folder automatically — you must suspend BitLocker yourself (a ready-to-copy " +
                "command is provided) or resume protection with one click.",
                "Nếu sửa registry không giải quyết được, công cụ có thể backup rồi xoá thư mục Ngc lưu cấu hình PIN, " +
                "buộc Windows tạo lại từ đầu sau khi khởi động lại. Nếu BitLocker đang bật, công cụ sẽ từ chối tự " +
                "động đụng vào thư mục đó — bạn phải tự tạm ngưng BitLocker (đã có sẵn lệnh để copy) hoặc bật lại " +
                "bảo vệ chỉ với 1 cú nhấp."),
            ["faq.q5"] = ("Is it safe? What will it never do?", "Công cụ có an toàn không? Nó sẽ không bao giờ làm gì?"),
            ["faq.a5"] = (
                "It will never modify the SAM database, never bypass BitLocker, and never suspend BitLocker " +
                "automatically without your explicit action.",
                "Công cụ sẽ không bao giờ sửa SAM database, không bao giờ vượt qua BitLocker, và không bao giờ tự " +
                "động tạm ngưng BitLocker nếu không có hành động rõ ràng từ bạn."),
            ["faq.q6"] = ("Where can I see what changed?", "Tôi có thể xem những gì đã thay đổi ở đâu?"),
            ["faq.a6"] = (
                "The Log tab keeps a full history of every registry change, backup, and fix attempt.",
                "Tab Nhật ký lưu lại toàn bộ lịch sử mọi thay đổi registry, backup, và lần thử khắc phục."),

            // Feedback panel
            ["feedback.title"] = ("Have feedback or found a bug?", "Có góp ý hoặc phát hiện lỗi?"),
            ["feedback.description"] = (
                "Tell us what worked, what didn't, or what you'd like to see next. It only takes a minute.",
                "Cho chúng tôi biết điều gì hữu ích, điều gì chưa ổn, hoặc bạn muốn thấy gì tiếp theo. Chỉ mất một phút."),
            ["feedback.openButton"] = ("Open Feedback Form", "Mở Form Góp Ý"),

            // Donate panel
            ["donate.title"] = ("Thank you for using PIN Remover Fixer!", "Cảm ơn bạn đã sử dụng PIN Remover Fixer!"),
            ["donate.description"] = (
                "If this tool helped you, consider supporting its development with a small donation. Your support helps keep this project going!",
                "Nếu công cụ này hữu ích, hãy cân nhắc ủng hộ một chút để duy trì phát triển. Sự ủng hộ của bạn giúp dự án này tiếp tục!"),
            ["donate.yes"] = ("Yes", "Có"),
            ["donate.no"] = ("No", "Không"),
            ["donate.momoLabel"] = ("MoMo / VietQR", "MoMo / VietQR"),
            ["donate.techcombankLabel"] = ("Techcombank", "Techcombank"),
            ["donate.kofiHint"] = ("Prefer a card or PayPal? Use Ko-fi instead:", "Muốn dùng thẻ hoặc PayPal? Dùng Ko-fi:"),
            ["donate.kofiButton"] = ("Open Ko-fi Page", "Mở trang Ko-fi"),
            ["donate.linkError"] = ("Could not open the link: {0}", "Không mở được liên kết: {0}"),
            ["donate.linkErrorTitle"] = ("Error", "Lỗi"),

            // Severity badges
            ["severity.critical"] = ("CRITICAL", "NGHIÊM TRỌNG"),
            ["severity.warning"] = ("WARNING", "CẢNH BÁO"),
            ["severity.info"] = ("INFO", "THÔNG TIN"),

            // Common
            ["common.success"] = ("Success", "Thành công"),
            ["common.failed"] = ("Failed", "Thất bại"),

            // Services: RegistryPolicyDiagnostics
            ["svc.valueNotExist"] = ("{0}: does not exist", "{0}: không tồn tại"),
            ["svc.reg.readSystemPolicyError"] = ("Could not read System policy: {0}", "Không đọc được policy System: {0}"),
            ["svc.reg.policyManagerExists"] = ("{0}: exists ({1} value(s): {2})", "{0}: tồn tại ({1} giá trị: {2})"),
            ["svc.reg.readPolicyManagerError"] = ("Could not read PolicyManager Authentication: {0}", "Không đọc được PolicyManager Authentication: {0}"),
            ["svc.reg.gpresultFound"] = (
                "gpresult /r: found a GPO related to Windows Hello for Business / Passport",
                "gpresult /r: phát hiện GPO liên quan Windows Hello for Business / Passport"),
            ["svc.reg.gpresultNotFound"] = (
                "gpresult /r: no GPO related to Windows Hello for Business found",
                "gpresult /r: không thấy GPO liên quan Windows Hello for Business"),
            ["svc.reg.noGpedit"] = (
                "This machine has no gpedit.msc (Windows Home) — skipping the gpresult step.",
                "Máy không có gpedit.msc (Windows Home) — bỏ qua bước gpresult."),
            ["svc.reg.gpresultRunError"] = ("Could not run gpresult: {0}", "Không chạy được gpresult: {0}"),
            ["svc.reg.titleGpDetected"] = (
                "Group Policy related to Windows Hello was detected",
                "Phát hiện chính sách Group Policy liên quan Windows Hello"),
            ["svc.reg.titlePolicyManagerDetected"] = (
                "A registry value (PolicyManager) that may block Remove PIN was detected",
                "Phát hiện giá trị Registry (PolicyManager) có thể chặn Remove PIN"),
            ["svc.reg.titleNoneFound"] = (
                "No Registry/Group Policy blocking the PIN was found",
                "Không thấy chính sách Registry/Group Policy nào chặn PIN"),
            ["svc.reg.gpresultTimeout"] = ("(gpresult timed out)", "(gpresult vượt timeout)"),
            ["svc.reg.source"] = ("Registry / Group Policy", "Registry / Group Policy"),

            // Services: BitLockerDiagnostics
            ["svc.bl.volumeNotFound"] = (
                "System volume not found in Win32_EncryptableVolume.",
                "Không tìm thấy volume hệ thống trong Win32_EncryptableVolume."),
            ["svc.bl.titleUnknown"] = ("Could not determine BitLocker status", "Không xác định được trạng thái BitLocker"),
            ["svc.bl.unknownReason"] = ("Unknown reason.", "Không rõ nguyên nhân."),
            ["svc.bl.titleOn"] = ("BitLocker is ON for drive {0}", "BitLocker đang BẬT trên ổ {0}"),
            ["svc.bl.detailOn"] = (
                "ProtectionStatus = On. Reboots remaining before it auto-resumes (if currently suspended): {0}.\nThe Ngc folder will not be deleted while this is on, unless you suspend BitLocker yourself.",
                "ProtectionStatus = On. Số lần khởi động lại còn lại trước khi tự resume (nếu đang suspend): {0}.\nKhông thao tác xoá Ngc folder khi đang bật, trừ khi bạn tự tạm ngưng BitLocker."),
            ["svc.bl.titleOff"] = ("BitLocker is OFF for drive {0}", "BitLocker đang TẮT trên ổ {0}"),
            ["svc.bl.detailOff"] = (
                "ProtectionStatus = Off. Safe to run the fallback fix (delete the Ngc folder) if needed.",
                "ProtectionStatus = Off. An toàn để xử lý fallback (xoá thư mục Ngc) nếu cần."),
            ["svc.bl.source"] = ("BitLocker", "BitLocker"),

            // Services: AccountDiagnostics
            ["svc.acc.isMicrosoft"] = (
                "Account: Microsoft Account (detected via IdentityCRL cache)",
                "Tài khoản: Microsoft Account (phát hiện qua IdentityCRL cache)"),
            ["svc.acc.isLocal"] = (
                "Account: Local Account (no IdentityCRL cache)",
                "Tài khoản: Local Account (không có IdentityCRL cache)"),
            ["svc.acc.typeError"] = ("Could not determine the account type: {0}", "Không xác định được loại tài khoản: {0}"),
            ["svc.acc.domainJoined"] = ("Machine: joined to an Active Directory domain.", "Máy: đã join Active Directory domain."),
            ["svc.acc.notDomainJoined"] = (
                "Machine: not domain-joined (workgroup or Azure AD only).",
                "Máy: không join AD domain (workgroup hoặc chỉ Azure AD)."),
            ["svc.acc.domainCheckError"] = ("Could not check domain membership: {0}", "Không kiểm tra được domain-join: {0}"),
            ["svc.acc.azureJoined"] = ("Machine: Azure AD Joined / Hybrid Joined (dsregcmd).", "Máy: đã Azure AD Join / Hybrid Join (dsregcmd)."),
            ["svc.acc.notAzureJoined"] = ("Machine: not Azure AD Joined (dsregcmd).", "Máy: không Azure AD Join (dsregcmd)."),
            ["svc.acc.dsregcmdError"] = ("Could not run dsregcmd: {0}", "Không chạy được dsregcmd: {0}"),
            ["svc.acc.titleLocalUnmanaged"] = (
                "Local account, machine not managed by any organization",
                "Tài khoản Local, máy không thuộc tổ chức nào"),
            ["svc.acc.titleManaged"] = (
                "Machine is managed by an organization (Domain / Azure AD)",
                "Máy thuộc quản lý tổ chức (Domain / Azure AD)"),
            ["svc.acc.titleMicrosoftUnmanaged"] = (
                "Microsoft Account, machine not managed by an organization",
                "Tài khoản Microsoft, máy không thuộc tổ chức"),
            ["svc.acc.source"] = ("Account type", "Loại tài khoản"),

            // Services: DiagnosticsEngine
            ["svc.engine.rootCauseGp"] = (
                "The PIN is locked by an organizational policy (Group Policy)",
                "PIN bị khoá do chính sách tổ chức (Group Policy)"),
            ["svc.engine.priority1TitleGp"] = ("Priority 1: Fix Registry/Group Policy", "Ưu tiên 1: Sửa chính sách Registry/Group Policy"),
            ["svc.engine.priority1DescGp"] = (
                "Back up, then set AllowDomainPINLogon (or the equivalent policy) to the value that allows removing the PIN.",
                "Backup rồi sửa AllowDomainPINLogon (hoặc policy tương ứng) về giá trị cho phép remove PIN."),
            ["svc.engine.rootCausePolicyManager"] = ("The PIN is locked by an incorrect registry value", "PIN bị khoá do giá trị Registry sai lệch"),
            ["svc.engine.priority1TitlePm"] = ("Priority 1: Fix the incorrect registry value", "Ưu tiên 1: Sửa giá trị Registry sai lệch"),
            ["svc.engine.priority1DescPm"] = (
                "Back up, then fix the PolicyManager\\Authentication value that is blocking Remove PIN.",
                "Backup rồi sửa giá trị PolicyManager\\Authentication đang chặn Remove PIN."),
            ["svc.engine.rootCauseUnknown"] = (
                "No clear cause found — this may be an Ngc folder issue",
                "Không xác định được nguyên nhân rõ ràng — có thể do lỗi Ngc folder"),
            ["svc.engine.priority2Title"] = ("Priority 2: Delete the Ngc folder (fallback)", "Ưu tiên 2: Xoá thư mục Ngc (fallback)"),
            ["svc.engine.priority2Desc"] = (
                "Back up and delete the Ngc folder to force Windows to rebuild the PIN configuration from scratch. Requires a reboot afterwards.",
                "Backup và xoá thư mục Ngc để buộc Windows tạo lại cấu hình PIN từ đầu. Yêu cầu khởi động lại sau khi xoá."),

            // Services: RegistryFixService
            ["svc.log.actionFixRegistry"] = ("Fix Registry", "Sửa Registry"),
            ["svc.reg.fixDetail"] = ("Set {0} = 1 (old value backed up)", "Đặt {0} = 1 (đã backup giá trị cũ)"),
            ["svc.reg.applyResultMessage"] = ("Applied, and the old value was backed up.", "Đã áp dụng và backup giá trị cũ."),
            ["svc.reg.noBackupMessage"] = ("No backup available to restore.", "Không có bản backup để khôi phục."),
            ["svc.log.actionRollbackRegistry"] = ("Rollback Registry", "Rollback Registry"),
            ["svc.reg.rollbackDetail"] = ("Restored {0} value(s) from backup", "Khôi phục {0} giá trị từ backup"),
            ["svc.reg.rollbackResultMessage"] = ("The original registry value has been restored.", "Đã khôi phục giá trị Registry gốc."),

            // Services: NgcFolderFixService
            ["svc.log.actionDeleteNgc"] = ("Delete Ngc folder", "Xoá Ngc folder"),
            ["svc.ngc.blockedDetail"] = ("BitLocker is on for the system drive", "BitLocker đang bật trên ổ hệ thống"),
            ["svc.ngc.blockedResult"] = ("Blocked — not deleted automatically", "Bị chặn — không tự xoá"),
            ["svc.ngc.blockedMessage"] = (
                "BitLocker is on. The tool will not delete the Ngc folder automatically. Suspend BitLocker yourself first, or enter your recovery key.",
                "BitLocker đang bật. Tool sẽ không tự xoá thư mục Ngc. Hãy tự tạm ngưng BitLocker hoặc nhập recovery key trước."),
            ["svc.ngc.notExistMessage"] = ("The Ngc folder does not exist — nothing to do.", "Thư mục Ngc không tồn tại — không cần xử lý."),
            ["svc.ngc.successDetail"] = ("Backup: {0}. Took ownership and deleted {1}", "Backup: {0}. Đã lấy quyền sở hữu và xoá {1}"),
            ["svc.ngc.successMessage"] = (
                "The Ngc folder has been deleted. Reboot the machine for the PIN to fully disappear.",
                "Đã xoá thư mục Ngc. Khởi động lại máy để PIN biến mất hoàn toàn."),
            ["svc.ngc.accessDeniedResult"] = ("Failed — access denied", "Thất bại — quyền truy cập bị từ chối"),
            ["svc.ngc.accessDeniedMessage"] = (
                "Access still denied even after takeown/icacls. Try running this again in Safe Mode.",
                "Không đủ quyền truy cập ngay cả sau khi takeown/icacls. Hãy thử chạy lại trong Safe Mode."),
            ["svc.ngc.timeoutMessage"] = ("{0} timed out.", "{0} vượt quá thời gian chờ."),

            // Services: BitLockerActionService
            ["svc.log.actionResumeBitlocker"] = ("Resume BitLocker", "Resume BitLocker"),
            ["svc.bla.failedDetail"] = ("Failed: {0}", "Thất bại: {0}"),
            ["svc.bla.resumeSuccessMessage"] = ("BitLocker protection has been turned back on.", "Đã bật lại bảo vệ BitLocker."),
        };
    }
}
