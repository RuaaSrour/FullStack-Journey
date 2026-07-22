using Assig1.Data;
using Assig1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Get AppDbContext from dependency injection
        public TeachersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Teacher>>> GetAllTeachers()
        {
            // Read all teachers from the database
            List<Teacher> teachers =
                await _context.Teachers.ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetTeacherById(int id)
        {
            // Search for one teacher by Id
            Teacher? teacher =
                await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            return Ok(teacher);
        }

        [HttpPost]
        public async Task<ActionResult<Teacher>> AddTeacher(Teacher teacher)
        {
            // Add the teacher to the context
            _context.Teachers.Add(teacher);

            // Save the teacher in the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTeacherById),
                new { id = teacher.Id },
                teacher
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Teacher>> UpdateTeacher(
            int id,
            Teacher updatedTeacher)
        {
            // Search for the existing teacher
            Teacher? existingTeacher =
                await _context.Teachers.FindAsync(id);

            if (existingTeacher == null)
            {
                return NotFound();
            }

            // Update the teacher name
            existingTeacher.Name = updatedTeacher.Name;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok(existingTeacher);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTeacher(int id)
        {
            // Search for the teacher
            Teacher? teacher =
                await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Remove the teacher
            _context.Teachers.Remove(teacher);

            // Save the deletion
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Teacher-course relationship endpoints
        [HttpPost("{teacherId}/courses/{courseId}")]
        public async Task<ActionResult> AddCourseToTeacher(
    int teacherId,
    int courseId)
        {
            // Find the teacher with current courses
            Teacher? teacher = await _context.Teachers
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            // Find the course
            Course? course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Check if the teacher already has this course
            bool alreadyAssigned =
                teacher.Courses.Any(c => c.Id == courseId);

            if (alreadyAssigned)
            {
                return BadRequest("Teacher is already assigned to this course.");
            }

            // Add the course to the teacher
            teacher.Courses.Add(course);

            // Save the relation in the database
            await _context.SaveChangesAsync();

            return Ok("Course added to teacher successfully.");
        }
        [HttpDelete("{teacherId}/courses/{courseId}")]
        public async Task<ActionResult> RemoveCourseFromTeacher(
        int teacherId,
        int courseId)
        {
            // Find the teacher with current courses
            Teacher? teacher = await _context.Teachers
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            // Find the course inside the teacher's courses
            Course? course = teacher.Courses
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Teacher is not assigned to this course.");
            }

            // Remove the course from the teacher
            teacher.Courses.Remove(course);

            // Save the relation change
            await _context.SaveChangesAsync();

            return Ok("Course removed from teacher successfully.");
        }

        // Get teacher courses
        [HttpGet("{teacherId}/courses")]
        public async Task<ActionResult> GetTeacherCourses(int teacherId)
        {
            // Find the teacher with assigned courses
            Teacher? teacher = await _context.Teachers
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

            // Return teacher data with course details
            return Ok(new
            {
                teacher.Id,
                teacher.Name,
                Courses = teacher.Courses.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Hours
                })
            });
        }
    }
}