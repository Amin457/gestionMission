using gestionMissionBack.Application.DTOs.Notification;
using gestionMissionBack.Application.Interfaces;
using gestionMissionBack.Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using gestionMissionBack.Domain.Enums;

namespace gestionMissionBack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
            [FromQuery] NotificationFilter filter)
        {
            try
            {
                // Get user ID from claims (simplified - you'll need to implement based on your auth)
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var notifications = await _notificationService.GetUserNotificationsAsync(userId.Value, filter);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var notification = await _notificationService.GetNotificationByIdAsync(id, userId.Value);
                if (notification == null)
                    return NotFound();

                return Ok(notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(createDto);
                return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notification);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var success = await _notificationService.MarkAsReadAsync(id, userId.Value);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                await _notificationService.MarkAllAsReadAsync(userId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("mark-all-read")]
        public async Task<ActionResult> MarkAllAsReadPatch()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                await _notificationService.MarkAllAsReadAsync(userId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateNotificationStatus(int id, NotificationStatus Status)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var success = await _notificationService.UpdateNotificationStatusAsync(id, userId.Value, Status);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var success = await _notificationService.DeleteNotificationAsync(id, userId.Value);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<NotificationStatsDto>> GetStats()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var stats = await _notificationService.GetNotificationStatsAsync(userId.Value);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("send-all")]
        public async Task<ActionResult> SendRealTimeNotificationToAll([FromBody] CreateNotificationDto notification)
        {
            try
            {
                await _notificationService.SendRealTimeNotificationToAllAsync(notification);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("send-realtime")]
        public async Task<ActionResult> SendRealTimeNotification([FromBody] CreateNotificationDto notification)
        {
            try
            {
                await _notificationService.SendRealTimeNotificationAsync(notification.UserId, notification);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private int? GetUserIdFromClaims()
        {
            // Look for the user ID in different claim types
            var userIdClaim = User.FindFirst("UserId")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("nameid")?.Value;
                
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
} 