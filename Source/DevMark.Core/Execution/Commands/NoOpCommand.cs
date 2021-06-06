using System.Collections.Generic;

namespace DevMark
{
    public class NoOpCommand : CommandBase
    {
        public NoOpCommand()
        {
            Params = new Dictionary<string, object>();
            Script = "@{ ExitCode = 0 }";
        }
    }
}
