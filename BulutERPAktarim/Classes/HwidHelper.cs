// HwidHelper.cs
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace BulutERPAktarim.Classes
{
    internal static class HwidHelper
    {
        /// <summary>
        /// Bilgisayara özgü benzersiz HWID üretir.
        /// CPU ID + Anakart Seri No + Disk Seri No birleşiminden SHA256 hash alır.
        /// </summary>
        internal static string GetHwid()
        {
            try
            {
                string cpuId = GetWmiValue("Win32_Processor", "ProcessorId");
                string boardSerial = GetWmiValue("Win32_BaseBoard", "SerialNumber");
                string diskSerial = GetWmiValue("Win32_DiskDrive", "SerialNumber");
                string raw = $"{cpuId}-{boardSerial}-{diskSerial}";
                return ComputeSha256(raw);
            }
            catch
            {
                // WMI başarısız olursa fallback: MachineName + UserName hash
                string fallback = $"{Environment.MachineName}-{Environment.UserName}";
                return ComputeSha256(fallback);
            }
        }
        private static string GetWmiValue(string wmiClass, string property)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string val = obj[property]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }
            catch { }
            return "UNKNOWN";
        }
        private static string ComputeSha256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }
    }
}