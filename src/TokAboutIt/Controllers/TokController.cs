using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using OpenTokSDK;
using TokAboutIt.Models;

namespace TokAboutIt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokController : ControllerBase
    {
        private readonly OpenTok _openTok;
        private readonly Dictionary<string, string> _openTokSessions;

        public TokController(Dictionary<string, string> openTokSessions)
        {
            int opentokAPIKey = Convert.ToInt32(Environment.GetEnvironmentVariable("OPENTOK_API_KEY"));
            string opentokAPISecret = Environment.GetEnvironmentVariable("OPENTOK_API_SECRET");
            _openTok = new OpenTok(opentokAPIKey, opentokAPISecret);
            _openTokSessions = openTokSessions;
        }

        [HttpPost("join")]
        public IActionResult Join([FromForm]JoinTokModel joinTokModel)
        {
            string sessionName = joinTokModel.RoomName.Trim().ToLower();

            if (string.IsNullOrEmpty(sessionName))
            {
                return BadRequest("Room Name is required.");
            }

            string nickname = joinTokModel.NickName.Trim();

            if (string.IsNullOrEmpty(nickname))
            {
                return BadRequest("Nick Name is required.");
            }

            string sessionId;
            
            if (_openTokSessions.Count(a => a.Value.Equals(sessionName)) > 0)
            {
                // session exists
                sessionId = _openTokSessions.FirstOrDefault(f => f.Value.Equals(sessionName)).Key;
            }
            else
            {
                // create session
                Session session = _openTok.CreateSession();
                sessionId = session.Id;

                _openTokSessions.Add(sessionId, sessionName);
            }

            Role role = Role.PUBLISHER;
            if (nickname.ToLower().Equals("opentok"))
            {
                role = Role.MODERATOR;
            }

            double sevenDaysFromNow = DateTime.UtcNow.AddDays(7).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            string token = _openTok.GenerateToken(sessionId, role, sevenDaysFromNow, $"name={nickname}");

            TokResponseModel result = new TokResponseModel()
            {
                SessionId = sessionId,
                Token = token
            };

            return Ok(result);
        }
    }
}