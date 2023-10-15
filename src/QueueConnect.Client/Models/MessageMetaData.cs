using System;
using System.Collections.Generic;

namespace QueueConnect.Client.Models
{
    public class MessageMetaData
    {
        public Dictionary<DateTimeOffset, string> Errors { get; set; } = new Dictionary<DateTimeOffset, string>();
    }
}