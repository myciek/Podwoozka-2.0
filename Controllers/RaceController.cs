using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Podwoozka.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Podwoozka.Services;
using Podwoozka.Dtos;
using Podwoozka.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Podwoozka.Models;


namespace RaceApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class RaceController : ControllerBase
    {
        private readonly RaceContext _context;
        private readonly IAuthorizationService _authorizationService;

        public RaceController(RaceContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
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
            //item.Owner = User.Identity.Name;
            item.FreeSeats = item.Seats;
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

                if (item.Participants == null)
                {
                    item.Participants = new string[item.Seats];
                }
                if (item.Participants.Contains(User.Identity.Name))
                {
                    return Content("Jestes juz zapisany do tego przejazdu.");
                }
                if (item.FreeSeats > 0)
                {
                    item.Participants[item.Seats - item.FreeSeats] = User.Identity.Name;
                    item.FreeSeats--;
                    _context.Entry(item).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                return Content("Brak wolnych miejsc");
            }
            else
            {
                return new ChallengeResult();
            }


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRaceItem(long id)
        {
            var item = await _context.RaceItems.FindAsync(id);

            if (id != item.Id)
            {
                return BadRequest();
            }
            var authorizationResult = await _authorizationService
           .AuthorizeAsync(User, item, "IsOwnerPolicy");

            if (authorizationResult.Succeeded)
            {
                _context.RaceItems.Remove(item);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            else if (User.Identity.IsAuthenticated)
            {
                if (item.Participants != null && item.FreeSeats != item.Seats)
                {
                    if (item.Participants.Contains(User.Identity.Name))
                    {
                        item.Participants = item.Participants.Where(val => val != User.Identity.Name).ToArray();
                        item.FreeSeats++;
                        await _context.SaveChangesAsync();
                        return NoContent();

                    }
                    else
                    {
                        return Content("Nie mozna wypisac z przejazdu w ktorym nie jestes.");
                    }
                }
                else
                {
                    return Content("Przejazd jest pusty");
                }
            }
            else
            {
                return new ChallengeResult();
            }
        }




    }
}