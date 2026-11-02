using API_proj.Data;
using API_proj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_proj.Controllers
{
    [ApiController]

       [Route("api/[controller]")]


    [Authorize] // every endpoint requires a logged-in user


    public class EventsController : ControllerBase
    {
           private readonly ApplicationDBContext _context;

            public EventsController(ApplicationDBContext context)
        {
               _context = context;


        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Returns all events. Available to Organisers and Participants.</summary>
        [HttpGet]
           [ProducesResponseType(typeof(List<Event>), StatusCodes.Status200OK)]
              [ProducesResponseType(StatusCodes.Status401Unauthorized)]

           [ProducesResponseType(StatusCodes.Status404NotFound)]


        public async Task<ActionResult<List<Event>>> GetEvents()
        {
            var events = await _context.Events.AsNoTracking().ToListAsync();
               return events.Count == 0 ? NotFound("No events found.") : Ok(events);
        }

        /// <summary>Creates an event owned by the logged-in Organiser.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]

          [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
           [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Event>> CreateEvent(EventDto dto)
        {
            var ev = new Event
            {
                OrganiserID = CurrentUserId, // taken from the session, never from the client
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                TargetAudience = dto.TargetAudience
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return Ok(ev);
        }

        /// <summary>Updates an event. Only the Organiser who created it can do this.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(Event), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Event>> UpdateEvent(int id, EventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound("Event not found.");

            if (ev.OrganiserID != CurrentUserId)
                return Forbid();

            ev.Description = dto.Description;
              ev.EventDate = dto.EventDate;
               ev.Location = dto.Location;

            ev.TargetAudience = dto.TargetAudience;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        /// Deletes an event. Only the Organiser who created it can do this
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

          [ProducesResponseType(StatusCodes.Status401Unauthorized)]

             [ProducesResponseType(StatusCodes.Status403Forbidden)]
              
            [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)

              return NotFound("Event not found.");

            if (ev.OrganiserID != CurrentUserId)
                  return Forbid();

            // The database uses NoAction deletes, so block deletion while related rows exist.
            var hasCategories = await _context.Categories.AnyAsync(c => c.EventID == id);

                var hasPayments = await _context.Payments.AnyAsync(p => p.EventID == id);
                if (hasCategories || hasPayments)

                return Conflict("Remove this event's categories and payments before deleting it.");

            _context.Events.Remove(ev);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}