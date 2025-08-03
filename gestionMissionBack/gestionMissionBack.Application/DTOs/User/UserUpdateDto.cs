using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Application.DTOs.User
{
    public class UserUpdateDto
    {
        public int UserId { get; set; } // Required for identifying the user
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; } // Plain password, hashed in service
        public string? Role { get; set; } // Role name (e.g., "Admin")
        public DriverStatus? CurrentDriverStatus { get; set; }

    }
}