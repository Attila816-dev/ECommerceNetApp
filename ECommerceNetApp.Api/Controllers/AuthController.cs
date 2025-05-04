using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : BaseApiController
    {
        public AuthController(IMediator mediator, IHateoasLinkService linkService)
            : base(linkService, mediator)
        {
        }

        [HttpPost("register")]
        [AllowAnonymous] // TODO: only admin
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var userId = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

            var getUserLink = LinkService.CreateLink(
                this,
                nameof(GetUserByEmail),
                values: new { email = command.Email },
                rel: "self");

            var links = new List<LinkDto>
            {
                getUserLink,
            };

            var resource = CreateResource(userId, links);

            return CreatedAtAction(
                nameof(GetUserByEmail),
                new { email = command.Email },
                resource);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return Unauthorized(new { result.Message });
            }

            return Ok(new { result.Token, result.Message });
        }

        [HttpGet("{email}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LinkedResourceDto<UserDto>>> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await Mediator.Send(new GetUserQuery(email), cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(CreateResource(user));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await Mediator.Send(new GetUserQuery(email), cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(CreateResource(user));
        }
    }
}
