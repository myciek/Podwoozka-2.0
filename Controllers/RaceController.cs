using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Podwoozka.Models;

namespace RaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        private readonly RaceContext _context;

        public RaceController(RaceContext context)
        {
            _context = context;

            if (_context.RaceItems.Count() == 0)
            {
                // Create a new RaceItem if collection is empty,
                // which means you can't delete all RaceItems.
                _context.RaceItems.Add(new RaceItem { Name = "Item1" });
                _context.SaveChanges();
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


    }
}