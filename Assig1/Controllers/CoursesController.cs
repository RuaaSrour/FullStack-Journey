using Assig1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Assig1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private static List<Course> courses = new List<Course>
        {
            new Course
            {
                Id = 1,
                Name = "Database",
                Hours = 3
            },
            new Course
            {
                Id = 2,
                Name = "Operating Systems",
                Hours = 3
            }
        };

        [HttpGet]
        public ActionResult<List<Course>> GetAllCourses()
        {
            return Ok(courses);
        }
        [HttpPost]
        public ActionResult AddCourse(Course course)
        {
            bool idExists = courses.Any(c => c.Id == course.Id);

            if (idExists)
            {
                return BadRequest("A course with the same Id already exists.");
            }

            courses.Add(course);

            return Ok(course);
        }
        [HttpPut("{id}")]
        public ActionResult UpdateCourse(int id, Course updatedCourse)
        {
            Course? existingCourse =
                courses.FirstOrDefault(c => c.Id == id);

            if (existingCourse == null)
            {
                return NotFound();
            }

            existingCourse.Name = updatedCourse.Name;
            existingCourse.Hours = updatedCourse.Hours;

            return Ok(existingCourse);
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteCourse(int id)
        {
            Course? course = courses.FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            courses.Remove(course);

            return NoContent();
        }
    }
}