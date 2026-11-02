using API_proj.Data;
using API_proj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace API_Proj.Controllers

//Importing all necessarry assets 

{
    namespace API_proj.Controllers
    {
        [ApiController]
        [Route("api")]
        [Authorize] // every endpoint requires a logged-in user that is registered
        public class CategoriesController : ControllerBase
        {
            private readonly ApplicationDBContext _context;

            public CategoriesController(ApplicationDBContext context)
            {
                _context = context; // Assigning context to a context container 
            }

            private int CurrentUserId =>

              int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            //GET

            ///GET ROLE:  Lists the race categories for an event. Available to both roles
            [HttpGet("events/{id}/categories")]


            //listing the Types of responses on the API Endpoint can produce  
            [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<ActionResult<List<Category>>> GetEventCategories(int id)
            {
                var categories = await _context.Categories.AsNoTracking()
                    .Where(c => c.EventID == id)
                    .ToListAsync();
                //if the category is not found display an appropriate message
                return categories.Count == 0 ? NotFound("Category not found.") : Ok(categories);
            }



            ///POST
            //
            ///Adds a category to an event. Only the Organiser who owns the event can do this.
            [HttpPost("events/{id}/categories")]
            [Authorize(Roles = "Organiser")]
            [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<ActionResult<Category>> CreateCategory(int id, CategoryDto dto)
            {
                var ev = await _context.Events.FindAsync(id);
                if (ev == null)
                    return NotFound("Event not found.");

                if (ev.OrganiserID != CurrentUserId)
                    return Forbid();


                //creating a container to store all the category values that will be stored in the database 
                var category = new Category
                {
                    EventID = id,
                    CategoryType = dto.CategoryType,
                    Description = dto.Description,
                    Name = dto.Name,

                    Distance = dto.Distance,
                    MaxParticipants = dto.MaxParticipants
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created, category);//return an appropriate status code upon success
            }

            //PUT 


            ///PUT ROLE:  Updates a category's details. Only the Organiser who owns the event can update the category details

            [HttpPut("categories/{id}")] //insert the appropriate category id in the API endpoint
            [Authorize(Roles = "Organiser")]//Assign the user a organiser role

            //Summary of the different response status codes 
            [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]

            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]

            //Assigning the data types that will be collected in the API
            public async Task<ActionResult<Category>> UpdateCategory(int id, CategoryDto dto)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound("Category not found.");//return an appropriate message when the categrory is not found 

                var ev = await _context.Events.FindAsync(category.EventID);
                if (ev == null || ev.OrganiserID != CurrentUserId)
                    return Forbid();
                //Container to store the category data 
                category.CategoryType = dto.CategoryType;
                category.Description = dto.Description;
                category.Name = dto.Name;
                category.Distance = dto.Distance;
                category.MaxParticipants = dto.MaxParticipants;

                await _context.SaveChangesAsync();
                return Ok(category);
            }

            ///DELETE ROLE Removes a category. Only the Organiser who owns the event can do this.</summary>
            [HttpDelete("categories/{id}")]
            [Authorize(Roles = "Organiser")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]

            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status409Conflict)]
            public async Task<IActionResult> DeleteCategory(int id)
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    return NotFound("Category not found.");

                var ev = await _context.Events.FindAsync(category.EventID);
                if (ev == null || ev.OrganiserID != CurrentUserId)
                    return Forbid();

                var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CategoryID == id);
                if (hasEnrollments)
                    return Conflict("This category has enrolments and cannot be deleted.");

                // unlink the event if it points at this category
                if (ev.CategoryID == id)
                    ev.CategoryID = null;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}