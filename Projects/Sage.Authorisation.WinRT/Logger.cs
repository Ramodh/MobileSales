using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sage.Authorisation.WinRT
{
    internal class Logger
    {
        #region Events / Delegates

        public delegate void LogEventHandler(LogEvent e);

        public event LogEventHandler LogMessage;

        #endregion

        /// <summary>
        /// Logs a message for LogMessage event subscribers at Info level
        /// </summary>
        public void Info(LogEventType code, string message)
        {
            Log(LogLevel.Info, code, message);
        }

        /// <summary>
        /// Logs a message for LogMessage event subscribers at Warning level
        /// </summary>
        public void Warning(LogEventType code, string message)
        {
            Log(LogLevel.Warning, code, message);
        }

        /// <summary>
        /// Logs a message for LogMessage event subscribers at Error level
        /// </summary>
        public void Error(LogEventType code, string message)
        {
            Log(LogLevel.Error, code, message);
        }

        /// <summary>
        /// Logs a message for LogMessage event subscribers at Diagnostic level
        /// </summary>
        public void Diagnostic(LogEventType code, string message)
        {
            Log(LogLevel.Diagnostic, code, message);
        }


        #region Private Members

        /// <summary>
        /// Raise log event on owner thread
        /// </summary>
        private void Log(LogLevel level, LogEventType @event, string message)
        {
            LogEvent newEvent = new LogEvent(level, @event, message);

            if (LogMessage != null)
            {
                LogMessage(newEvent);
            }

            Debug.WriteLine(String.Format("{0}, {1}, {2}", level.ToString(), @event.ToString(), message));
        }

        #endregion
    }

    /// <summary>
    /// The severity of the log event
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Diagnostic log level
        /// </summary>
        Diagnostic,

        /// <summary>
        /// Info log level
        /// </summary>
        Info,

        /// <summary>
        /// Warning log level
        /// </summary>
        Warning,

        /// <summary>
        /// Error log level
        /// </summary>
        Error
    }

    /// <summary>
    /// The situation, operation or state that occured to raise the log event
    /// </summary>
    public enum LogEventType
    {
        /// <summary>
        /// An exception occured while processing a HTTP(S) request on a background thread
        /// </summary>
        BackgroundHttpException,

        /// <summary>
        /// The secure browser has been displayed
        /// </summary>
        BrowserActivated,

        /// <summary>
        /// The secure browser is navigating/navigated to a different URI
        /// </summary>
        BrowserNavigating,

        /// <summary>
        /// A secure error has occured in the secure browser, e.g. TLS error
        /// </summary>
        BrowserSecurityError,

        /// <summary>
        /// The secure browser is about to be started
        /// </summary>
        BrowserStarting,

        /// <summary>
        /// An access token has been added into the in memory cache of access tokens
        /// </summary>
        CacheAccessToken,

        /// <summary>
        /// The client credential is being cleared
        /// </summary>
        ClearingClientCredential,

        /// <summary>
        /// The secure storage area is cleared
        /// </summary>
        ClearedSecureStorage,

        /// <summary>
        /// The client credential is expired
        /// </summary>
        ClientCredentialExpired,

        /// <summary>
        /// A client credential could not be found
        /// </summary>
        ClientCredentialNotFound,

        /// <summary>
        /// An authorisation code is about to be exchanged for tokens
        /// </summary>
        ExchangeAuthorisationCode,

        /// <summary>
        /// A valid cached access token has been found in memory
        /// </summary>
        FoundCachedAccessToken,

        /// <summary>
        /// A valid client credential has been found
        /// </summary>
        FoundClientCredential,

        /// <summary>
        /// A valid refresh token has been found
        /// </summary>
        FoundRefreshToken,

        /// <summary>
        /// A HTTP(S) call to retrieve a client credential is about to be performed
        /// </summary>
        HttpGetClientCredential,

        /// <summary>
        /// A HTTP(S) call to retrieve a client credential has completed
        /// </summary>
        HttpGetClientCredentialComplete,

        /// <summary>
        /// A HTTP(S) call to retrieve access / refresh tokens is about to be performed
        /// </summary>
        HttpGetTokens,

        /// <summary>
        /// A HTTP(S) call to retrieve access / refresh tokens has completed
        /// </summary>
        HttpGetTokensComplete,

        /// <summary>
        /// A HTTP(S) call to start an authorisation attempt is about to be performed
        /// </summary>
        HttpStartAuthorisation,

        /// <summary>
        /// A HTTP(S) call to start an authorisation attempt has completed
        /// </summary>
        HttpStartAuthorisationComplete,

        /// <summary>
        /// Interactive sign on, with the user, is about to be started
        /// </summary>
        InteractiveAuthentication,

        /// <summary>
        /// The server has rejected the refresh token that has been presented. It may be expired.
        /// </summary>
        InvalidRefreshToken,

        /// <summary>
        /// A client credential is being persisted
        /// </summary>
        PersistClientCredential,

        /// <summary>
        /// A value is being writen to secure storage
        /// </summary>
        PersistValue,

        /// <summary>
        /// A web proxy server has been specified and will be used for the browser and HTTP(S) calls
        /// </summary>
        ProxyServerSpecified,

        /// <summary>
        /// A value is being read from secure storage
        /// </summary>
        ReadValue,

        /// <summary>
        /// A redirect to an error page has been requested by the server
        /// </summary>
        RedirectErrorFound,

        /// <summary>
        /// A redirect that must be handled by the client has been requested by the server
        /// </summary>
        RedirectUriFound,

        /// <summary>
        /// A refresh token for the current context could not be found
        /// </summary>
        RefreshTokenNotFound,

        /// <summary>
        /// Reset token duration flag specified. Removing cached access and refresh tokens
        /// </summary>
        ResetDuration,

        /// <summary>
        /// The server has updated the scope of the authorisation attempt.
        /// </summary>
        ServerUpdatedScope,

        /// <summary>
        /// The lock file is corrupt.
        /// </summary>
        CorruptLockFile,

        /// <summary>
        /// The WebAuthenticationBroker is about to be launched to perform an interactive authorisation
        /// </summary>
        AuthenticateUsingBroker
    }
}
