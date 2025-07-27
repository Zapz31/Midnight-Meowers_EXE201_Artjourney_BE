using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Certificate;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class UserCertificateInfoRepository : IUserCertificateInfoRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public UserCertificateInfoRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<UserCertificateInfo> CreateUserCertificateAsync(UserCertificateInfo userCertificate)
        {
            var createdUserCertificate = await _unitOfWork.GetRepo<UserCertificateInfo>().CreateAsync(userCertificate);
            return createdUserCertificate;
        }

        public async Task<List<UserCertificateDTO>> GetUserCertificatesByUserIdAsync(long userId)
        {
            var sql = @"
                SELECT 
                    uci.id as Id,
                    uci.user_id as UserId,
                    uci.certificate_id as CertificateId,
                    c.image_url as CertificateImageUrl,
                    c.course_id as CourseId,
                    co.title as CourseName,
                    uci.completed_at as CompletedAt,
                    uci.completed_in as CompletedIn
                FROM user_certificate_infos uci
                INNER JOIN certificates c ON uci.certificate_id = c.certificate_id
                INNER JOIN courses co ON c.course_id = co.course_id
                INNER JOIN user_course_infos uco ON uco.user_id = uci.user_id AND uco.course_id = c.course_id
                WHERE uci.user_id = @userId 
                  AND c.is_active = true 
                  AND uco.learning_status = 'Completed'
                ORDER BY uci.completed_at DESC";

            var results = new List<UserCertificateDTO>();

            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@userId";
                    parameter.Value = userId;
                    command.Parameters.Add(parameter);

                    await _context.Database.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new UserCertificateDTO
                            {
                                Id = reader.GetInt64(0), // Id
                                UserId = reader.GetInt64(1), // UserId
                                CertificateId = reader.GetInt64(2), // CertificateId
                                CertificateImageUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // CertificateImageUrl
                                CourseId = reader.GetInt64(4), // CourseId
                                CourseName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), // CourseName
                                CompletedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6), // CompletedAt
                                CompletedIn = reader.IsDBNull(7) ? null : (TimeSpan)reader.GetValue(7) // CompletedIn (stored as interval)
                            });
                        }
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return results;
        }

        public async Task<List<UserCertificateDTO>> GetAllUserCertificatesAsync()
        {
            var sql = @"
                SELECT 
                    uci.id as Id,
                    uci.user_id as UserId,
                    uci.certificate_id as CertificateId,
                    c.image_url as CertificateImageUrl,
                    c.course_id as CourseId,
                    co.title as CourseName,
                    uci.completed_at as CompletedAt,
                    uci.completed_in as CompletedIn
                FROM user_certificate_infos uci
                INNER JOIN certificates c ON uci.certificate_id = c.certificate_id
                INNER JOIN courses co ON c.course_id = co.course_id
                INNER JOIN user_course_infos uco ON uco.user_id = uci.user_id AND uco.course_id = c.course_id
                WHERE c.is_active = true 
                  AND uco.learning_status = 'Completed'
                ORDER BY uci.completed_at DESC";

            var results = new List<UserCertificateDTO>();

            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;

                    await _context.Database.OpenConnectionAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new UserCertificateDTO
                            {
                                Id = reader.GetInt64(0), // Id
                                UserId = reader.GetInt64(1), // UserId
                                CertificateId = reader.GetInt64(2), // CertificateId
                                CertificateImageUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // CertificateImageUrl
                                CourseId = reader.GetInt64(4), // CourseId
                                CourseName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), // CourseName
                                CompletedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6), // CompletedAt
                                CompletedIn = reader.IsDBNull(7) ? null : (TimeSpan)reader.GetValue(7) // CompletedIn (stored as interval)
                            });
                        }
                    }
                }
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return results;
        }

        public async Task<UserCertificateInfo?> GetUserCertificateByUserIdAndCourseIdAsync(long userId, long courseId)
        {
            var sql = @"
                SELECT uci.*
                FROM user_certificate_infos uci
                INNER JOIN certificates c ON uci.certificate_id = c.certificate_id
                WHERE uci.user_id = {0} AND c.course_id = {1} AND c.is_active = true";

            var result = await _context.UserCertificateInfos
                .FromSqlRaw(sql, userId, courseId)
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<UserCertificateDTO?> GetCertificateDetailsByIdAsync(long userCertificateId)
        {
            var sql = @"
                SELECT 
                    uci.id as Id,
                    uci.user_id as UserId,
                    uci.certificate_id as CertificateId,
                    c.image_url as CertificateImageUrl,
                    c.course_id as CourseId,
                    co.title as CourseName,
                    uci.completed_at as CompletedAt,
                    uci.completed_in as CompletedIn
                FROM user_certificate_infos uci
                INNER JOIN certificates c ON uci.certificate_id = c.certificate_id
                INNER JOIN courses co ON c.course_id = co.course_id
                INNER JOIN user_course_infos uco ON uco.user_id = uci.user_id AND uco.course_id = c.course_id
                WHERE uci.id = @userCertificateId 
                  AND c.is_active = true 
                  AND uco.learning_status = 'Completed'";

            try
            {
                var connection = _context.Database.GetDbConnection();
                var wasConnectionClosed = connection.State == System.Data.ConnectionState.Closed;
                
                if (wasConnectionClosed)
                {
                    await _context.Database.OpenConnectionAsync();
                }

                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@userCertificateId";
                        parameter.Value = userCertificateId;
                        command.Parameters.Add(parameter);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new UserCertificateDTO
                                {
                                    Id = reader.GetInt64(0), // Id
                                    UserId = reader.GetInt64(1), // UserId
                                    CertificateId = reader.GetInt64(2), // CertificateId
                                    CertificateImageUrl = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // CertificateImageUrl
                                    CourseId = reader.GetInt64(4), // CourseId
                                    CourseName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), // CourseName
                                    CompletedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6), // CompletedAt
                                    CompletedIn = reader.IsDBNull(7) ? null : (TimeSpan)reader.GetValue(7) // CompletedIn as TimeSpan from interval
                                };
                            }
                        }
                    }
                }
                finally
                {
                    if (wasConnectionClosed)
                    {
                        await _context.Database.CloseConnectionAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Let the exception bubble up to be handled by the service layer
                throw;
            }

            return null;
        }

        public async Task<bool> DeleteUserCertificateAsync(long userId, long certificateId)
        {
            var queryOption = new QueryBuilder<UserCertificateInfo>()
                .WithPredicate(uci => uci.UserId == userId && uci.CertificateId == certificateId)
                .Build();
            
            var userCertificates = await _unitOfWork.GetRepo<UserCertificateInfo>().GetAllAsync(queryOption);
            var userCertificate = userCertificates.FirstOrDefault();
            
            if (userCertificate == null)
                return false;

            await _unitOfWork.GetRepo<UserCertificateInfo>().DeleteAsync(userCertificate);
            return true;
        }
    }
}
