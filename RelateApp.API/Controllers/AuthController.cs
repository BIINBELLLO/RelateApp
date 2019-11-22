using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RelateApp.API.Data;
using RelateApp.API.DTOs;
using RelateApp.API.Models;

namespace RelateApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController (IAuthRepository repo, IConfiguration config) {
            _config = config;
            _repo = repo;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserForRegisterDTO userForRegisterDTO) {
            // This ensures that every username starts with a lowercase letter.
            userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower ();

            if (await _repo.UserExists (userForRegisterDTO.Username))
                return BadRequest ("Username already exists");

            var userToCreate = new User {
                Username = userForRegisterDTO.Username
            };

            var createdUser = await _repo.Register (userToCreate, userForRegisterDTO.Password);

            return StatusCode (201);

        }

        [HttpPost ("Login")]
        public async Task<IActionResult> Login (UserLoginDTO userLoginDTO) 
        {
            var userFromRepo = await _repo.Login(userLoginDTO.Username.ToLower(), userLoginDTO.Password);

            if(userFromRepo == null) 
                return Unauthorized();

            // Create token and send back to the user.
            // Create the claims here, which is basically 
            // the information you want to attach to the payload section of the token
            var claims = new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            // Create the key that will be used to sign the credentials

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            // Create the credentials 

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create the token descriptor

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = cred
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok( new 
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}