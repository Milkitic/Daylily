﻿using Bleatingsheep.Osu.ApiV2b.Models;
using Daylily.Bot.Attributes;
using Daylily.Bot.Enum;
using Daylily.Bot.Models;
using Daylily.Bot.PluginBase;
using Daylily.Common.Utils.LoggerUtils;
using Daylily.Common.Utils.RequestUtils;
using Daylily.CoolQ;
using Daylily.Osu.Database.BLL;
using Daylily.Osu.Database.Model;
using Daylily.Osu.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Daylily.Plugin.Osu
{
    [Name("随机挖坑")]
    [Author("yf_extension")]
    [Version(0, 1, 0, PluginVersion.Beta)]
    [Help("从发送者的Graveyard Beatmaps中随机挖一张图。")]
    [Command("挖坑")]
    public class MyGraveyard : CommandPlugin
    {
        public override void OnInitialized(string[] args)
        {

        }

        public override CommonMessageResponse OnMessageReceived(CommonMessage messageObj)
        {
            BllUserRole bllUserRole = new BllUserRole();
            List<TblUserRole> userInfo = bllUserRole.GetUserRoleByQq(long.Parse(messageObj.UserId));
            if (userInfo.Count == 0)
                return new CommonMessageResponse(LoliReply.IdNotBound, messageObj, true);

            var id = userInfo[0].UserId.ToString();

            List<Beatmapset> totalList = new List<Beatmapset>();
            Beatmapset[] tmpArray;
            int page = 0;
            const int count = 10;
            do
            {
                string json = WebRequestUtil.GetResponseString(
                    WebRequestUtil.CreateGetHttpResponse(
                        "https://osu.ppy.sh/users/" + id + "/beatmapsets/graveyard?offset=" + page + "&limit=" + count));
                Logger.Debug("GET JSON");

                tmpArray = JsonConvert.DeserializeObject<Beatmapset[]>(json);
                totalList.AddRange(tmpArray);
                page += count;

                if (tmpArray.Length != count) break;
            } while (tmpArray.Length != 0);

            if (totalList.Count == 0)
            {
                return new CommonMessageResponse("惊了，你竟然会没坑！", messageObj, true);
            }

            Random rnd = new Random();
            Beatmapset beatmap = totalList[rnd.Next(totalList.Count)];
            var cqMusic = new CustomMusic("https://osu.ppy.sh/s/" + beatmap.Id, $"https://b.ppy.sh/preview/{beatmap.Id}.mp3", beatmap.Title,
                $"{beatmap.Artist}\r\n({beatmap.FavouriteCount} fav)", $"https://b.ppy.sh/thumb/{beatmap.Id}l.jpg");

            return new CommonMessageResponse(cqMusic.ToString(), messageObj);
        }
    }
}
