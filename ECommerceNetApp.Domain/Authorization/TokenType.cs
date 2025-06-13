namespace ECommerceNetApp.Domain.Authorization
{
    public enum TokenType
    {
        /// <summary>
        /// Access token used for authentication and authorization in API requests.
        /// </summary>
        Access,

        /// <summary>
        /// ID token used for OpenID Connect compliance, containing user identity information.
        /// This is typically used in OAuth2/OpenID Connect flows.
        /// </summary>
        Id,

        /// <summary>
        /// Refresh token used to obtain a new access token without requiring the user to log in again.
        /// </summary>
        Refresh,
    }
}
