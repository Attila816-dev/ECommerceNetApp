using System.IdentityModel.Tokens.Jwt;
using Asp.Versioning;
using ECommerceNetApp.Api.Authorization;
using ECommerceNetApp.Api.Model;
using ECommerceNetApp.Api.Services;
using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceNetApp.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : BaseApiController
    {
        public AuthController(IDispatcher dispatcher, IHateoasLinkService linkService)
            : base(linkService, dispatcher)
        {
        }

        [HttpPost("register")]
        [RequirePermission(Permissions.Create, Resources.User)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var userId = await Dispatcher.SendCommandAsync<RegisterUserCommand, int>(command, cancellationToken).ConfigureAwait(false);

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

        [HttpPost("register/customer")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                UserRole.Customer);
            var userId = await Dispatcher.SendCommandAsync<RegisterUserCommand, int>(command, cancellationToken).ConfigureAwait(false);

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginUserCommandResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var result = await Dispatcher.SendCommandAsync<LoginUserCommand, LoginUserCommandResponse>(command, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return Unauthorized(new { result.Message });
            }

            return Ok(result);
        }

        [HttpPost("refreshtoken")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshTokenCommandResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));
            var result = await Dispatcher.SendCommandAsync<RefreshTokenCommand, RefreshTokenCommandResponse>(command, cancellationToken).ConfigureAwait(false);

            if (!result.Success)
            {
                return Unauthorized(new { result.Message });
            }

            return Ok(result);
        }

        [HttpGet("{email}")]
        [RequirePermission(Permissions.Read, Resources.User)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LinkedResourceDto<UserDto>>> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await Dispatcher.SendQueryAsync<GetUserQuery, UserDto?>(new GetUserQuery(email), cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(CreateResource(user));
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await Dispatcher.SendQueryAsync<GetUserQuery, UserDto?>(new GetUserQuery(email), cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(CreateResource(user));
        }
    }
}