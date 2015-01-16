using System;
using Windows.Storage;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     The configuration can be used to override some of the default values used by the client library.
    /// </summary>
    public sealed class Configuration
    {
        internal const string RedirectUriError = "error";
        internal const string RedirectUriErrorDescription = "error_description";
        internal const string RedirectUriCred = "cred";
        internal const string StartAuthorisationResponseTypeDefault = "code";
        internal const string ClientCredentialFormat = @"pfx";
        internal const string GetTokensGrantType = "authorization_code";
        internal const string RefreshAccessTokenPostGrantType = "refresh_token";
        private const string RedirectErrorPatternDefault = "/error/";

        private const string GetClientCredentialPostDataFormatterDefault =
            @"code={0}&format={1}&password={2}&devicename={3}&clientutctime={4}";

        private const string GetAccessTokenPostDataFormatterDefault = @"grant_type={0}&code={1}&redirect_uri={2}";
        private const string RefreshAccessTokenPostDataFormatterDefault = @"grant_type={0}&refresh_token={1}&scope={2}";

        private readonly string GetAccessTokenUriDefault =
            @"https://services.sso.services.sage.com/SSO/OAuthServiceMutualSSL/WebGetAccessToken";

        private readonly string GetClientCredentialUriDefault =
            @"https://signon.sso.services.sage.com/SSO/OAuthService/WebGetClientCredential";

        private readonly bool IsSageIdProduction = true;
        private readonly string RedirectUriDefault = @"https://signon.sso.services.sage.com/oauth/native";

        private readonly string StartAuthorisationAttemptUriDefault =
            @"https://signon.sso.services.sage.com/SSO/OAuthService/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";

        private readonly string StartAuthorisationAttemptUriDefault_WithCredential =
            @"https://services.sso.services.sage.com/SSO/OAuthServiceMutualSSL/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";

        //#elif(ALPHA)
//        private const string RedirectUriDefault =                                   @"https://qa1-signon.sso.sagessdp.co.uk/oauth/native";
//        private const string GetClientCredentialUriDefault =                        @"https://qa1-signon.sso.sagessdp.co.uk/SSO/OAuthService/WebGetClientCredential";
//        private const string StartAuthorisationAttemptUriDefault =                  @"https://qa1-signon.sso.sagessdp.co.uk/SSO/OAuthService/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";
//        private const string GetAccessTokenUriDefault =                             @"https://qa1-services.sso.sagessdp.co.uk/SSO/OAuthServiceMutualSSL/WebGetAccessToken";
//        private const string StartAuthorisationAttemptUriDefault_WithCredential =   @"https://qa1-services.sso.sagessdp.co.uk/SSO/OAuthServiceMutualSSL/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";
//#else
//        private const string RedirectUriDefault = @"https://sso1.sso.dev.sage.com/oauth/native";
//        private const string GetClientCredentialUriDefault = @"https://sso.dev.sage.com:8888/SSO/OAuthService/WebGetClientCredential";
//        private const string StartAuthorisationAttemptUriDefault = @"https://sso.dev.sage.com:8888/SSO/OAuthService/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";
//        private const string GetAccessTokenUriDefault = @"https://sso.dev.sage.com:42443/SSO/OAuthServiceMutualSSL/WebGetAccessToken";
//        private const string StartAuthorisationAttemptUriDefault_WithCredential = @"https://sso.dev.sage.com:42443/SSO/OAuthServiceMutualSSL/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";    
//#endif

        /// <summary>
        ///     Initializes a new instance of the <see cref="Configuration" /> class.
        /// </summary>
        public Configuration()
        {
            var configSettings = ApplicationData.Current.LocalSettings;
#if(PRODUCTION)


            IsSageIdProduction = true;


#else
            IsSageIdProduction = false;

#endif
            if (configSettings.Containers != null)
            {
                if (configSettings.Containers.ContainsKey("ConfigurationSettingsContainer"))
                {
                    IsSageIdProduction =
                        Convert.ToBoolean(
                            configSettings.Containers["ConfigurationSettingsContainer"].Values["IsSageProduction"]);
                }
            }

            // for non production we need to use staging URLs, so change it
            if (!IsSageIdProduction)
            {
                RedirectUriDefault = @"https://signon.sso.staging.services.sage.com/oauth/native";
                GetClientCredentialUriDefault =
                    @"https://signon.sso.staging.services.sage.com/SSO/OAuthService/WebGetClientCredential";
                StartAuthorisationAttemptUriDefault =
                    @"https://signon.sso.staging.services.sage.com/SSO/OAuthService/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";
                GetAccessTokenUriDefault =
                    @"https://services.sso.staging.services.sage.com/SSO/OAuthServiceMutualSSL/WebGetAccessToken";
                StartAuthorisationAttemptUriDefault_WithCredential =
                    @"https://services.sso.staging.services.sage.com/SSO/OAuthServiceMutualSSL/WebStartAuthorisationAttempt?response_type={RESPONSETYPE}&client_id={CLIENTID}&redirect_uri={REDIRECTURI}&scope={SCOPE}&state={STATE}&template_name=WinRT";
            }


            RedirectUri = RedirectUriDefault;
            GetClientCredentialUri = GetClientCredentialUriDefault;
            StartAuthorisationAttemptUri = StartAuthorisationAttemptUriDefault;
            StartAuthorisationAttemptWithCredentialUri = StartAuthorisationAttemptUriDefault_WithCredential;
            GetAccessTokenUri = GetAccessTokenUriDefault;
            GetClientCredentialUri = GetClientCredentialUriDefault;
            RedirectErrorPattern = RedirectErrorPatternDefault;
            StartAuthorisationResponseType = StartAuthorisationResponseTypeDefault;
            GetClientCredentialPostDataFormatter = GetClientCredentialPostDataFormatterDefault;
            GetAccessTokenPostDataFormatter = GetAccessTokenPostDataFormatterDefault;
            RefreshAccessTokenPostDataFormatter = RefreshAccessTokenPostDataFormatterDefault;
        }

        /// <summary>
        ///     Gets or sets the redirect URI which is used for a successfull authorisation.
        /// </summary>
        /// <value>
        ///     The redirect URI.
        /// </value>
        public string RedirectUri { get; set; }

        /// <summary>
        ///     Gets or sets the get URI used to retrieve a client credential.
        /// </summary>
        /// <value>
        ///     The get client credential URI.
        /// </value>
        public string GetClientCredentialUri { get; set; }

        /// <summary>
        ///     Gets or sets the URI used to start authorisation attempt (when the client DOESN'T have a valid credential).
        /// </summary>
        /// <value>
        ///     The start authorisation attempt URI.
        /// </value>
        public string StartAuthorisationAttemptUri { get; set; }

        /// <summary>
        ///     Gets or sets the URI used to start authorisation attempt (when the client DOES have a valid credential).
        /// </summary>
        /// <value>
        ///     The start authorisation attempt with credential URI.
        /// </value>
        public string StartAuthorisationAttemptWithCredentialUri { get; set; }

        /// <summary>
        ///     Gets or sets the URI used to get access tokens.
        /// </summary>
        /// <value>
        ///     The get access token URI.
        /// </value>
        public string GetAccessTokenUri { get; set; }

        /// <summary>
        ///     Gets or sets the URI pattern to check for a failed authorisation.
        /// </summary>
        /// <value>
        ///     The redirect error pattern.
        /// </value>
        public string RedirectErrorPattern { get; set; }

        /// <summary>
        ///     Gets or sets the authorisation type.
        ///     <remarks>SageID only supports "code" at the moment</remarks>
        /// </summary>
        public string StartAuthorisationResponseType { get; set; }

        /// <summary>
        ///     Gets or sets the get client credential HTTP post body format string.
        /// </summary>
        /// <value>
        ///     The get client credential post data formatter.
        /// </value>
        public string GetClientCredentialPostDataFormatter { get; set; }

        /// <summary>
        ///     Gets or sets the get access token HTTP post body format string.
        /// </summary>
        /// <value>
        ///     The get access token post data formatter.
        /// </value>
        public string GetAccessTokenPostDataFormatter { get; set; }

        /// <summary>
        ///     Gets or sets the refresh access token HTTP post body format string.
        /// </summary>
        /// <value>
        ///     The refresh access token post data formatter.
        /// </value>
        public string RefreshAccessTokenPostDataFormatter { get; set; }
    }
}