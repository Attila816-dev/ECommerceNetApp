using ECommerceNetApp.Domain.Enums;

namespace ECommerceNetApp.Domain.Entities
{
    public class UserEntity : BaseEntity<int>
    {
        public const int MinPasswordLength = 8;

        public const int MaxFirstNameLength = 100;

        public const int MaxLastNameLength = 100;

        public const int MaxEmailLength = 200;

        internal UserEntity(int? id, string email, string passwordHash, string firstName, string lastName, UserRole role)
            : base(default)
        {
            if (id.HasValue)
            {
                Id = id.Value;
            }

            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        // For EF Core
        protected UserEntity()
            : base(default)
        {
            Email = string.Empty;
            PasswordHash = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        public string Email { get; private set; }

        public string PasswordHash { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? LastLogin { get; private set; }

        public UserRole Role { get; private set; }

        public string FullName => $"{FirstName} {LastName}";

        public static UserEntity Create(
            string email,
            string passwordHash,
            string firstName,
            string lastName,
            UserRole role,
            int? id = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));

            return new UserEntity(id, email, passwordHash, firstName, lastName, role);
        }

        public void UpdateProfile(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateRole(UserRole role)
        {
            Role = role;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLoginDate()
        {
            LastLogin = DateTime.UtcNow;
        }

        public override void MarkAsDeleted()
        {
        }
    }
}
