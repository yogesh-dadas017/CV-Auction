﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CV_Auction.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CV_Auction_New.Models;

namespace CV_Auction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CvauctionContext _context;

        public UsersController(CvauctionContext context)
        {
            _context = context;
        }


        // Method to get list of all the avaliable users
        // GET: api/Users
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }

            return await _context.Users.ToListAsync();
        }


        [HttpGet("{Uemail}")]
        public async Task<IActionResult> ForgotPassword(string Uemail)
        {
            if (_context.Users == null)
            {
                return NotFound();
            } // Missing closing brace here

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uemail == Uemail);
            if (user != null)
            {
                Random random = new Random();
                int otp = random.Next(100000, 1000000);

                // Send OTP to the user's email
                EmailService emailService = new EmailServiceImpl();
                emailService.SendEmail(user.Uemail, "Password Recovery", "Your OTP is: " + otp);

                // Return a JSON result containing the Uemail and OTP
                var result = new
                {
                    Uemail = user.Uemail,
                    OTP = otp
                };

                // Use JsonResult to return the result as JSON
                return new JsonResult(result);
            }

            return NotFound(); // In case user doesn't exist
        }


        [HttpPut("{Uemail}")]
        public async Task<IActionResult> UpdatePassword(string Uemail, User u)
        {
            if (_context.Users == null)
            {
                return NotFound();
            } // Missing closing brace here

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uemail == Uemail);

            if (user != null)
            {
                user.Upwd = u.Upwd;
                await _context.SaveChangesAsync();
                return Ok(user);
            }

            return NotFound();
        }



        // GET: api/Users/String?Upwd
        [HttpPost("{Uemail}")]
        // This HTTP GET method retrieves a user based on the provided Uemail
        public async Task<ActionResult<User>> LoginUser(string Uemail, [FromQuery] string Upwd)
        {
            // Check if the Users DbSet is null, which means the context is not properly initialized
            if (_context.Users == null)
            {
                // Return a NotFound response if the Users DbSet is null
                return NotFound();
            }

            // Use FirstOrDefaultAsync to find the user with the specified Uemail in the database
            // FirstOrDefaultAsync returns the first matching record or null if no match is found
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uemail == Uemail);

            // If the user is not found, return a NotFound response
            if (user != null)
            {
                if (Upwd.Equals(user.Upwd))
                    return Ok(user);
            }

            var admin = await _context.Admins.FirstOrDefaultAsync(u => u.Aemail == Uemail);
            if (admin != null)
            {
                if (Upwd.Equals(admin.Apwd))
                    return Ok(admin);
            }

            // Return the user object if found, wrapped in an Ok response (indicating success)
            return NotFound();
        }


        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Uid)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // This endpoint registers a new user by adding them to the database.
        [HttpPost]
        public async Task<ActionResult<User>> RegisterUser(User user)
        {
            // Check if the Users DbSet is null, meaning the context is not properly initialized
            if (_context.Users == null)
            {
                // Return a Problem response with an error message if Users DbSet is null
                return Problem("Entity set 'cvauctionContext.Users' is null.");
            }

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private User? GetUserByEmail(string Uemail)
        {
            return _context.Users.FirstOrDefault(u => u.Uemail == Uemail);
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Uid == id)).GetValueOrDefault();
        }
    }
}
