using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Podwoozka.Models;
using Microsoft.AspNetCore.Authorization;

namespace RaceApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        private readonly RaceContext _context;
        private readonly IAuthorizationService _authorizationService;

        public RaceController(RaceContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;


            if (_context.RaceItems.Count() == 0)
            {
                // Create a new RaceItem if collection is empty,
                // which means you can't delete all RaceItems.
                // _context.RaceItems.Add(new RaceItem { Name = "Item1" });
                // _context.SaveChanges();

            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RaceItem>>> GetRaceItems()
        {
            return await _context.RaceItems.ToListAsync();

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RaceItem>> GetRaceItem(long id)
        {
            var raceItem = await _context.RaceItems.FindAsync(id);
            if (raceItem == null)
            {
                return NotFound();

            }
            return raceItem;
        }
        [HttpPost]
        public async Task<ActionResult<RaceItem>> PostRaceItem(RaceItem item)
        {
            _context.RaceItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRaceItem), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRaceItem(long id, RaceItem item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }
            var authorizationResult = await _authorizationService
           .AuthorizeAsync(User, item, "IsOwnerPolicy");

            if (authorizationResult.Succeeded)
            {
                _context.Entry(item).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            else if (User.Identity.IsAuthenticated)
            {
                return new ForbidResult();
            }
            else
            {
                return new ChallengeResult();
            }


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRaceItem(long id)
        {
            var raceItem = await _context.RaceItems.FindAsync(id);

            if (raceItem == null)
            {
                return NotFound();
            }

            _context.RaceItems.Remove(raceItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }




    }
}