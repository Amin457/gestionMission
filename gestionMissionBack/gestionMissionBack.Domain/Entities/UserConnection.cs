using System;

namespace gestionMissionBack.Domain.Entities
{
    public class UserConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
} 