using Codebelt.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace Codebelt.Extensions.Swashbuckle.AspNetCore.Assets.V1
{
    /// <summary>
    /// A fake controller for functional testing purposes.
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class FakeController : ControllerBase
    {
        /// <summary>
        /// Gets an OK response with a body of Functional Test V1.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(FakeModel), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public IActionResult Get()
        {
            return Ok(new FakeModel { Message = "Functional Test V1" });
        }

        /// <summary>
        /// Gets an OK response with a body of Token.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet("/token")]
        [ProducesResponseType(typeof(Token), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public IActionResult GetToken()
        {
            return Ok(new Token("Functional Test V1"));
        }
    }
}
