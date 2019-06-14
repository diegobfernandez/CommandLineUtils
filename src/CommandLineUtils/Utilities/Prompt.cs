// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace McMaster.Extensions.CommandLineUtils
{
    /// <summary>
    /// Utilities for getting input from an interactive console.
    /// </summary>
    public static class Prompt
    {
        private const char Backspace = '\b';

        /// <summary>
        /// Gets a yes/no response from the console after displaying a <paramref name="prompt" />.
        /// <para>
        /// The parsing is case insensitive. Valid responses include: yes, no, y, n.
        /// </para>
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>True is 'yes'</returns>
        public static bool GetYesNo(string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var truthyValues = new[] { "y", "yes" };
            var falsyValues = new[] { "n", "no" };
            var truthyHint = "y";
            var falsyHint = "n";
            return GetBoolean(prompt, defaultAnswer, truthyValues, falsyValues, truthyHint, falsyHint, promptColor, promptBgColor);
        }

        /// <summary>
        /// Gets a boolean response from the console after displaying a <paramref name="prompt" />.
        /// <para>
        /// The parsing is case insensitive. Valid responses must be configured.
        /// </para>
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="truthyValues">These responses yield a true value</param>
        /// <param name="falsyValues">These responses yield a false value</param>
        /// <param name="truthyHint"></param>
        /// <param name="falsyHint"></param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>True is 'yes'</returns>
        public static bool GetBoolean(string prompt, bool defaultAnswer, IEnumerable<string> truthyValues, IEnumerable<string> falsyValues, string truthyHint, string falsyHint, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            Func<bool, string, string> toUpper = (u, s) => u ? s.ToUpper() : s.ToLower();

            if (truthyValues is null || !truthyValues.Any())
            {
                truthyValues = new[] { "y", "yes" };
            }

            if (falsyValues is null || !falsyValues.Any())
            {
                falsyValues = new[] { "n", "no" };
            }

            if (string.IsNullOrWhiteSpace(truthyHint))
            {
                truthyHint = truthyValues.First();
            }

            if (string.IsNullOrWhiteSpace(falsyHint))
            {
                falsyHint = falsyValues.First();
            }

            var answerHint = $"[{toUpper(defaultAnswer, truthyHint)}/{toUpper(defaultAnswer, truthyHint)}]";
            do
            {
                Write($"{prompt} {answerHint}", promptColor, promptBgColor);
                Console.Write(' ');

                string resp;
                using (ShowCursor())
                {
                    resp = Console.ReadLine()?.ToLower()?.Trim();
                }

                if (string.IsNullOrEmpty(resp))
                {
                    return defaultAnswer;
                }

                if (falsyValues.Contains(resp))
                {
                    return false;
                }

                if (truthyValues.Contains(resp))
                {
                    return true;
                }

                Console.WriteLine($"Invalid response '{resp}'. Please answer '{truthyHint}' or '{falsyHint}' or CTRL+C to exit.");
            }
            while (true);
        }

        /// <summary>
        /// Gets a console response from the console after displaying a <paramref name="prompt" />.
        /// </summary>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="defaultValue">If the user enters a blank response, return this value instead.</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response the user gave. Can be null or empty</returns>
        public static string GetString(string prompt, string defaultValue = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            if (defaultValue != null)
            {
                prompt = $"{prompt} [{defaultValue}]";
            }

            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');

            string resp;
            using (ShowCursor())
            {
                resp = Console.ReadLine();
            }

            if (!string.IsNullOrEmpty(resp))
            {
                return resp;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a response that contains a password. Input is masked with an asterisk.
        /// </summary>
        /// <param name="prompt">The question to display on command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The password as plaintext. Can be null or empty.</returns>
        public static string GetPassword(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var resp = new StringBuilder();

            foreach (var key in ReadObfuscatedLine(prompt, promptColor, promptBgColor))
            {
                switch (key)
                {
                    case Backspace:
                        resp.Remove(resp.Length - 1, 1);
                        break;
                    default:
                        resp.Append(key);
                        break;
                }
            }

            return resp.ToString();
        }

#if NET45 || NETSTANDARD2_0
        /// <summary>
        /// Gets a response as a SecureString object. Input is masked with an asterisk.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>A finalized SecureString object, may be empty.</returns>
        public static System.Security.SecureString GetPasswordAsSecureString(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            var secureString = new System.Security.SecureString();

            foreach (var key in ReadObfuscatedLine(prompt, promptColor, promptBgColor))
            {
                switch (key)
                {
                    case Backspace:
                        secureString.RemoveAt(secureString.Length - 1);
                        break;
                    default:
                        secureString.AppendChar(key);
                        break;
                }
            }

            secureString.MakeReadOnly();
            return secureString;
        }
#elif NETSTANDARD1_6
#else
#error Target frameworks should be updated
#endif

        /// <summary>
        /// Base implementation of GetPassword and GetPasswordAsString. Prompts the user for
        /// a password and yields each key as the user inputs. Password is masked as input. Pressing Escape will reset the password
        /// by flushing the stream with backspace keys.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="promptColor">The console color to use for the prompt</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>A stream of characters as input by the user including Backspace for deletions.</returns>
        private static IEnumerable<char> ReadObfuscatedLine(string prompt, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            const string whiteOut = "\b \b";
            Write(prompt, promptColor, promptBgColor);
            Console.Write(' ');
            const ConsoleModifiers IgnoredModifiersMask = ConsoleModifiers.Alt | ConsoleModifiers.Control;
            var readChars = 0;
            ConsoleKeyInfo key;
            do
            {
                using (ShowCursor())
                {
                    key = Console.ReadKey(intercept: true);
                }

                if ((key.Modifiers & IgnoredModifiersMask) != 0)
                {
                    continue;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        break;
                    case ConsoleKey.Backspace:
                        if (readChars > 0)
                        {
                            Console.Write(whiteOut);
                            --readChars;
                            yield return Backspace;
                        }
                        break;
                    case ConsoleKey.Escape:
                        // Reset the password
                        while (readChars > 0)
                        {
                            Console.Write(whiteOut);
                            yield return Backspace;
                            --readChars;
                        }
                        break;
                    default:
                        readChars += 1;
                        Console.Write('*');
                        yield return key.KeyChar;
                        break;
                }
            }
            while (key.Key != ConsoleKey.Enter);
        }

        /// <summary>
        /// Gets an integer response from the console after displaying a <paramref name="prompt" />.
        /// </summary>
        /// <param name="prompt">The question to display on the command line</param>
        /// <param name="defaultAnswer">If the user provides an empty response, which value should be returned</param>
        /// <param name="promptColor">The console color to display</param>
        /// <param name="promptBgColor">The console background color for the prompt</param>
        /// <returns>The response as a number</returns>
        public static int GetInt(string prompt, int? defaultAnswer = null, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
        {
            do
            {
                Write(prompt, promptColor, promptBgColor);
                if (defaultAnswer.HasValue)
                {
                    Write($" [{defaultAnswer.Value}]", promptColor, promptBgColor);
                }
                Console.Write(' ');

                string resp;
                using (ShowCursor())
                {
                    resp = Console.ReadLine()?.ToLower()?.Trim();
                }

                if (string.IsNullOrEmpty(resp))
                {
                    if (defaultAnswer.HasValue)
                    {
                        return defaultAnswer.Value;
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid number or press CTRL+C to exit.");
                        continue;
                    }
                }

                if (int.TryParse(resp, out var result))
                {
                    return result;
                }

                Console.WriteLine($"Invalid number '{resp}'. Please enter a valid number or press CTRL+C to exit.");
            }
            while (true);
        }

        private static void Write(string value, ConsoleColor? foreground, ConsoleColor? background)
        {
            if (foreground.HasValue)
            {
                Console.ForegroundColor = foreground.Value;
            }

            if (background.HasValue)
            {
                Console.BackgroundColor = background.Value;
            }

            Console.Write(value);

            if (foreground.HasValue || background.HasValue)
            {
                Console.ResetColor();
            }
        }

        private static IDisposable ShowCursor() => new CursorState();

        private class CursorState : IDisposable
        {
            private readonly bool _original;

            public CursorState()
            {
                try
                {
                    _original = Console.CursorVisible;
                }
                catch
                {
                    // some platforms throw System.PlatformNotSupportedException
                    // Assume the cursor should be shown
                    _original = true;
                }

                TrySetVisible(true);
            }

            private void TrySetVisible(bool visible)
            {
                try
                {
                    Console.CursorVisible = visible;
                }
                catch
                {
                    // setting cursor may fail if output is piped or permission is denied.
                }
            }

            public void Dispose()
            {
                TrySetVisible(_original);
            }
        }
    }
}
