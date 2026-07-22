using Assig1.Data;
using Assig1.DTOs;
using Assig1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Get AppDbContext from dependency injection
        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<CourseDto>>> GetAllCourses()
        {
            // Read all courses from the database
            List<CourseDto> courses = await _context.Courses
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Hours = c.Hours
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourseById(int id)
        {
            // Search for one course by its Id
            Course? course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Hours = course.Hours
            });
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> AddCourse(CourseCreateDto courseDto)
        {
            // Map the incoming DTO to a new Course entity
            Course course = new()
            {
                Name = courseDto.Name,
                Hours = courseDto.Hours
            };

            // Add the course to the context
            _context.Courses.Add(course);

            // Save the course in the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = course.Id },
                new CourseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Hours = course.Hours
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(
            int id,
            CourseUpdateDto courseDto)
        {
            // Search for the existing course
            Course? existingCourse =
                await _context.Courses.FindAsync(id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            // Update the course values
            existingCourse.Name = courseDto.Name;
            existingCourse.Hours = courseDto.Hours;

            // Save the changes in the database
            await _context.SaveChangesAsync();

            return Ok(new CourseDto
            {
                Id = existingCourse.Id,
                Name = existingCourse.Name,
                Hours = existingCourse.Hours
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            // Search for the course
            Course? course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            // Remove the course from the context
            _context.Courses.Remove(course);

            // Save the deletion in the database
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{courseId}/students")]
        public async Task<ActionResult<CourseStudentsDto>> GetCourseStudents(int courseId)
        {
            // Find the course with registered students
            Course? course = await _context.Courses
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Return course data with student details
            return Ok(new CourseStudentsDto
            {
                Id = course.Id,
                Name = course.Name,
                Hours = course.Hours,
                Students = course.Students.Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList()
            });
        }

        [HttpGet("{courseId}/teachers")]
        public async Task<ActionResult<CourseTeachersDto>> GetCourseTeachers(int courseId)
        {
            // Find the course with assigned teachers
            Course? course = await _context.Courses
                .Include(c => c.Teachers)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Return course data with teacher details
            return Ok(new CourseTeachersDto
            {
                Id = course.Id,
                Name = course.Name,
                Hours = course.Hours,
                Teachers = course.Teachers.Select(t => new TeacherSummaryDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            });
        }
    }
}