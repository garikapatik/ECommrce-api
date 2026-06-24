using GroceryEcommerceApi.Data;
using GroceryEcommerceApi.DTOs;
using GroceryEcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GroceryEcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UserInfoDto>> CreateUser(CreateUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "Email and password are required." });
        }

        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return Conflict(new { Message = "Email already exists." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUserInfo), new { id = user.Id }, response);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfo(int id)
    {
        var authUserId = GetAuthUserId();
        // if (authUserId != id)
        // {
        //     return Forbid();
        // }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt
        });
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserInfoDto>> UpdateUser(int id, UpdateUserRequestDto request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId != id)
        {
            return Forbid();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound(new { Message = "User not found." });
        }

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone.Trim();
        user.Address = request.Address.Trim();

        await _context.SaveChangesAsync();

        return Ok(new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            CreatedAt = user.CreatedAt
        });
    }

    private int GetAuthUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }
}
