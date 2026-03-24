using System.Net;
using System.Threading.Tasks;
using CoreFX.Abstractions.Bases.Interfaces;
using CoreFX.Abstractions.Contracts.Extensions;
using Hello6.Domain.Contract.Models.Echo;
using Hello6.Domain.Endpoint.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Swashbuckle.AspNetCore.Annotations;

namespace Hello6.Domain.Endpoint.Controllers
{
    public partial class HelloController : DomainContollerBase
    {
        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersionNeutral]
        [ApiExplorerSettings(GroupName = "latest")]
        [Route("api/hello/sendcommand")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<HelloSendCommand_ResponseDto>))]
        public async Task<ActionResult> SendCommand_latest(HelloSendCommand_RequestDto requestDto) => await SendCommand_v202104(requestDto);

        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersion("202603")]
        [ApiVersion("202104")]
        [ApiExplorerSettings(GroupName = "v202104")]
        [Route("api/v202104/hello/sendcommand")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<HelloSendCommand_ResponseDto>))]
        public async Task<ActionResult> SendCommand_v202104(HelloSendCommand_RequestDto requestDto)
        {
            var res = await _svcSendCommand.ExecuteAsync(requestDto);

            if (res.Any())
            {
                res.Data.Recv += "_v202104_format";
            }

            return Ok(res);
        }

        /// <summary>
        /// Send command
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [ApiVersion("202103")]
        [ApiExplorerSettings(GroupName = "v202103")]
        [Route("api/v202103/hello/sendcommand")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ISvcResponseBaseDto<HelloSendCommand_ResponseDto>))]
        public async Task<ActionResult> SendCommand_v202103(HelloSendCommand_RequestDto requestDto)
        {
            var res = await _svcSendCommand.ExecuteAsync(requestDto);

            if (res.Any())
            {
                res.Data.Recv += "_v202103_format";
            }

            return Ok(res);
        }

    }
}
