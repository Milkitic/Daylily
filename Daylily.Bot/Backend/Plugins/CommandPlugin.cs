﻿using Daylily.Bot.Message;
using Daylily.Common.Utils.LoggerUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daylily.Bot.Backend.Plugins
{
    public abstract class CommandPlugin : ResponsivePlugin, IInjectableBackend, ICommandBackend
    {
        public sealed override PluginType PluginType => PluginType.Command;
        public override bool RunInMultiThreading { get; } = true;
        public override bool RunInMultipleInstances { get; } = true;
        public override BackendConfig BackendConfig { get; } = new BackendConfig();

        public virtual void OnCommandBindingFailed(BindingFailedEventArgs args)
        {

        }

        public string[] Commands { get; set; }

        protected CommandPlugin()
        {
            Type t = GetType();
            if (!t.IsDefined(typeof(CommandAttribute), false))
            {
                Logger.Warn($"\"{Name}\"尚未设置命令，因此无法被用户激活。");
            }
            else
            {
                var attrs = (CommandAttribute[])t.GetCustomAttributes(typeof(CommandAttribute), false);
                Commands = attrs.First().Commands;
            }
        }
    }
}