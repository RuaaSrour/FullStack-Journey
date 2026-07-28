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
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Get AppDbContext from dependency injection
        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<StudentDto>>> GetAllStudents()
        {
            // Read all students from the database
            List<StudentDto> students = await _context.Students
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudentById(int id)
        {
            // Search for one student by Id
            Student? student =
                await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(new StudentDto
            {
                Id = student.Id,
                Name = student.Name
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentDto>> AddStudent(StudentCreateDto studentDto)
        {
            // Map the incoming DTO to a new Student entity
            Student student = new()
            {
                Name = studentDto.Name
            };

            // Add the student to the context
            _context.Students.Add(student);

            // Save the student in the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetStudentById),
                new { id = student.Id },
                new StudentDto
                {
                    Id = student.Id,
                    Name = student.Name
                }
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentDto>> UpdateStudent(
            int id,
            StudentUpdateDto studentDto)
        {
            // Search for the existing student
            Student? existingStudent =
                await _context.Students.FindAsync(id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            // Update the student name
            existingStudent.Name = studentDto.Name;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok(new StudentDto
            {
                Id = existingStudent.Id,
                Name = existingStudent.Name
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
        // Student-course relationship endpoints
        [HttpPost("{studentId}/courses/{courseId}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult> AddCourseToStudent(
    int studentId,
    int courseId)
        {
            // Confirm the student exists
            bool studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);

            if (!studentExists)
            {
                return NotFound("Student not found.");
            }

            // Confirm the course exists
            bool courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);

            if (!courseExists)
            {
                return NotFound("Course not found.");
            }

            // Check if the student already has this course
            bool alreadyRegistered = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (alreadyRegistered)
            {
                return BadRequest("Student is already registered in this course.");
            }

            // Get the prerequisite courses for this course
            List<CoursePrerequisite> prerequisites = await _context.CoursePrerequisites
                .Where(cp => cp.CourseId == courseId)
                .Include(cp => cp.PrerequisiteCourse)
                .ToListAsync();

            if (prerequisites.Count > 0)
            {
                // Get the course ids the student has completed and passed
                List<int> completedCourseIds = await _context.StudentCourses
                    .Where(sc => sc.StudentId == studentId
                        && sc.CourseStatus == CourseStatus.Completed
                        && sc.PassStatus == PassStatus.Passed)
                    .Select(sc => sc.CourseId)
                    .ToListAsync();

                // Find prerequisite courses not yet completed and passed
                List<string> missingPrerequisites = prerequisites
                    .Where(p => !completedCourseIds.Contains(p.PrerequisiteCourseId))
                    .Select(p => p.PrerequisiteCourse.Name)
                    .ToList();

                if (missingPrerequisites.Count > 0)
                {
                    return BadRequest(new
                    {
                        message = "The student is missing required prerequisites.",
                        missingPrerequisites
                    });
                }
            }

            // Create the enrollment record
            StudentCourse studentCourse = new()
            {
                StudentId = studentId,
                CourseId = courseId
            };

            _context.StudentCourses.Add(studentCourse);

            // Save the relation in the database
            await _context.SaveChangesAsync();

            return Ok("Course added to student successfully.");
        }

        [HttpDelete("{studentId}/courses/{courseId}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult> RemoveCourseFromStudent(
    int studentId,
    int courseId)
        {
            // Find the enrollment record
            StudentCourse? studentCourse = await _context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (studentCourse == null)
            {
                return NotFound("Student is not registered in this course.");
            }

            // Remove the enrollment record
            _context.StudentCourses.Remove(studentCourse);

            // Save the relation change
            await _context.SaveChangesAsync();

            return Ok("Course removed from student successfully.");
        }

        [HttpPut("{studentId}/courses/{courseId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult> UpdateStudentCourse(
            int studentId,
            int courseId,
            StudentCourseUpdateDto updateDto)
        {
            // Find the enrollment record
            StudentCourse? studentCourse = await _context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (studentCourse == null)
            {
                return NotFound("Student is not registered in this course.");
            }

            // A course cannot be marked as passed unless it is completed
            if (updateDto.PassStatus == PassStatus.Passed
                && updateDto.CourseStatus != CourseStatus.Completed)
            {
                return BadRequest("A course cannot be marked as passed unless it is completed.");
            }

            // Update the status fields
            studentCourse.CourseStatus = updateDto.CourseStatus;
            studentCourse.PassStatus = updateDto.PassStatus;
            studentCourse.CompletionDate = updateDto.CompletionDate;

            await _context.SaveChangesAsync();

            return Ok("Student course updated successfully.");
        }

        [HttpGet("{studentId}/courses")]
        public async Task<ActionResult<StudentCoursesDto>> GetStudentCourses(int studentId)
        {
            // Find the student with registered courses
            Student? student = await _context.Students
                .Include(s => s.StudentCourses)
                    .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Return student data with course details
            return Ok(new StudentCoursesDto
            {
                Id = student.Id,
                Name = student.Name,
                Courses = student.StudentCourses.Select(sc => new CourseSummaryDto
                {
                    Id = sc.Course.Id,
                    Name = sc.Course.Name,
                    Hours = sc.Course.Hours
                }).ToList()
            });
        }



    }
}