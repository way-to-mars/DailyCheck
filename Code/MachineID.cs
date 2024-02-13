using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DailyCheck
{
    internal class MachineId
    {
        public static string[] Get()
        {
            StringBuilder sb = new();
            ManagementObjectSearcher searcher = new("root\\CIMV2", "SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
            {
                sb.Append(queryObj["NumberOfCores"]);
                sb.Append(queryObj["ProcessorId"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["SocketDesignation"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
            {
                sb.Append(queryObj["Manufacturer"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["Version"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
            {
                sb.Append(queryObj["Product"]);
            }

            string finalString = sb.ToString();
            string finalSealed = finalString.Replace(" ", "").FormatSealed();

            HashAlgorithm hash = MD5.Create();
            SHA256Managed sha = new SHA256Managed();

            var keyMD5 = hash.ComputeHash(Encoding.ASCII.GetBytes(finalSealed));
            var keySHA256 = sha.ComputeHash(Encoding.ASCII.GetBytes(finalSealed));
            var strMD5 = BitConverter.ToString(keyMD5);
            var strSHA256 = BitConverter.ToString(keySHA256);
            Console.WriteLine($"{finalSealed}\nMD5 = {strMD5}\nSHA256 = {strSHA256}");

            return [finalString, finalSealed, strMD5, strSHA256];
        }

    }
}
