using StudentManagerWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagerWebApp.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

            await context.Database.EnsureCreatedAsync();
            await EnsureSchemaCompatibleAsync(context);
            await BackfillExistingStudentsAsync(context, passwordService);

            if (context.AppUsers.Any())
            {
                return;
            }

            context.AppUsers.Add(new AppUser
            {
                Email = "admin@studentmanager.local",
                FullName = "System Admin",
                Role = UserRole.Admin,
                PasswordHash = passwordService.HashPassword("Admin123!")
            });

            await context.SaveChangesAsync();
        }

        private static async Task EnsureSchemaCompatibleAsync(StudentDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS AppUsers (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    Email VARCHAR(120) NOT NULL,
                    FullName VARCHAR(120) NOT NULL,
                    PasswordHash LONGTEXT NOT NULL,
                    Role INT NOT NULL,
                    IsActive TINYINT(1) NOT NULL DEFAULT 1,
                    UNIQUE KEY IX_AppUsers_Email (Email)
                );
                """);

            await AddColumnIfMissingAsync(context, "Students", "AppUserId", "INT NULL");
            await AddColumnIfMissingAsync(context, "Courses", "TeacherId", "INT NULL");
            await AddColumnIfMissingAsync(context, "Enrollments", "Grade", "DECIMAL(65,30) NULL");

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS Teachers (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    AppUserId INT NOT NULL,
                    FirstName VARCHAR(50) NOT NULL,
                    LastName VARCHAR(50) NOT NULL,
                    Department VARCHAR(100) NOT NULL DEFAULT '',
                    UNIQUE KEY IX_Teachers_AppUserId (AppUserId)
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS Assignments (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    CourseId INT NOT NULL,
                    Title VARCHAR(120) NOT NULL,
                    Description VARCHAR(1000) NOT NULL,
                    DueDate DATETIME(6) NOT NULL,
                    KEY IX_Assignments_CourseId (CourseId)
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS AssignmentSubmissions (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    AssignmentId INT NOT NULL,
                    StudentId INT NOT NULL,
                    Content VARCHAR(2000) NOT NULL,
                    SubmittedAt DATETIME(6) NOT NULL,
                    Grade DECIMAL(65,30) NULL,
                    Feedback VARCHAR(1000) NOT NULL,
                    UNIQUE KEY IX_AssignmentSubmissions_AssignmentId_StudentId (AssignmentId, StudentId),
                    KEY IX_AssignmentSubmissions_StudentId (StudentId)
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS Absences (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    StudentId INT NOT NULL,
                    CourseId INT NOT NULL,
                    Date DATE NOT NULL,
                    Notes VARCHAR(300) NOT NULL,
                    KEY IX_Absences_CourseId (CourseId),
                    KEY IX_Absences_StudentId (StudentId)
                );
                """);

            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    SenderUserId INT NOT NULL,
                    RecipientUserId INT NOT NULL,
                    Subject VARCHAR(120) NOT NULL,
                    Body VARCHAR(2000) NOT NULL,
                    SentAt DATETIME(6) NOT NULL,
                    IsRead TINYINT(1) NOT NULL DEFAULT 0,
                    KEY IX_Messages_SenderUserId (SenderUserId),
                    KEY IX_Messages_RecipientUserId (RecipientUserId)
                );
                """);
        }

        private static async Task AddColumnIfMissingAsync(StudentDbContext context, string table, string column, string definition)
        {
#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                SET @column_exists = (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = '{table}'
                      AND COLUMN_NAME = '{column}'
                );
                SET @statement = IF(@column_exists = 0, 'ALTER TABLE {table} ADD COLUMN {column} {definition}', 'SELECT 1');
                PREPARE prepared_statement FROM @statement;
                EXECUTE prepared_statement;
                DEALLOCATE PREPARE prepared_statement;
                """);
#pragma warning restore EF1002
        }

        private static async Task BackfillExistingStudentsAsync(StudentDbContext context, PasswordService passwordService)
        {
            var studentsWithoutUsers = await context.Students
                .Where(s => s.AppUserId == null || s.AppUser == null)
                .ToListAsync();

            foreach (var student in studentsWithoutUsers)
            {
                var email = $"student-{student.Id}@studentmanager.local";
                var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    user = new AppUser
                    {
                        Email = email,
                        FullName = $"{student.FirstName} {student.LastName}",
                        Role = UserRole.Student,
                        PasswordHash = passwordService.HashPassword("Student123!")
                    };
                    context.AppUsers.Add(user);
                    await context.SaveChangesAsync();
                }

                student.AppUserId = user.Id;
            }

            await context.SaveChangesAsync();
        }
    }
}
