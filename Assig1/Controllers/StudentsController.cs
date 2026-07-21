using Assig1.Data;
using Assig1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Get AppDbContext from dependency injection
        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Student>>> GetAllStudents()
        {
            // Read all students from the database
            List<Student> students =
                await _context.Students.ToListAsync();

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudentById(int id)
        {
            // Search for one student by Id
            Student? student =
                await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> AddStudent(Student student)
        {
            // Add the student to the context
            _context.Students.Add(student);

            // Save the student in the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = student.Id },
                student
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Student>> UpdateStudent(
            int id,
            Student updatedStudent)
        {
            // Search for the existing student
            Student? existingStudent =
                await _context.Students.FindAsync(id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            // Update the student name
            existingStudent.Name = updatedStudent.Name;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok(existingStudent);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            // Search for the student
            Student? student =
                await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            // Remove the student
            _context.Students.Remove(student);

            // Save the deletion
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("{studentId}/courses/{courseId}")]
        public async Task<ActionResult> AddCourseToStudent(
    int studentId,
    int courseId)
        {
            // Find the student with current courses
            Student? student = await _context.Students
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Find the course
            Course? course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Check if the student already has this course
            bool alreadyRegistered =
                student.Courses.Any(c => c.Id == courseId);

            if (alreadyRegistered)
            {
                return BadRequest("Student is already registered in this course.");
            }

            // Add the course to the student
            student.Courses.Add(course);

            // Save the relation in the database
            await _context.SaveChangesAsync();

            return Ok("Course added to student successfully.");
        }

        [HttpDelete("{studentId}/courses/{courseId}")]
        public async Task<ActionResult> RemoveCourseFromStudent(
    int studentId,
    int courseId)
        {
            // Find the student with current courses
            Student? student = await _context.Students
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Find the course inside the student's courses
            Course? course = student.Courses
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Student is not registered in this course.");
            }

            // Remove the course from the student
            student.Courses.Remove(course);

            // Save the relation change
            await _context.SaveChangesAsync();

            return Ok("Course removed from student successfully.");
        }
        [HttpGet("{studentId}/courses")]
        public async Task<ActionResult> GetStudentCourses(int studentId)
        {
            // Find the student with registered courses
            Student? student = await _context.Students
                .Include(s => s.Courses)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Return student data with course details
            return Ok(new
            {
                student.Id,
                student.Name,
                Courses = student.Courses.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Hours
                })
            });
        }



    }
}