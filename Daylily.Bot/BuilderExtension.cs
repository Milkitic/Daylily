﻿using Daylily.Bot.Interface;
using System;

namespace Daylily.Bot
{
    public static class BuilderExtension
    {
        public static T Config<T>(this T configurableObject, Action<T> config) where T : IConfigurable
        {
            config?.Invoke(configurableObject);
            return configurableObject;
        }
    }
}