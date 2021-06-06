using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace DevMark
{
    public class AIDiagnosticOperation : IDisposable
    {
        private readonly IOperationHolder<RequestTelemetry> _operation;

        public AIDiagnosticOperation(IOperationHolder<RequestTelemetry> operation)
        {
            _operation = operation;
        }

        public void Dispose()
        {
            _operation?.Dispose();
        }
    }

}