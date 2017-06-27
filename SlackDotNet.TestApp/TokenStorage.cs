using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SlackDotNet
{
    /// <summary>
    /// Options set used to configure a <see cref="TokenStorage"/> instance.
    /// </summary>
    public struct TokenStorageOptions
    {
        /// <summary>
        /// Gets the default <see cref="TokenStorageOptions"/> structure.
        /// </summary>
        public static readonly TokenStorageOptions Default = default(TokenStorageOptions);

        /// <summary>
        /// Gets or sets the entropy used to encrypt the token.
        /// Not supported on all platforms.
        /// </summary>
        public Guid Entropy;

        /// <summary>
        /// Gets or sets the baking file that stores the token.
        /// </summary>
        public string Filename;

        /// <summary>
        /// Gets or sets the data protection scope.
        /// </summary>
        public DataProtectionScope Scope;
    }

    /// <summary>
    /// Represents a cross-platform token storage.
    /// </summary>
    public class TokenStorage
    {
        private TokenStorageOptions options;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes the <see cref="TokenStorage"/> instance with default options.
        /// </summary>
        /// <param name="logger">An optional logger instance.</param>
        public TokenStorage(ILogger logger)
            : this(TokenStorageOptions.Default, logger)
        {
        }

        /// <summary>
        /// Initializes the <see cref="TokenStorage"/> instance with custom options.
        /// </summary>
        /// <param name="options">Options to setup the token storage.</param>
        /// <param name="logger">An optional logger instance.</param>
        public TokenStorage(TokenStorageOptions options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;

            string entrypointAssemblyDirecty = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (string.IsNullOrWhiteSpace(this.options.Filename))
            {
                this.options.Filename = Path.Combine(entrypointAssemblyDirecty, "token");
                logger?.LogTrace($"'{nameof(TokenStorageOptions.Filename)}' not set on '{nameof(TokenStorageOptions)}', fallback to '{this.options.Filename}'");
            }
            else
            {
                string originalFilename = this.options.Filename;
                if (Path.IsPathRooted(originalFilename) == false)
                {
                    this.options.Filename = Path.GetFullPath(Path.Combine(entrypointAssemblyDirecty, originalFilename));
                    logger?.LogTrace($"file '{originalFilename}' not rooted, fallback to '{this.options.Filename}'");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether a token is present or not.
        /// </summary>
        public bool IsTokenAvailable
        {
            get
            {
                return File.Exists(options.Filename);
            }
        }

        /// <summary>
        /// Stores a token in a secure way. (Not yet secure on non-Windows platforms)
        /// </summary>
        /// <param name="token">The token to store securely.</param>
        public void StoreToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (IsTokenAvailable)
                File.Delete(options.Filename);

            bool isWindowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            byte[] entropyBytes = options.Entropy.ToByteArray();
            byte[] workingBytes = Encoding.UTF8.GetBytes(token);

            if (isWindowsPlatform)
                workingBytes = ProtectedData.Protect(workingBytes, entropyBytes, options.Scope);
            else
            {
                logger?.LogWarning("The token will be stored in an non secure way.");

                File.Create(options.Filename).Dispose();
                Process.Start("chmod", $"600 \"{options.Filename}\"");

                // shitty security on non-Windows platforms, better than nothing, but still shitty
                for (int i = 0; i < workingBytes.Length; i++)
                    workingBytes[i] ^= entropyBytes[i % entropyBytes.Length];
            }

            File.WriteAllBytes(options.Filename, workingBytes);
        }

        /// <summary>
        /// Gets a clear token from a secure storage.
        /// </summary>
        /// <returns>Returns a clear token.</returns>
        public string LoadToken()
        {
            if (IsTokenAvailable == false)
                return null;

            byte[] entropyBytes = options.Entropy.ToByteArray();
            byte[] workingBytes = File.ReadAllBytes(options.Filename);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                workingBytes = ProtectedData.Unprotect(workingBytes, entropyBytes, options.Scope);
            else
            {
                // shitty security on non-Windows platforms, better than nothing, but still shitty
                for (int i = 0; i < workingBytes.Length; i++)
                    workingBytes[i] ^= entropyBytes[i % entropyBytes.Length];
            }

            return Encoding.UTF8.GetString(workingBytes);
        }
    }
}
