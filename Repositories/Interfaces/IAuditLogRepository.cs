using RBFSS.Models;

namespace RBFSS.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(string userId, int page = 1, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetAuditLogsByFileIdAsync(int fileId, int page = 1, int pageSize = 50);
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null, int page = 1, int pageSize = 50);
        Task<int> GetTotalAuditLogsCountAsync();
    }
}