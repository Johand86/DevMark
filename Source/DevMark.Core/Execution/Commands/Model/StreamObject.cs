using System;

namespace DevMark
{
    public class StreamObject
    {
        public DateTime Timestamp { get; set; }
        public StreamSeverty Severity { get; set; }
        public object Message { get; set; }
    }
}
