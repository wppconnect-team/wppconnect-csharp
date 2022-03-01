using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace WPPConnect.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly WPPConnectInstance _WPPConnectInstance;

        public SessionController(WPPConnectInstance wppConnectInstance)
        {
            _WPPConnectInstance = wppConnectInstance;
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromHeader] string sessionName, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] WPPConnect.Models.Token token = default)
        {
            try
            {
                await _WPPConnectInstance.SessionCreate(sessionName, token);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("qrcode")]
        public async Task<IActionResult> QrCode([FromHeader] string sessionName)
        {
            try
            {
                WPPConnect.Models.Session session = await _WPPConnectInstance.SessionQrCode(sessionName);

                dynamic json = new JObject();
                json.Status = session.Status;
                json.Message = session.Mensagem;

                return Ok(json.ToString());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status([FromHeader] string sessionName)
        {
            try
            {
                WPPConnect.Models.Session session = await _WPPConnectInstance.SessionStatus(sessionName);

                dynamic json = new JObject();
                json.Status = session.Status;
                json.Message = session.Mensagem;

                return Ok(json.ToString());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromHeader] string sessionName)
        {
            try
            {
                WPPConnect.Models.Session session = await _WPPConnectInstance.SessionLogout(sessionName);

                dynamic json = new JObject();
                json.Status = session.Status;
                json.Message = session.Mensagem;

                return Ok(json.ToString());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("token")]
        public async Task<IActionResult> Token([FromHeader] string sessionName)
        {
            try
            {
                WPPConnect.Models.Token token = await _WPPConnectInstance.SessionToken(sessionName);

                return Ok(token);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
