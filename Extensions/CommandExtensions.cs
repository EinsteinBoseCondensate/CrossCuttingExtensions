using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;

namespace CrossCuttingExtensions.Extensions
{
    public static class CommandExtensions
    {
        public static SecureString ConvertToSecureString(this string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }
        /// <summary>
        /// Execute a given command with no credentials
        /// </summary>
        /// <param name="command"></param>
        /// <param name="hidden">true to hide command prompt, default is false</param>
        public static string ExecuteNoCredentialsCommand(this string command, bool hidden = false, Func<StreamReader, string> behaviour = null)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = !hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.WorkingDirectory = "C:\\Windows\\System32";
                startInfo.Arguments = command;
                process.StartInfo = startInfo;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                process.WaitForExit();
                var errorSuccessChallenge = string.IsNullOrEmpty(process.StandardError?.ReadLine()?.ToString()) ? process.StandardOutput : process.StandardError;
                behaviour?.Invoke(errorSuccessChallenge);
                return errorSuccessChallenge.ReadLine();
            }
            catch (Exception ex)
            {
                var catchStream = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes($"process didn't run : {ex}")));
                behaviour?.Invoke(catchStream);
                return catchStream.ReadLine();
            }
        }
    }
}
