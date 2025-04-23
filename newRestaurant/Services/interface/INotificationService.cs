// Services/Interfaces/INotificationService.cs
using newRestaurant.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace newRestaurant.Services.Interfaces
{
    public interface INotificationService
    {
        // Get notifications for a specific user
        Task<List<Notification>> GetNotificationsAsync(int userId, bool includeRead = false);

        // Get count of unread notifications
        Task<int> GetUnreadNotificationCountAsync(int userId);

        // Mark a single notification as read
        Task<bool> MarkAsReadAsync(int notificationId);

        // Mark all notifications for a user as read
        Task<bool> MarkAllAsReadAsync(int userId);

        // Delete a notification (optional)
        // Task<bool> DeleteNotificationAsync(int notificationId);

        // Delete all notifications for a user (optional)
        // Task<bool> DeleteAllNotificationsAsync(int userId);

        // Add a notification (useful for testing or system messages)
        Task<bool> AddNotificationAsync(Notification notification);
    }
}