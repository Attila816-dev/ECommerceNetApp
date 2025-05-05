namespace ECommerceNetApp.Service.DTO
{
    /// <summary>
    /// Data transfer object for a user.
    /// </summary>
    public class UserDto(string firstName, string lastName, string email)
    {
        /// <summary>
        /// Gets the user FirstName.
        /// </summary>
        public string FirstName { get; } = firstName;

        /// <summary>
        /// Gets the user LastName.
        /// </summary>
        public string LastName { get; } = lastName;

        /// <summary>
        /// Gets the user email.
        /// </summary>
        public string Email { get; } = email;
    }
}
