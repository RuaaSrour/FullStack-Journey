using Assig1.Data;
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
        public async Task<ActionResult<List<Course>>> GetAllCourses()
        {
            // Read all courses from the database
            List<Course> courses = await _context.Courses.ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourseById(int id)
        {
            // Search for one course by its Id
            Course? course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<Course>> AddCourse(Course course)
        {
            // Add the course to the context
            _context.Courses.Add(course);

            // Save the course in the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCourseById),
                new { id = course.Id },
                course
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Course>> UpdateCourse(
            int id,
            Course updatedCourse)
        {
            // Search for the existing course
            Course? existingCourse =
                await _context.Courses.FindAsync(id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            // Update the course values
            existingCourse.Name = updatedCourse.Name;
            existingCourse.Hours = updatedCourse.Hours;

            // Save the changes in the database
            await _context.SaveChangesAsync();

            return Ok(existingCourse);
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
        public async Task<ActionResult> GetCourseStudents(int courseId)
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
            return Ok(new
            {
                course.Id,
                course.Name,
                course.Hours,
                Students = course.Students.Select(s => new
                {
                    s.Id,
                    s.Name
                })
            });
        }

        [HttpGet("{courseId}/teachers")]
        public async Task<ActionResult> GetCourseTeachers(int courseId)
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
            return Ok(new
            {
                course.Id,
                course.Name,
                course.Hours,
                Teachers = course.Teachers.Select(t => new
                {
                    t.Id,
                    t.Name
                })
            });
        }
    }
}