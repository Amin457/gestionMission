using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace gestionMissionBack.Api.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string connectionId, NotificationDto notification)
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToAllAsync(NotificationDto notification)
        {
            await _hubContext.Clients.All
                .SendAsync("ReceiveNotification", notification);
        }
    }
} 