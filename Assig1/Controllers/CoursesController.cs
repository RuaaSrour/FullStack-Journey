using Assig1.Data;
using Assig1.DTOs;
using Assig1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        [Authorize(Roles = "Teacher,Admin")]
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
        [Authorize(Roles = "Teacher,Admin")]
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
        [Authorize(Roles = "Teacher,Admin")]
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
                .Include(c => c.StudentCourses)
                    .ThenInclude(sc => sc.Student)
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
                Students = course.StudentCourses.Select(sc => new StudentSummaryDto
                {
                    Id = sc.Student.Id,
                    Name = sc.Student.Name
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

        // Get all prerequisites for a course
        [HttpGet("{courseId}/prerequisites")]
        public async Task<ActionResult<List<CourseSummaryDto>>> GetCoursePrerequisites(
            int courseId)
        {
            // Check if the course exists
            bool courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId);

            if (!courseExists)
            {
                return NotFound("Course not found.");
            }

            // Get the prerequisite courses
            List<CourseSummaryDto> prerequisites =
                await _context.CoursePrerequisites
                    .Where(cp => cp.CourseId == courseId)
                    .Select(cp => new CourseSummaryDto
                    {
                        Id = cp.PrerequisiteCourse.Id,
                        Name = cp.PrerequisiteCourse.Name,
                        Hours = cp.PrerequisiteCourse.Hours
                    })
                    .ToListAsync();

            return Ok(prerequisites);
        }


        // Add a prerequisite to a course
        [HttpPost("{courseId}/prerequisites/{prerequisiteCourseId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult> AddCoursePrerequisite(
            int courseId,
            int prerequisiteCourseId)
        {
            // A course cannot require itself
            if (courseId == prerequisiteCourseId)
            {
                return BadRequest(
                    "A course cannot be its own prerequisite.");
            }

            // Check if the main course exists
            bool courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId);

            if (!courseExists)
            {
                return NotFound("Course not found.");
            }

            // Check if the prerequisite course exists
            bool prerequisiteExists = await _context.Courses
                .AnyAsync(c => c.Id == prerequisiteCourseId);

            if (!prerequisiteExists)
            {
                return NotFound("Prerequisite course not found.");
            }

            // Check if this prerequisite was already added
            bool alreadyExists = await _context.CoursePrerequisites
                .AnyAsync(cp =>
                    cp.CourseId == courseId &&
                    cp.PrerequisiteCourseId == prerequisiteCourseId);

            if (alreadyExists)
            {
                return BadRequest(
                    "This prerequisite has already been added.");
            }

            // Create the prerequisite relationship
            CoursePrerequisite prerequisite = new()
            {
                CourseId = courseId,
                PrerequisiteCourseId = prerequisiteCourseId
            };

            // Save the new relationship
            _context.CoursePrerequisites.Add(prerequisite);
            await _context.SaveChangesAsync();

            return Ok("Prerequisite added successfully.");
        }


        // Remove a prerequisite from a course
        [HttpDelete("{courseId}/prerequisites/{prerequisiteCourseId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult> RemoveCoursePrerequisite(
            int courseId,
            int prerequisiteCourseId)
        {
            // Find the prerequisite relationship
            CoursePrerequisite? prerequisite =
                await _context.CoursePrerequisites
                    .FirstOrDefaultAsync(cp =>
                        cp.CourseId == courseId &&
                        cp.PrerequisiteCourseId == prerequisiteCourseId);

            // Return an error if the relationship does not exist
            if (prerequisite == null)
            {
                return NotFound(
                    "This prerequisite relationship does not exist.");
            }

            // Remove the relationship
            _context.CoursePrerequisites.Remove(prerequisite);
            await _context.SaveChangesAsync();

            return Ok("Prerequisite removed successfully.");
        }


    }
}