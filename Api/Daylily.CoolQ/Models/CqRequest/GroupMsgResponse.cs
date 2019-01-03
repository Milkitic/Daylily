﻿using Newtonsoft.Json;

namespace Daylily.CoolQ.Models.CqRequest
{
    public class GroupMsgResponse
    {
        [JsonProperty(PropertyName = "reply")]
        public string Reply { get; set; }
        [JsonProperty(PropertyName = "auto_escape")]
        public bool AutoEscape { get; set; }
        [JsonProperty(PropertyName = "at_sender")]
        public bool AtSender { get; set; }
        [JsonProperty(PropertyName = "delete")]
        public bool Delete { get; set; }
        [JsonProperty(PropertyName = "kick")]
        public bool Kick { get; set; }
        [JsonProperty(PropertyName = "ban")]
        public bool Ban { get; set; }
    }
}
