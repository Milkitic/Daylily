﻿using Daylily.Common.Assist;
using Daylily.Common.Interface.CQHttp;
using Daylily.Common.Models;
using Daylily.Common.Models.CQResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Daylily.Web.Function
{
    public class MessageHandler
    {
        public static GroupList GroupInfo { get; set; } = new GroupList();
        public static DiscussList DiscussInfo { get; set; } = new DiscussList();
        public static PrivateList PrivateInfo { get; set; } = new PrivateList();

        public static string CommandFlag = "!";

        Random rnd = new Random();
        int minTime = 200, maxTime = 300; // 回应的反应时间
        //string UserId = null, GroupId = null, DiscussId = null;
        //MessageType messageType;

        HttpApi CQApi = new HttpApi();

        /// <summary>
        /// 群聊消息
        /// </summary>
        public MessageHandler(GroupMsg parsedObj)
        {
            long id = parsedObj.GroupId;

            GroupInfo.Add(id);
            if (GroupInfo[id].MsgQueue.Count < GroupInfo[id].MsgLimit) // 允许缓存n条，再多的丢弃
                GroupInfo[id].MsgQueue.Enqueue(parsedObj);

            else if (!GroupInfo[id].LockMsg)
            {
                GroupInfo[id].LockMsg = true;
                AppConstruct.SendMessage(new CommonMessageResponse(parsedObj.Message, new CommonMessage(parsedObj)));
            }

            if (GroupInfo[id].Thread == null ||
                (GroupInfo[id].Thread.ThreadState != ThreadState.Running && GroupInfo[id].Thread.ThreadState != ThreadState.WaitSleepJoin))
            {
                GroupInfo[id].Thread = new Thread(new ParameterizedThreadStart(HandleGroupMessage));
                GroupInfo[id].Thread.Start(parsedObj);
            }
            else
            {
                Logger.InfoLine("当前已有" + GroupInfo[id].MsgQueue.Count + "条消息在" + id + "排队");
            }
        }
        /// <summary>
        /// 讨论组消息
        /// </summary>
        public MessageHandler(DiscussMsg parsedObj)
        {
            long id = parsedObj.DiscussId;

            DiscussInfo.Add(id);
            if (DiscussInfo[id].MsgQueue.Count < DiscussInfo[id].MsgLimit) // 允许缓存n条，再多的丢弃
                DiscussInfo[id].MsgQueue.Enqueue(parsedObj);

            else if (!DiscussInfo[id].LockMsg)
            {
                DiscussInfo[id].LockMsg = true;
                AppConstruct.SendMessage(new CommonMessageResponse(parsedObj.Message, new CommonMessage(parsedObj)));
            }

            if (DiscussInfo[id].Thread == null ||
                (DiscussInfo[id].Thread.ThreadState != ThreadState.Running && DiscussInfo[id].Thread.ThreadState != ThreadState.WaitSleepJoin))
            {
                DiscussInfo[id].Thread = new Thread(new ParameterizedThreadStart(HandleDiscussMessage));
                DiscussInfo[id].Thread.Start(parsedObj);
            }
            else
            {
                Logger.InfoLine("当前已有" + DiscussInfo[id].MsgQueue.Count + "条消息在" + id + "排队");
            }
        }
        /// <summary>
        /// 私聊消息
        /// </summary>
        public MessageHandler(PrivateMsg parsedObj)
        {
            long id = parsedObj.UserId;

            PrivateInfo.Add(id);
            if (PrivateInfo[id].MsgQueue.Count < PrivateInfo[id].MsgLimit) // 允许缓存n条，再多的丢弃
                PrivateInfo[id].MsgQueue.Enqueue(parsedObj);

            else if (!PrivateInfo[id].LockMsg)
            {
                PrivateInfo[id].LockMsg = true;
                AppConstruct.SendMessage(new CommonMessageResponse("？？求您慢点说话好吗", new CommonMessage(parsedObj)));
            }

            if (PrivateInfo[id].Thread == null ||
                (PrivateInfo[id].Thread.ThreadState != ThreadState.Running && PrivateInfo[id].Thread.ThreadState != ThreadState.WaitSleepJoin))
            {
                PrivateInfo[id].Thread = new Thread(new ParameterizedThreadStart(HandlePrivateMessage));
                PrivateInfo[id].Thread.Start(parsedObj);
            }
            else
            {
                Logger.InfoLine("当前已有" + PrivateInfo[id].MsgQueue.Count + "条消息在" + id + "排队");
            }
        }

        private void HandleGroupMessage(object obj)
        {
            var parsedObj = (GroupMsg)obj;
            long groupId = parsedObj.GroupId;

            while (GroupInfo[groupId].MsgQueue.Count != 0)
            {
                if (GroupInfo[groupId].MsgQueue.Count == 0) break; // 不加这条总有奇怪的错误发生

                var currentInfo = GroupInfo[groupId].MsgQueue.Dequeue();

                currentInfo.Message.Replace("\n", "").Replace("\r", "").Trim();
                CommonMessage commonMessage = new CommonMessage(currentInfo);
                try
                {
                    HandleMessage(commonMessage);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        Logger.DangerLine(ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace);
                    else
                        Logger.DangerLine(ex.Message + Environment.NewLine + ex.StackTrace);
                    //GC.Collect();
                }
            }
            GroupInfo[groupId].LockMsg = false;
        }
        private void HandleDiscussMessage(object obj)
        {
            var parsedObj = (DiscussMsg)obj;

            long discussId = parsedObj.DiscussId;
            while (DiscussInfo[discussId].MsgQueue.Count != 0)
            {
                if (DiscussInfo[discussId].MsgQueue.Count == 0) break; // 不加这条总有奇怪的错误发生

                var currentInfo = DiscussInfo[discussId].MsgQueue.Dequeue();

                currentInfo.Message.Replace("\n", "").Replace("\r", "").Trim();
                CommonMessage commonMessage = new CommonMessage(currentInfo);
                try
                {
                    HandleMessage(commonMessage);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        Logger.DangerLine(ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace);
                    else
                        Logger.DangerLine(ex.Message + Environment.NewLine + ex.StackTrace);
                    //GC.Collect();
                }
            }
            DiscussInfo[discussId].LockMsg = false;
        }
        private void HandlePrivateMessage(object obj)
        {
            var parsedObj = (PrivateMsg)obj;

            long userId = parsedObj.UserId;
            while (PrivateInfo[userId].MsgQueue.Count != 0)
            {
                if (PrivateInfo[userId].MsgQueue.Count == 0) break; // 不加这条总有奇怪的错误发生

                var currentInfo = PrivateInfo[userId].MsgQueue.Dequeue();

                currentInfo.Message.Replace("\n", "").Replace("\r", "").Trim();
                CommonMessage commonMessage = new CommonMessage(currentInfo);

                try
                {
                    HandleMessage(commonMessage);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        Logger.DangerLine(ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace);
                    else
                        Logger.DangerLine(ex.Message + Environment.NewLine + ex.StackTrace);
                    //GC.Collect();
                }
            }
            PrivateInfo[userId].LockMsg = false;
        }

        private void HandleMessage(CommonMessage commonMessage)
        {
            long groupId = Convert.ToInt64(commonMessage.GroupId);
            long userId = Convert.ToInt64(commonMessage.UserId);
            long discussId = Convert.ToInt64(commonMessage.DiscussId);
            var type = commonMessage.MessageType;
            string message = commonMessage.Message;

            switch (commonMessage.MessageType)
            {
                case MessageType.Private:
                    Logger.WriteLine($"{userId}: {CqCode.Decode(message)}");
                    break;
                case MessageType.Discuss:
                    Logger.WriteLine($"({DiscussInfo[discussId].Name}) {userId}: {CqCode.Decode(message)}");
                    break;
                case MessageType.Group:
                    var userInfo = CQApi.GetGroupMemberInfo(groupId.ToString(), userId.ToString());  // 有点费时间
                    Logger.WriteLine($"({GroupInfo[groupId].Name}) {userInfo.Data.Nickname}: {CqCode.Decode(message)}");
                    break;
            }

            if (commonMessage.Message.Substring(0, 1) == CommandFlag)
            {
                if (commonMessage.Message.IndexOf(CommandFlag + "root ") == 0)
                {
                    if (commonMessage.UserId != "2241521134")
                    {
                        AppConstruct.SendMessage(new CommonMessageResponse("你没有权限...", commonMessage));
                    }
                    else
                    {
                        commonMessage.FullCommand = commonMessage.Message.Substring(6, commonMessage.Message.Length - 6);
                        commonMessage.PermissionLevel = PermissionLevel.Root;
                        HandleMessageCmd(commonMessage);
                    }

                }
                else if (message.IndexOf(CommandFlag + "sudo ") == 0 && type == MessageType.Group)
                {
                    if (!GroupInfo[groupId].AdminList.Contains(userId))
                    {
                        AppConstruct.SendMessage(new CommonMessageResponse("你没有权限...仅本群管理员可用", commonMessage));
                    }
                    else
                    {
                        commonMessage.FullCommand = message.Substring(6, message.Length - 6);
                        commonMessage.PermissionLevel = PermissionLevel.Admin;
                        HandleMessageCmd(commonMessage);
                    }
                }
                else
                {
                    commonMessage.FullCommand = message.Substring(1, message.Length - 1);
                    HandleMessageCmd(commonMessage);
                }
            }

            HandleMesasgeApp(commonMessage);

        }
        private void HandleMesasgeApp(CommonMessage commonMessage)
        {
            foreach (var item in Mapper.NormalPlugins)
            {

                #region 折叠：invoke
                Type type = Type.GetType("Daylily.Web.Function.Application." + item);
                MethodInfo mi = type.GetMethod("Execute");
                object appClass = Activator.CreateInstance(type);
                object[] invokeArgs = { commonMessage };

                #endregion

                CommonMessageResponse reply;
                try
                {
                    reply = (CommonMessageResponse)mi.Invoke(appClass, invokeArgs);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        throw new Exception("\n\"" + commonMessage.Message + "\" caused an exception: \n" +
                            type.Name + ": " + ex.InnerException.Message + "\n\n" + ex.InnerException.StackTrace);
                    throw new Exception("\n\"" + commonMessage.Message + "\" caused an exception: \n" +
                                        type.Name + ": " + ex.Message + "\n\n" + ex.StackTrace);
                }
                if (reply == null) continue;
                AppConstruct.SendMessage(reply);
            }
        }
        private void HandleMessageCmd(CommonMessage commonMessage)
        {
            string fullCmd = commonMessage.FullCommand;
            Thread.Sleep(rnd.Next(minTime, maxTime));

            commonMessage.Command = fullCmd.Split(' ')[0].Trim();
            commonMessage.Parameter = fullCmd.IndexOf(" ") == -1 ? "" :
                fullCmd.Substring(fullCmd.IndexOf(" ") + 1, fullCmd.Length - commonMessage.Command.Length - 1).Trim();
            string className = Mapper.GetClassName(commonMessage.Command, out string file);
            if (className == null)
                return;

            #region 折叠：invoke
            MethodInfo mi;
            object appClass;
            Type type;
            System.IO.FileInfo fi = null;
            if (file == null)
            {
                type = Type.GetType("Daylily.Web.Function.Application.Command." + className);
                appClass = Activator.CreateInstance(type);
            }
            else
            {
                try
                {
                    Logger.PrimaryLine("读取插件信息中");
                    fi = new System.IO.FileInfo(file);
                    Assembly assemblyTmp = Assembly.LoadFrom(file);
                    type = assemblyTmp.GetType(className);
                    appClass = assemblyTmp.CreateInstance(className);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        throw new Exception("\n\"/" + fullCmd + "\" caused an exception: \n" +
                            fi.Name + ": " + ex.InnerException.Message + "\n\n" + ex.InnerException.StackTrace);
                    throw new Exception("\n\"/" + fullCmd + "\" caused an exception: \n" +
                                        fi.Name + ": " + ex.Message + "\n\n" + ex.StackTrace);
                }
            }

            object[] invokeArgs = { commonMessage };
            #endregion

            CommonMessageResponse reply;
            try
            {
                mi = type.GetMethod("Execute");
                reply = (CommonMessageResponse)mi.Invoke(appClass, invokeArgs);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw new Exception("\n/\"" + fullCmd + "\" caused an exception: \n" +
                        type.Name + ": " + ex.InnerException.Message + "\n\n" + ex.InnerException.StackTrace);
                else
                    throw new Exception("\n/\"" + fullCmd + "\" caused an exception: \n" +
                        type.Name + ": " + ex.Message + "\n\n" + ex.StackTrace);
            }

            if (reply == null) return;
            AppConstruct.SendMessage(reply);
        }
    }
}