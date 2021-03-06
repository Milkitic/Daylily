﻿using Daylily.Bot.Session;
using System;
using System.Collections.Generic;
using System.Text;
using Daylily.Bot.Messaging;
using Daylily.Common;

namespace Daylily.Bot.Dispatcher
{
    public abstract class CompoundDispatcher : IMessageDispatcher, IEventDispatcher, ISessionDispatcher
    {
        public event ExceptionEventHandler ErrorOccured;
        public event SessionReceivedEventHandler SessionReceived;
        public abstract MiddlewareConfig MiddlewareConfig { get; }
        public abstract bool Message_Received(object sender, MessageEventArgs args);
        public abstract void SendMessageAsync(RouteMessage message);
        public abstract bool Event_Received(object sender, EventEventArgs args);

        protected virtual bool RaiseSessionEvent(RouteMessage message)
        {
            if (SessionReceived != null)
            {
                SessionReceived?.Invoke(null, new SessionReceivedEventArgs
                {
                    RouteMessageObj = message
                });
                return true;
            }

            return false;
        }
    }
}
