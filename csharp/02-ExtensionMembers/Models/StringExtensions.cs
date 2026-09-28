using System.Text;
using System.Text.RegularExpressions;

namespace ExtensionBlocks.Models;

/// <summary>
/// C# 14 Extension Block for string type demonstrating extension methods.
/// Extension methods provide a clean way to add functionality to existing types.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10 RC 1.
/// This provides cleaner organization of related extension methods in a single block,
/// better IntelliSense, and improved performance compared to traditional extension methods.
/// </summary>
public static class StringExtensions
{
    extension (string input)
    {
        /// <summary>
        /// Extension method that converts a string to title case.
        /// Demonstrates string manipulation in extension methods.
        /// </summary>
        public string ToTitleCase()
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            StringBuilder result = new();

            foreach (var word in words)
            {
                if (result.Length > 0)
                    result.Append(' ');

                if (word.Length > 0)
                {
                    result.Append(char.ToUpper(word[0]));
                    if (word.Length > 1)
                        result.Append(word[1..].ToLower());
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Extension method that counts the number of words in a string.
        /// Simple counting extension method with null safety.
        /// </summary>
        public int WordCount()
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            return input.Split([' ', '\t', '\n', '\r'], 
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Extension method that removes all non-alphanumeric characters.
        /// Demonstrates regex usage in extension methods.
        /// </summary>
        public string ToAlphanumericOnly()
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input, @"[^a-zA-Z0-9]", "");
        }

        /// <summary>
        /// Extension method that truncates a string to a specified length with ellipsis.
        /// Practical string utility demonstrating parameter handling in extension methods.
        /// </summary>
        public string Truncate(int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(input) || maxLength < 0)
                return input;

            if (input.Length <= maxLength)
                return input;

            var truncatedLength = Math.Max(0, maxLength - ellipsis.Length);
            return input[..truncatedLength] + ellipsis;
        }

        /// <summary>
        /// Extension method that checks if a string is a valid email address.
        /// Boolean extension method with regex validation.
        /// </summary>
        public bool IsValidEmail()
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, emailPattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Extension method that reverses the string.
        /// Simple string manipulation demonstrating character array operations.
        /// </summary>
        public string Reverse()
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Extension method that extracts initials from a full name.
        /// Combines multiple string operations in a single extension method.
        /// </summary>
        public string GetInitials()
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = new StringBuilder();

            foreach (var word in words)
            {
                if (word.Length > 0)
                    initials.Append(char.ToUpper(word[0]));
            }

            return initials.ToString();
        }

        /// <summary>
        /// Extension method that counts occurrences of a substring.
        /// Demonstrates parameter passing and counting logic in extension methods.
        /// </summary>
        public int CountOccurrences(string substring, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(substring))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = input.IndexOf(substring, index, comparison)) != -1)
            {
                count++;
                index += substring.Length;
            }

            return count;
        }
    }
}