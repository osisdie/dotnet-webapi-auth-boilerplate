using System.Net;
using System.Threading.Tasks;
using CoreFX.Abstractions.Bases.Interfaces;
using CoreFX.Auth.Contracts.RefreshToken;
using CoreFX.Auth.Models;
using Hello6.Domain.Endpoint.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Swashbuckle.AspNetCore.Annotations;

namespace Hello6.Domain.Endpoint.Controllers.AuthActions
{
    public partial class AuthController : DomainContollerBase
    {
        /// <summary>
        /// Refresh token key
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersionNeutral]
        [ApiExplorerSettings(GroupName = "latest")]
        [Route("api/auth/refresh-token")] // latest
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> RefreshToken_latest([FromBody] RefreshToken_RequestDto requestDto) => await RefreshToken_v202104(requestDto);

        /// <summary>
        /// Refresh token key
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersion("202603")]
        [ApiVersion("202104")]
        [ApiExplorerSettings(GroupName = "v202104")]
        [Route("api/v202104/auth/refresh-token")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> RefreshToken_v202104([FromBody] RefreshToken_RequestDto requestDto) => await RefreshToken_v202103(requestDto);

        /// <summary>
        /// Refresh token key
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersion("202103")]
        [ApiExplorerSettings(GroupName = "v202103")]
        [Route("api/v202103/auth/refresh-token")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> RefreshToken_v202103([FromBody] RefreshToken_RequestDto requestDto)
        {
            var res = _sessionAdmin.RefeshToken(requestDto.RefreshToken);
            await Task.CompletedTask;
            return Ok(res);
        }
    }
}
