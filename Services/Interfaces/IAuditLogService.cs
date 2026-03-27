using RBFSS.Models;

namespace RBFSS.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string userId, string action, string resource, string? ipAddress = null, string? details = null, int? fileId = null);
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(string userId, int page = 1, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetAuditLogsByFileIdAsync(int fileId, int page = 1, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100);
        Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null, int page = 1, int pageSize = 50);
        Task<Dictionary<string, int>> GetAuditStatisticsAsync();
    }
}