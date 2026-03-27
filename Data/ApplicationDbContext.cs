using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RBFSS.Models;

namespace RBFSS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<FilePermission> FilePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Folder> Folders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure UploadedFile relationships
            builder.Entity<UploadedFile>()
                .HasOne(f => f.UploadedBy)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(f => f.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FilePermission relationships
            builder.Entity<FilePermission>()
                .HasOne(fp => fp.File)
                .WithMany(f => f.FilePermissions)
                .HasForeignKey(fp => fp.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FilePermission>()
                .HasOne(fp => fp.User)
                .WithMany(u => u.FilePermissions)
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AuditLog relationships
            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AuditLog>()
                .HasOne(al => al.File)
                .WithMany(f => f.AuditLogs)
                .HasForeignKey(al => al.FileId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Folder relationships
            builder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Folder>()
                .HasOne(f => f.CreatedBy)
                .WithMany(u => u.CreatedFolders)
                .HasForeignKey(f => f.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UploadedFile>()
                .HasOne(f => f.Folder)
                .WithMany(f => f.Files)
                .HasForeignKey(f => f.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes for performance
            builder.Entity<UploadedFile>()
                .HasIndex(f => f.UploadedById);

            builder.Entity<FilePermission>()
                .HasIndex(fp => new { fp.FileId, fp.UserId })
                .IsUnique();

            builder.Entity<AuditLog>()
                .HasIndex(al => al.Timestamp);

            builder.Entity<AuditLog>()
                .HasIndex(al => al.UserId);
        }
    }
}