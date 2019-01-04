﻿using Daylily.Bot;
using Daylily.Bot.Enum;
using Daylily.Bot.Message;
using Daylily.Common.Utils.LoggerUtils;
using System;
using System.Linq;
using Daylily.Bot.Backend;
using Daylily.CoolQ.Message;
using CommonMessageResponse = Daylily.Bot.Message.CommonMessageResponse;

namespace Daylily.Plugin.Kernel
{
    public class PermissionChecker : ApplicationPlugin
    {
        public override bool RunInMultiThreading => false;

        public override BackendConfig BackendConfig { get; } = new BackendConfig
        {
            Priority = -1
        };

        public override CommonMessageResponse OnMessageReceived(CoolQNavigableMessage navigableMessageObj)
        {
            var cm = navigableMessageObj;

            long groupId = Convert.ToInt64(cm.GroupId);
            long userId = Convert.ToInt64(cm.UserId);
            long discussId = Convert.ToInt64(cm.DiscussId);
            string message = cm.Message.RawMessage;
            var type = cm.MessageType;

            if (message.Substring(0, 1) == Bot.Core.CurrentCore.CommandFlag)
            {
                if (message.IndexOf(Bot.Core.CurrentCore.CommandFlag + "root ", StringComparison.InvariantCulture) == 0)
                {
                    if (cm.UserId != "2241521134")
                    {
                        Logger.Raw("Access denied.");
                        return new CommonMessageResponse(LoliReply.FakeRoot, cm)
                        {
                            Handled = true
                        };
                    }
                    else
                    {
                        cm.FullCommand = message.Substring(6, message.Length - 6);
                        cm.Authority = Authority.Root;
                    }

                }
                else if (message.IndexOf(Bot.Core.CurrentCore.CommandFlag + "sudo ", StringComparison.InvariantCulture) == 0 &&
                         cm.MessageType == MessageType.Group)
                {
                    if (CoolQDispatcher.Current.SessionInfo[cm.CqIdentity].GroupInfo.Admins.Count(q => q.UserId == userId) == 0)
                    {
                        Logger.Raw("Access denied.");
                        return new CommonMessageResponse(LoliReply.FakeAdmin, cm)
                        {
                            Handled = true
                        };
                    }
                    else
                    {
                        cm.FullCommand = message.Substring(6, message.Length - 6);
                        cm.Authority = Authority.Admin;
                    }
                }
                else
                {
                    // auto
                    if (CoolQDispatcher.Current.SessionInfo[cm.CqIdentity].GroupInfo?.Admins.Count(q => q.UserId == userId) != 0)
                        cm.Authority = Authority.Admin;
                    if (cm.UserId == "2241521134")
                        cm.Authority = Authority.Root;

                    cm.FullCommand = message.Substring(1, message.Length - 1);
                }
            }

            return null;
        }
    }
}