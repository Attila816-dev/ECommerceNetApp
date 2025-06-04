namespace ECommerceNetApp.Domain.Authorization
{
    public enum TokenType
    {
        /// <summary>
        /// Access token used for authentication and authorization in API requests.
        /// </summary>
        Access,

        /// <summary>
        /// Refresh token used to obtain a new access token without requiring the user to log in again.
        /// </summary>
        Refresh,
    }
}
