using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DailyCheck
{
    public static class StringExtensions
    {
        private static readonly List<KeyValuePair<String, String>> ReplaceDict = [
            new KeyValuePair<string, string>("04", "G"),
            new KeyValuePair<string, string>("44", "H"),
            new KeyValuePair<string, string>("00", "I"),
            new KeyValuePair<string, string>("II", "J"),
            new KeyValuePair<string, string>("01", "K"),
            new KeyValuePair<string, string>("02", "L"),
            new KeyValuePair<string, string>("03", "M"),
            new KeyValuePair<string, string>("04", "N"),
            new KeyValuePair<string, string>("05", "O"),
            new KeyValuePair<string, string>("06", "P"),
            new KeyValuePair<string, string>("07", "Q"),
            new KeyValuePair<string, string>("08", "R"),
            new KeyValuePair<string, string>("09", "S"),
            new KeyValuePair<string, string>("0A", "T"),
            new KeyValuePair<string, string>("0B", "U"),
            new KeyValuePair<string, string>("0C", "V"),
            new KeyValuePair<string, string>("0D", "W"),
            new KeyValuePair<string, string>("0E", "X"),
            new KeyValuePair<string, string>("0F", "Y"),
            new KeyValuePair<string, string>("66", "Z"),
        ];
        private static readonly IEnumerable<KeyValuePair<String, String>> InvertedReplaceDict = ReplaceDict.AsEnumerable().Reverse();
        private static int RandomSeed(int length, int index) => 31 * index - length * 17;
        public static string FormatSealed(this string inputString, bool undo = false, int index = 0)
        {
            if (inputString.Length == 0 && !undo) return "%%";
            if (undo && inputString == "%%") return "";

            int key;
            if (undo)
            {
                string knownLengthString = inputString.UnRenameHex();
                key = RandomSeed(knownLengthString.Length / 4, index);
                return knownLengthString.DeShuffle(key).FromUTFCodeString();
            }

            key = RandomSeed(inputString.Length, index);
            return inputString.ToUTFCodeString().Shuffle(key).RenameHex();
        }


        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        private static int[] GetShuffleExchanges(int size, int key)
        {
            int[] exchanges = new int[size - 1];
            var rand = new Random(key);
            for (int i = size - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                exchanges[size - 1 - i] = n;
            }
            return exchanges;
        }

        private static string Shuffle(this string toShuffle, int key)
        {
            int size = toShuffle.Length;
            char[] chars = toShuffle.ToArray();
            var exchanges = GetShuffleExchanges(size, key);
            for (int i = size - 1; i > 0; i--)
            {
                int n = exchanges[size - 1 - i];
                (chars[n], chars[i]) = (chars[i], chars[n]);
            }
            return new string(chars);
        }

        private static string DeShuffle(this string shuffled, int key)
        {
            int size = shuffled.Length;
            char[] chars = shuffled.ToArray();
            var exchanges = GetShuffleExchanges(size, key);
            for (int i = 1; i < size; i++)
            {
                int n = exchanges[size - i - 1];
                (chars[n], chars[i]) = (chars[i], chars[n]);
            }
            return new string(chars);
        }

        private static string ToUTFCodeString(this string str)
        {
            List<string> output = [];
            for (var i = 0; i < str.Length; i++)
                output.Add(string.Format("{0:X4}", (ushort)str[i]));
            return string.Concat(output);
        }

        private static string FromUTFCodeString(this string str)
        {
            List<char> output = [];
            for (var i = 0; i < str.Length; i += 4)
                output.Add((char)Convert.ToUInt16(str.Substring(i, 4), 16));
            return string.Concat(output);
        }

        private static string RenameHex(this string hexString)
        {
            StringBuilder sb = new(hexString);
            foreach (var pair in ReplaceDict) sb.Replace(pair.Key, pair.Value);
            return sb.ToString();
        }

        private static string UnRenameHex(this string hexString)
        {
            StringBuilder sb = new(hexString);
            foreach (var pair in InvertedReplaceDict) sb.Replace(pair.Value, pair.Key);
            return sb.ToString();
        }
    }
}
