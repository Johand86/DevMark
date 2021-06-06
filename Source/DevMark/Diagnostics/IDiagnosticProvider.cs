using System;

namespace DevMark
{
    public interface IDiagnosticProvider
    {
        void Flush();
        IDisposable StartOperation(string name);
    }
}