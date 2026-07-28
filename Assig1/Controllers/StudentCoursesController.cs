using Assig1.Data;
using Assig1.DTOs;
using Assig1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/student-courses")]
    [Authorize(Roles = "Teacher,Admin")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Get AppDbContext from dependency injection
        public StudentCoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<StudentCourseSearchResultDto>>> SearchStudentCourses(
            string? studentName,
            string? courseName,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Build the filtered query
            IQueryable<StudentCourse> query = _context.StudentCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Course);

            if (!string.IsNullOrWhiteSpace(studentName))
            {
                query = query.Where(sc => sc.Student.Name.Contains(studentName));
            }

            if (!string.IsNullOrWhiteSpace(courseName))
            {
                query = query.Where(sc => sc.Course.Name.Contains(courseName));
            }

            // Count matches before paging
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Sort, then page, then project to the result DTO
            List<StudentCourseSearchResultDto> items = await query
                .OrderBy(sc => sc.Student.Name)
                .ThenBy(sc => sc.Course.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sc => new StudentCourseSearchResultDto
                {
                    StudentId = sc.StudentId,
                    StudentName = sc.Student.Name,
                    CourseId = sc.CourseId,
                    CourseName = sc.Course.Name,
                    CourseStatus = sc.CourseStatus,
                    PassStatus = sc.PassStatus,
                    EnrollmentDate = sc.EnrollmentDate,
                    CompletionDate = sc.CompletionDate
                })
                .ToListAsync();

            return Ok(new PagedResult<StudentCourseSearchResultDto>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}
