using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Win32;

namespace FufuTools
{
    public class LoginDataModel
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("device_id")]
        public string DeviceId { get; set; }

        [JsonPropertyName("device_fp")]
        public string DeviceFp { get; set; }

        [JsonPropertyName("cookies")]
        public Dictionary<string, string> Cookies { get; set; }

        [JsonPropertyName("cookie_string")]
        public string CookieString { get; set; }
    }

    public static class SecureCookieStorage
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secure_login_data.dat");
        
        private static byte[] _entropy;
        private static byte[] Entropy => _entropy ??= GetHwIdEntropy();
        
        private static byte[] GetHwIdEntropy()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key != null)
                    {
                        var guid = key.GetValue("MachineGuid")?.ToString();
                        if (!string.IsNullOrWhiteSpace(guid))
                        {
                            return Encoding.UTF8.GetBytes(guid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取 MachineGuid 失败: {ex.Message}");
            }
            
            string fallbackId = $"{Environment.MachineName}_{Environment.UserName}_FufuTools";
            return Encoding.UTF8.GetBytes(fallbackId);
        }
        
        public static void Save(LoginDataModel data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                byte[] plainBytes = Encoding.UTF8.GetBytes(json);
                
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                
                File.WriteAllBytes(FilePath, encryptedBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存失败: {ex.Message}");
            }
        }
        
        public static LoginDataModel Get()
        {
            if (!File.Exists(FilePath)) return null;

            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(FilePath);
                
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
                string json = Encoding.UTF8.GetString(plainBytes);
                
                return JsonSerializer.Deserialize<LoginDataModel>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取Cookie失败{ex.Message}");
                File.Delete(FilePath); 
                return null;
            }
        }
    }
}