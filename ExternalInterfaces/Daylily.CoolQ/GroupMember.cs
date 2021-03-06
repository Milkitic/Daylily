﻿using System.Collections.Generic;

namespace Daylily.CoolQ
{
    public class GroupMemberGroupInfo
    {
        public GroupMemberGroupInfo(long userId)
        {
            UserId = userId;
            GroupIdList = new List<long>();
        }

        public long UserId { get; set; }
        public List<long> GroupIdList { get; set; }
    }
}
