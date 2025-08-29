using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Entities
{



    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsActive { get; private set; }
        public IReadOnlyList<string> Roles => _roles.AsReadOnly();

        private List<string> _roles = new();

        // Private constructor for EF
        private User()
        {
            Email = string.Empty;
            Username = string.Empty;
            PasswordHash = string.Empty;
            _roles = new List<string>();
        }

        public User(string email, string username, string passwordHash, string? firstName = null, string? lastName = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

            Email = email.ToLowerInvariant();
            Username = username;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            _roles = new List<string> { "User" };
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void AddRole(string role)
        {
            if (!_roles.Contains(role))
                _roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            _roles.Remove(role);
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void UpdateProfile(string? firstName, string? lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }

}

