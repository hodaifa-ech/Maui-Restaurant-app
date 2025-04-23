// Services/NotificationService.cs
using Microsoft.EntityFrameworkCore;
using newRestaurant.Data;
using newRestaurant.Models;
using newRestaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace newRestaurant.Services
{
    public class NotificationService : INotificationService
    {
        private readonly RestaurantDbContext _context;

        public NotificationService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            if (notification == null || notification.UserId <= 0 || string.IsNullOrWhiteSpace(notification.Title))
                return false; // Basic validation

            notification.SentDate = DateTime.UtcNow; // Ensure date is set
            notification.IsRead = false; // Ensure starts as unread

            _context.Notifications.Add(notification);
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding notification: {ex.Message}");
                return false;
            }
        }


        public async Task<List<Notification>> GetNotificationsAsync(int userId, bool includeRead = false)
        {
            try
            {
                var query = _context.Notifications
                                    .Where(n => n.UserId == userId);

                if (!includeRead)
                {
                    query = query.Where(n => !n.IsRead);
                }

                return await query.OrderByDescending(n => n.SentDate)
                                  .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting notifications for user {userId}: {ex.Message}");
                return new List<Notification>(); // Return empty list on error
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            try
            {
                return await _context.Notifications
                                     .CountAsync(n => n.UserId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting unread notification count for user {userId}: {ex.Message}");
                return 0; // Return 0 on error
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null || notification.IsRead)
                {
                    return false; // Not found or already read
                }

                notification.IsRead = true;
                _context.Notifications.Update(notification);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking notification {notificationId} as read: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                // Find unread notifications for the user
                var unreadNotifications = await _context.Notifications
                                                        .Where(n => n.UserId == userId && !n.IsRead)
                                                        .ToListAsync();

                if (!unreadNotifications.Any())
                {
                    return true; // Nothing to mark, considered success
                }

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    // EF Core tracks changes, update happens on SaveChangesAsync
                }
                // Use ExecuteUpdate for potentially better performance on large sets (EF Core 7+)
                // return await _context.Notifications
                //                .Where(n => n.UserId == userId && !n.IsRead)
                //                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true)) > 0;

                // For EF Core 6 or simplicity:
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error marking all notifications as read for user {userId}: {ex.Message}");
                return false;
            }
        }
    }
}