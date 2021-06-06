using System;

namespace DevMark
{
    public class NopDiagnosticProvider : IDiagnosticProvider
    {
        public void Flush()
        {
        }

        public IDisposable StartOperation(string name)
        {
            return new NopDiagnosticOperation();
        }
    }

}