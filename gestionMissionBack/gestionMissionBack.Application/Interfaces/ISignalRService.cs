using gestionMissionBack.Application.DTOs.Notification;
using System.Threading.Tasks;

namespace gestionMissionBack.Application.Interfaces
{
    public interface ISignalRService
    {
        Task SendToUserAsync(string connectionId, NotificationDto notification);
        Task SendToAllAsync(NotificationDto notification);
    }
} 