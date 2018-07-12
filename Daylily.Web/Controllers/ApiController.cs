﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Daylily.Common.Assist;
using Daylily.Common.Function;
using Daylily.Common.Models.CQRequest;
using Daylily.Common.Models.CQResponse;
using Daylily.Common.Utils;
using Daylily.Common.Utils.LogUtils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Daylily.Web.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public async Task<JsonResult> GetResponse()
        {
            var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            if (ip == "74.120.171.198")
            {
                Logger.Warn("来自白菜的请求：" + ip);
                return Json(new { });
            }

            string json;
            using (var sr = new StreamReader(Request.Body))
            {
                json = await sr.ReadToEndAsync();
            }

            object ret = JsonHandler.HandleReportJson(json);

            return Json(ret ?? new { });
        }
    }
}