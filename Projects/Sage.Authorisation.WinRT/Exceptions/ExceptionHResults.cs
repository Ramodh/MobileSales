namespace Sage.Authorisation.WinRT.Exceptions
{
    internal static class ExceptionHResults
    {
        internal const int AuthorisationInProgress = -600;
        internal const int InteractiveAuthorisationRequired = -601;
        internal const int ExpectedLocationHeader = -602;
        internal const int AuthorisationErrorResponseException = -603;
        internal const int CommunicationException = -604;
        internal const int ExpectedFoundException = -605;
    }
}