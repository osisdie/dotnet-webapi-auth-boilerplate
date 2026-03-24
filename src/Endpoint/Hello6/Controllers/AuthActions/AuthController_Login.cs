using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CoreFX.Abstractions.Bases.Interfaces;
using CoreFX.Abstractions.Contracts;
using CoreFX.Abstractions.Contracts.Extensions;
using CoreFX.Abstractions.Enums;
using CoreFX.Auth.Contracts.Login;
using CoreFX.Auth.Models;
using CoreFX.Notification.Smtp.Models;
using Hello6.Domain.Endpoint.Controllers.Bases;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Swashbuckle.AspNetCore.Annotations;

namespace Hello6.Domain.Endpoint.Controllers.AuthActions
{
    public partial class AuthController : DomainContollerBase
    {
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="requestDto">Username and password</param>
        /// <returns></returns>
        [ApiVersionNeutral]
        [ApiExplorerSettings(GroupName = "latest")]
        [Route("api/auth/login")] // latest
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> Login_latest([FromBody] AuthLogin_RequestDto requestDto) => await Login_v202104(requestDto);

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="requestDto">Username and password</param>
        /// <returns></returns>
        [ApiVersion("202603")]
        [ApiVersion("202104")]
        [ApiExplorerSettings(GroupName = "v202104")]
        [Route("api/v202104/auth/login")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> Login_v202104([FromBody] AuthLogin_RequestDto requestDto) => await Login_v202103(requestDto);

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="requestDto">Username and password</param>
        /// <returns></returns>
        [ApiVersion("202103")]
        [ApiExplorerSettings(GroupName = "v202103")]
        [Route("api/v202103/auth/login")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<JwtTokenDto>))]
        public async Task<IActionResult> Login_v202103([FromBody] AuthLogin_RequestDto requestDto)
        {
            var loginRes = await _svcLogin.ExecuteAsync(requestDto);
            if (!loginRes.Any())
            {
                loginRes.SetSubCode(SvcCodeEnum.AccountLoginFailed);

                _mediator?.Publish(new SvcEvent_MetadataDto
                {
                    From = HttpContext.Request.GetDisplayUrl(),
                    Category = "User login",
                    IsSuccess = loginRes.Any(),
                    User = requestDto.Username,
                    RequestDto = requestDto,
                    ResponseDto = loginRes,
                    Context = HttpContext
                });

                return new UnauthorizedObjectResult(loginRes);
            }

            var jwtRes = await _sessionAdmin.Authentication(requestDto.Username, requestDto.Password);
            if (!jwtRes.Any())
            {
                loginRes.SetSubCode(SvcCodeEnum.TokenCreateFailed);
                return new UnauthorizedObjectResult(jwtRes);
            }

            var res = new SvcResponseDto<AuthLogin_ResponseDto>
            {
                Data = new AuthLogin_ResponseDto
                {
                    UserId = jwtRes.Data.UserId,
                    UserName = jwtRes.Data.UserName,
                    Token = jwtRes.Data.Token,
                    RefreshToken = jwtRes.Data.RefreshToken,
                    Exp = jwtRes.Data.Exp,
                }
            }.Success();

            return Ok(res);
        }
    }
}
