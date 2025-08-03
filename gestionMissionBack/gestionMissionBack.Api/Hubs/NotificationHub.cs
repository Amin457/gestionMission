using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Domain.Entities;
using gestionMissionBack.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IUserConnectionRepository _userConnectionRepository;

        public NotificationHub(IUserConnectionRepository userConnectionRepository)
        {
            _userConnectionRepository = userConnectionRepository;
        }

        public override async Task OnConnectedAsync()
        {
            // Get user ID from claims (you'll need to implement this based on your auth system)
            var userId = GetUserIdFromContext();
            
            if (userId.HasValue)
            {
                // Store connection
                var userConnection = new UserConnection
                {
                    ConnectionId = Context.ConnectionId,
                    UserId = userId.Value,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                await _userConnectionRepository.AddAsync(userConnection);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove connection
            await _userConnectionRepository.RemoveConnectionAsync(Context.ConnectionId);
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateActivity()
        {
            // Update last activity
            await _userConnectionRepository.UpdateLastActivityAsync(Context.ConnectionId);
        }

        private int? GetUserIdFromContext()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                // Try different claim types that might contain the user ID
                var userIdClaim = Context.User.FindFirst("UserId")?.Value 
                    ?? Context.User.FindFirst("sub")?.Value 
                    ?? Context.User.FindFirst("nameid")?.Value
                    ?? Context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            
            return null;
        }
    }
} 