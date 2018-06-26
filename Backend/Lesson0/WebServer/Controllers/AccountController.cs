﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using static WebServer.Startup;

namespace WebServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        readonly UserManager<IdentityUser> _userManager;

        public AccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUser(string userId)
        {
           return Ok(User.Identity.Name);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody]IdentityUser user, string password)
        {
            var result =  await this._userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return Ok(await _userManager.FindByNameAsync(user.UserName));
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody]string[] userData)
        {
            var username = userData[0];
            var password = userData[1];
            var identity = await GetIdentity(username, password);

            if (identity == null)
            {
                return BadRequest(new { Error = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // create JWT-token
            var jwt = new JwtSecurityToken(
                    issuer: JWT_ISSUER,
                    audience: JWT_AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(JWT_LIFETIME)),
                    signingCredentials: new SigningCredentials(JwtSymmetricSecurityKey, SecurityAlgorithms.HmacSha256));

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };
            
            return Ok(response);

        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            IdentityUser user = await _userManager.FindByNameAsync(username);

            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, "SimpleUser")
                };

                var claimsIdentity = new ClaimsIdentity
                (
                    claims,
                    "Token",
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType
                );

                return claimsIdentity;
            }

            return null; // No user
        }
    }
}