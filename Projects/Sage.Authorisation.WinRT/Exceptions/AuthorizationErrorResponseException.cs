using System;

namespace Sage.Authorisation.WinRT.Exceptions
{
    /// <summary>
    ///     Thrown when an error is returned from the authorisation server for the current
    ///     authorisation attempt.
    /// </summary>
    /// <remarks>
    ///     If this exception occurs, the Sage ID server has rejected the authorisation attempt. You should examine the Error
    ///     and
    ///     ErrorDescription properties to see the reason for the failure. See the following list for types of error and error
    ///     description:
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Error Code</term>
    ///             <description>Explaination</description>
    ///         </listheader>
    ///         <item>
    ///             <term>invalid_request</term>
    ///             <description>
    ///                 The request is missing a required parameter, includes an invalid parameter value, or is
    ///                 otherwise malformed.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>unauthorized_client</term>
    ///             <description>
    ///                 The client is not authorized to request an authorisation code using this method / The
    ///                 authenticated client is not authorized to use this authorisation grant type.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>access_denied</term>
    ///             <description>The resource owner or authorisation server denied the request.</description>
    ///         </item>
    ///         <item>
    ///             <term>unsupported_response_type</term>
    ///             <description>The authorisation server does not support obtaining an authorisation code using this method.</description>
    ///         </item>
    ///         <item>
    ///             <term>invalid_scope</term>
    ///             <description>The requested scope is invalid, unknown, or malformed.</description>
    ///         </item>
    ///         <item>
    ///             <term>server_error</term>
    ///             <description>
    ///                 The authorisation server encountered an unexpected condition which prevented it from
    ///                 fulfilling the request.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>temporarily_unavailable</term>
    ///             <description>
    ///                 The authorisation server is currently unable to handle the request due to a temporary
    ///                 overloading or maintenance of the server.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>invalid_client</term>
    ///             <description>
    ///                 Client authentication failed (e.g. unknown client, no client authentication included, or
    ///                 unsupported authentication method). The authorisation server MAY return an HTTP 401 (Unauthorized)
    ///                 status code to indicate which HTTP authentication schemes are supported. If the client attempted to
    ///                 authenticate via the "Authorisation" request header field, the authorisation server MUST respond with
    ///                 an HTTP 401 (Unauthorized) status code, and include the "WWW-Authenticate" response header field
    ///                 matching the authentication scheme used by the client.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>invalid_grant</term>
    ///             <description>
    ///                 The provided authorisation grant (e.g. authorisation code, resource owner credentials) or
    ///                 refresh token is invalid, expired, revoked, does not match the redirection URI used in the
    ///                 authorisation request, or was issued to another client.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>unsupported_grant_type</term>
    ///             <description>The authorisation grant type is not supported by the authorisation server.</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The following is a list of sub-error codes, which are specific to the Sage ID implementation of OAuth 2.0.
    ///         These are available in the ErrorDescription property of the exception:
    ///     </para>
    ///     <list type="table">
    ///         <listheader>
    ///             <term>ErrorDescription Code</term>
    ///             <description>Explaination</description>
    ///         </listheader>
    ///         <item>
    ///             <term>mismatched_redirect_uri</term>
    ///             <description>
    ///                 The client supplied a redirect uri that is not identical to the redirect uri supplied in the
    ///                 initial authorisation request.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>unexpected_redirect_uri</term>
    ///             <description>
    ///                 The client must not include a redirect uri unless they included the same redirect uri in the
    ///                 initial authorisation request.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>missing_required_parameter</term>
    ///             <description>Used to notify the client that they did not supply a required argument.</description>
    ///         </item>
    ///         <item>
    ///             <term>scope_malformed</term>
    ///             <description>Used to notify the client that the supplied scope could not be parsed.</description>
    ///         </item>
    ///         <item>
    ///             <term>scope_disallowed</term>
    ///             <description>
    ///                 The client is not allowed to request at least one of the permission sets expressed in the
    ///                 scope parameter.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>invalid_permission_set</term>
    ///             <description>The client has requested a permission set with an unknown handle.</description>
    ///         </item>
    ///         <item>
    ///             <term>invalid_scope_arguments</term>
    ///             <description>The client has supplied too many or two few arguments for the requested permission sets.</description>
    ///         </item>
    ///         <item>
    ///             <term>mismatched_resource_app_in_scope</term>
    ///             <description>
    ///                 The client requested a permission set handle for a resource app that the client Id is not
    ///                 linked to.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>state_too_long</term>
    ///             <description>The supplied state string was longer than the maximum allowed length.</description>
    ///         </item>
    ///         <item>
    ///             <term>unexpected_parameters_present</term>
    ///             <description>The request contained at least one unspecified parameter.</description>
    ///         </item>
    ///         <item>
    ///             <term>protocol_violation</term>
    ///             <description>The request tried to jump into an invalid position in the state machine.</description>
    ///         </item>
    ///         <item>
    ///             <term>user_authentication_failed</term>
    ///             <description>
    ///                 A generic error to indicate that the user could not be signed in. We CANNOT be more specific
    ///                 than risk because we'd be leaking information about the existence of accounts.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>user_denied_authorisation</term>
    ///             <description>The user has specifically denied the clients request for authorisation.</description>
    ///         </item>
    ///         <item>
    ///             <term>user_cancelled</term>
    ///             <description>The user has decided to cancel the process of authorisation.</description>
    ///         </item>
    ///         <item>
    ///             <term>attempt_expired</term>
    ///             <description>Part of the authorisation attempt has expired and cannot continue.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    internal class AuthorisationErrorResponseException : AuthorisationException
    {
        #region Error Response Constants

        internal const string INVALID_GRANT = "invalid_grant";

        #endregion

        internal AuthorisationErrorResponseException(string state, string error, string error_description)
            : base(loader.GetString("ExceptionErrorResponseException"), state)
        {
            HResult = ExceptionHResults.AuthorisationErrorResponseException;
            Error = error;
            ErrorDescription = error_description;
        }

        internal AuthorisationErrorResponseException(string message, string state, string error,
            string error_description)
            : base(message, state)
        {
            HResult = ExceptionHResults.AuthorisationErrorResponseException;
            Error = error;
            ErrorDescription = error_description;
        }

        internal AuthorisationErrorResponseException(string message, string error, string error_description,
            Exception inner)
            : base(message, null, inner)
        {
            HResult = ExceptionHResults.AuthorisationErrorResponseException;
            Error = error;
            ErrorDescription = error_description;
        }

        /// <summary>
        ///     The OAuth specification defined error code representing the failure
        /// </summary>
        /// <remarks>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Error Code</term>
        ///             <description>Explaination</description>
        ///         </listheader>
        ///         <item>
        ///             <term>invalid_request</term>
        ///             <description>
        ///                 The request is missing a required parameter, includes an invalid parameter value, or is
        ///                 otherwise malformed.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>unauthorized_client</term>
        ///             <description>
        ///                 The client is not authorized to request an authorisation code using this method / The
        ///                 authenticated client is not authorized to use this authorisation grant type.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>access_denied</term>
        ///             <description>The resource owner or authorisation server denied the request.</description>
        ///         </item>
        ///         <item>
        ///             <term>unsupported_response_type</term>
        ///             <description>The authorisation server does not support obtaining an authorisation code using this method.</description>
        ///         </item>
        ///         <item>
        ///             <term>invalid_scope</term>
        ///             <description>The requested scope is invalid, unknown, or malformed.</description>
        ///         </item>
        ///         <item>
        ///             <term>server_error</term>
        ///             <description>
        ///                 The authorisation server encountered an unexpected condition which prevented it from
        ///                 fulfilling the request.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>temporarily_unavailable</term>
        ///             <description>
        ///                 The authorisation server is currently unable to handle the request due to a temporary
        ///                 overloading or maintenance of the server.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>invalid_client</term>
        ///             <description>
        ///                 Client authentication failed (e.g. unknown client, no client authentication included, or
        ///                 unsupported authentication method). The authorisation server MAY return an HTTP 401 (Unauthorized)
        ///                 status code to indicate which HTTP authentication schemes are supported. If the client attempted to
        ///                 authenticate via the "Authorisation" request header field, the authorisation server MUST respond with
        ///                 an HTTP 401 (Unauthorized) status code, and include the "WWW-Authenticate" response header field
        ///                 matching the authentication scheme used by the client.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>invalid_grant</term>
        ///             <description>
        ///                 The provided authorisation grant (e.g. authorisation code, resource owner credentials) or
        ///                 refresh token is invalid, expired, revoked, does not match the redirection URI used in the
        ///                 authorisation request, or was issued to another client.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>unsupported_grant_type</term>
        ///             <description>The authorisation grant type is not supported by the authorisation server.</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public string Error { get; internal set; }

        /// <summary>
        ///     The OAuth defined error_description field. The SageId implementation of OAuth
        ///     uses this parameter to return a sub-error code. These codes are defined in the
        ///     Web Service API documentation.
        /// </summary>
        /// <remarks>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>ErrorDescription Code</term>
        ///             <description>Explaination</description>
        ///         </listheader>
        ///         <item>
        ///             <term>mismatched_redirect_uri</term>
        ///             <description>
        ///                 The client supplied a redirect uri that is not identical to the redirect uri supplied in the
        ///                 initial authorisation request.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>unexpected_redirect_uri</term>
        ///             <description>
        ///                 The client must not include a redirect uri unless they included the same redirect uri in the
        ///                 initial authorisation request.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>missing_required_parameter</term>
        ///             <description>Used to notify the client that they did not supply a required argument.</description>
        ///         </item>
        ///         <item>
        ///             <term>scope_malformed</term>
        ///             <description>Used to notify the client that the supplied scope could not be parsed.</description>
        ///         </item>
        ///         <item>
        ///             <term>scope_disallowed</term>
        ///             <description>
        ///                 The client is not allowed to request at least one of the permission sets expressed in the
        ///                 scope parameter.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>invalid_permission_set</term>
        ///             <description>The client has requested a permission set with an unknown handle.</description>
        ///         </item>
        ///         <item>
        ///             <term>invalid_scope_arguments</term>
        ///             <description>The client has supplied too many or two few arguments for the requested permission sets.</description>
        ///         </item>
        ///         <item>
        ///             <term>mismatched_resource_app_in_scope</term>
        ///             <description>
        ///                 The client requested a permission set handle for a resource app that the client Id is not
        ///                 linked to.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>state_too_long</term>
        ///             <description>The supplied state string was longer than the maximum allowed length.</description>
        ///         </item>
        ///         <item>
        ///             <term>unexpected_parameters_present</term>
        ///             <description>The request contained at least one unspecified parameter.</description>
        ///         </item>
        ///         <item>
        ///             <term>protocol_violation</term>
        ///             <description>The request tried to jump into an invalid position in the state machine.</description>
        ///         </item>
        ///         <item>
        ///             <term>user_authentication_failed</term>
        ///             <description>
        ///                 A generic error to indicate that the user could not be signed in. We CANNOT be more specific
        ///                 than risk because we'd be leaking information about the existence of accounts.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>user_denied_authorisation</term>
        ///             <description>The user has specifically denied the clients request for authorisation.</description>
        ///         </item>
        ///         <item>
        ///             <term>user_cancelled</term>
        ///             <description>The user has decided to cancel the process of authorisation.</description>
        ///         </item>
        ///         <item>
        ///             <term>attempt_expired</term>
        ///             <description>Part of the authorisation attempt has expired and cannot continue.</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public string ErrorDescription { get; internal set; }

        /// <summary>
        ///     Creates and returns a string representation of the current exception
        /// </summary>
        /// <returns>
        ///     The string representation of this exception.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}\r\n\r\nError={1}, Description={2}.\r\n\r\n{3}", Message, Error, ErrorDescription,
                base.ToString());
        }
    }
}