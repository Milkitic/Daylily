﻿using Daylily.Bot.Backend;
using Daylily.Bot.Messaging;
using Daylily.CoolQ;
using Daylily.CoolQ.Messaging;
using Daylily.CoolQ.Plugin;
using Daylily.Osu;
using Daylily.Osu.Cabbage;
using OSharp.Api.V1.User;
using System;
using System.Linq;

namespace Daylily.Plugin.Osu
{
    [Name("绑定id")]
    [Author("yf_extension")]
    [Version(2, 0, 1, PluginVersion.Stable)]
    [Help("绑定osu id至发送者QQ。")]
    [Command("setid")]
    public class SetId : CoolQCommandPlugin
    {
        public override Guid Guid => new Guid("7c844f99-d416-4a9d-99b8-9b7c575bac93");

        [FreeArg]
        [Help("绑定指定的osu用户名。若带空格，请使用引号。")]
        public string UserName { get; set; }

        public override CoolQRouteMessage OnMessageReceived(CoolQScopeEventArgs scope)
        {
            var routeMsg = scope.RouteMessage;
            string userName = Decode(UserName);
            if (string.IsNullOrEmpty(userName))
                return routeMsg.ToSource(DefaultReply.ParamMissing);

            BllUserRole bllUserRole = new BllUserRole();
            OldSiteApiClient client = new OldSiteApiClient();
            int userNum = client.GetUser(UserComponent.FromUserName(UserName), out var userObj);
            if (userNum == 0)
                return routeMsg.ToSource(DefaultReply.IdNotFound, true);
            if (userNum > 1)
            {
                // ignored
            }

            var role = bllUserRole.GetUserRoleByQq(long.Parse(routeMsg.UserId));
            if (role.Count != 0)
            {
                if (role[0].CurrentUname == userObj.UserName)
                    return routeMsg.ToSource("我早就认识你啦.", true);
                string msg = role[0].CurrentUname + "，我早就认识你啦. 有什么问题请找Mother Ship（扔锅）";
                return routeMsg.ToSource(msg, true);
            }

            var newRole = new TableUserRole
            {
                UserId = userObj.UserId,
                Role = "creep",
                QQ = long.Parse(routeMsg.UserId),
                LegacyUname = "[]",
                CurrentUname = userObj.UserName,
                IsBanned = false,
                RepeatCount = 0,
                SpeakingCount = 0,
                Mode = 0,
            };
            var exist = bllUserRole.GetUserRoleByUid(userObj.UserId);
            if (exist != null && exist.Count > 0)
            {
                return routeMsg.ToSource("这个账号已经被QQ: " + exist.First().QQ + "绑定啦，请联系妈船或对方QQ哦.");
            }
            int c = bllUserRole.InsertUserRole(newRole);
            return c < 1
                ? routeMsg.ToSource("由于各种强大的原因，绑定失败..")
                : routeMsg.ToSource("明白了，" + userObj.UserName + "，多好的名字呢.");
        }

        private static string Decode(string source) =>
            source.Replace("\\&amp;", "&tamp;").Replace("\\#91;", "&t#91;").Replace("\\&#93;", "&t#93;").Replace("\\&#44;", "&t#44;")
                .Replace("&amp;", "&").Replace("&#91;", "[").Replace("&#93;", "]").Replace("&#44;", ",").
                Replace("&tamp;", "&amp;").Replace("&t#91;", "&#91;").Replace("&t#93;", "&#93;").Replace("&t#44;", "&#44;");
    }
}
