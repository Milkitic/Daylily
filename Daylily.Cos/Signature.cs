﻿using System;
using Daylily.Cos.Common;

namespace Daylily.Cos
{
    public static class Signature
    {
        public static int AppId;
        public static string SecretId;
        public static string SecretKey;
        public static string BucketName;

        public static string Get()
        {
            return Sign.Signature(AppId, SecretId, SecretKey, DateTime.Now.AddMonths(1).ToUnixTime() / 1000,
                BucketName);
        }
    }
}
