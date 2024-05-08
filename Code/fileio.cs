using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using static DailyCheck.DebugLogger;

namespace DailyCheck
{
    public static class FileIO
    {
        private static readonly byte[] InitVector = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];

        public static string GetFirstArgAsFilename()  // if the first argument is a valid file returns its full name
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args != null && args.Length > 1)
                {
                    FileInfo fi;
                    fi = new FileInfo(args[1]);
                    return fi.FullName;
                }
            }
            catch { /***/ }
            return string.Empty;
        }

        /// <summary>
        /// Input strings nust not be "large". The byte-array representation of a string should have a 16-bit size (ushort)
        /// </summary>
        /// <param name="str1">First unicode string</param>
        /// <param name="str2">Second unicode string</param>
        /// <param name="outputBytes">AES-256 encrypted byte array</param>
        /// <returns></returns>
        public static bool Encrypt(string str1, string str2, out byte[] outputBytes)
        {
            try
            {
                byte[] inputBytes = (str1, str2).ToByteArray();
                SymmetricAlgorithm crypt = Aes.Create();
                crypt.BlockSize = 128;  // For AES, the only valid block size is 128 bits.
                crypt.Key = MachineIdHash256();
                crypt.IV = InitVector;

                using (MemoryStream memoryStream = new())
                {
                    using (CryptoStream cryptoStream = new(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                    }
                    outputBytes = memoryStream.ToArray();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"Exception during ecryption:\n{ex}");
                outputBytes = [];
                return false;
            }
        }

        public static bool Decrypt(byte[] inputBytes, out string str1, out string str2)
        {
            try
            {
                SymmetricAlgorithm crypt = Aes.Create();
                crypt.Key = MachineIdHash256();
                crypt.BlockSize = 128;  // For AES, the only valid block size is 128 bits.
                crypt.IV = InitVector;

                using (MemoryStream memoryStream = new(inputBytes))
                {
                    byte[] decryptedBytes = new byte[inputBytes.Length];
                    using (CryptoStream cryptoStream = new(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        int count = cryptoStream.ReadAtLeast(decryptedBytes, decryptedBytes.Length, false);
                    }
                    (str1, str2) = decryptedBytes.ToStringsTuple();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"Exception during decryption:\n{ex}");
                str1 = str2 = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Creates a  PC specific key-password
        /// </summary>
        /// <returns>256-bits key</returns>
        /// <exception cref="ArgumentNullException">
        /// <exception cref="ArgumentException">
        /// <exception cref="ArgumentOutOfRangeException">
        /// <exception cref="InvalidCastException">
        /// <exception cref="EncoderFallbackException">
        private static byte[] MachineIdHash256()
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

            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProductId <= Windows Installation ID
            sb.Append(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")?.GetValue("ProductId")?.ToString() ?? "");

            searcher = new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
            {
                sb.Append(queryObj["Product"]);
            }

            string finalString = sb.Replace(" ", "").ToString().FormatSealed();

            return SHA256.HashData(Encoding.ASCII.GetBytes(finalString));
        }
    }

    public static class TupleExtentions
    {
        private readonly static (string, string) _emptyTuple = ("", "");

        public static byte[] ToByteArray(this (string, string) stringTuple)
        {
            byte[] firstBytes = Encoding.Unicode.GetBytes(stringTuple.Item1);
            byte[] secondBytes = Encoding.Unicode.GetBytes(stringTuple.Item2);

            Debug.Assert(firstBytes.Length < 0xFFFF && secondBytes.Length < 0xFFFF);
            ushort firstLength = (ushort)firstBytes.Length;  // input strings must be 'short-length' long
            ushort secondLength = (ushort)secondBytes.Length;

            byte[] result = new byte[firstLength + secondLength + 2 * sizeof(ushort)];
            Array.Copy(BitConverter.GetBytes(firstLength), 0, result, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes(secondLength), 0, result, sizeof(ushort), sizeof(ushort));

            if (firstLength > 0) Array.Copy(firstBytes, 0, result, 2 * sizeof(ushort), firstLength);
            if (secondLength > 0) Array.Copy(secondBytes, 0, result, 2 * sizeof(ushort) + firstLength, secondLength);

            return result;
        }

        public static (string, string) ToStringsTuple(this byte[] data)
        {
            if (data.Length < 2 * sizeof(ushort)) return _emptyTuple;

            ushort firstLength = BitConverter.ToUInt16(data, 0);
            ushort secondLength = BitConverter.ToUInt16(data, sizeof(ushort));

            if (data.Length < firstLength + secondLength + 2 * sizeof(ushort)) return _emptyTuple;

            string first = (firstLength == 0) ? string.Empty :
                             Encoding
                             .Unicode
                             .GetString(data[(2 * sizeof(ushort))..(2 * sizeof(ushort) + firstLength)]);
            string second = (secondLength == 0) ? string.Empty :
                            Encoding
                            .Unicode
                            .GetString(data[(2 * sizeof(ushort) + firstLength)..(2 * sizeof(ushort) + firstLength + secondLength)]);

            return (first, second);
        }
    }
}
