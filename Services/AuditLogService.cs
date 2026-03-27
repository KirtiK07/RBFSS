using RBFSS.Models;
using RBFSS.Repositories.Interfaces;
using RBFSS.Services.Interfaces;

namespace RBFSS.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogActionAsync(string userId, string action, string resource, string? ipAddress = null, string? details = null, int? fileId = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Resource = resource,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress ?? "",
                Details = details ?? "",
                FileId = fileId
            };

            await _auditLogRepository.CreateAuditLogAsync(auditLog);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(string userId, int page = 1, int pageSize = 50)
        {
            return await _auditLogRepository.GetAuditLogsByUserIdAsync(userId, page, pageSize);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByFileIdAsync(int fileId, int page = 1, int pageSize = 50)
        {
            return await _auditLogRepository.GetAuditLogsByFileIdAsync(fileId, page, pageSize);
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100)
        {
            return await _auditLogRepository.GetRecentAuditLogsAsync(count);
        }

        public async Task<IEnumerable<AuditLog>> SearchAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null, int page = 1, int pageSize = 50)
        {
            return await _auditLogRepository.GetAuditLogsAsync(fromDate, toDate, action, page, pageSize);
        }

        public async Task<Dictionary<string, int>> GetAuditStatisticsAsync()
        {
            var totalLogs = await _auditLogRepository.GetTotalAuditLogsCountAsync();
            var recentLogs = await _auditLogRepository.GetRecentAuditLogsAsync(100);

            var actionCounts = recentLogs
                .GroupBy(log => log.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new Dictionary<string, int>
            {
                { "TotalLogs", totalLogs }
            };

            foreach (var actionCount in actionCounts)
            {
                result[actionCount.Key] = actionCount.Value;
            }

            return result;
        }
    }
}