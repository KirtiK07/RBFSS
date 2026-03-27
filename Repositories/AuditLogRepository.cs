using Microsoft.EntityFrameworkCore;
using RBFSS.Data;
using RBFSS.Models;
using RBFSS.Repositories.Interfaces;

namespace RBFSS.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserIdAsync(string userId, int page = 1, int pageSize = 50)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.File)
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsByFileIdAsync(int fileId, int page = 1, int pageSize = 50)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.File)
                .Where(al => al.FileId == fileId)
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 100)
        {
            return await _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.File)
                .OrderByDescending(al => al.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? action = null, int page = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .Include(al => al.File)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(al => al.Action.Contains(action));

            return await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }
    }
}