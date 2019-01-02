﻿using Daylily.Bot;
using Daylily.Bot.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Daylily.AspNetCore.Controllers
{
    public class ApiController : Controller
    {
        private readonly Core _daylily;

        public ApiController(Bot.Core daylily)
        {
            _daylily = daylily;
        }

        [HttpPost]
        public async Task<JsonResult> GetResponse()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            if (ip == "74.120.171.198")
            {
                return Json(new { });
            }

            using (var sr = new StreamReader(Request.Body))
            {
                string json = await sr.ReadToEndAsync();
                _daylily.RaiseRawObjectEvents(json);
                //Core.ReceiveJson(json);
            }

            return Json(new { });
        }
    }
}