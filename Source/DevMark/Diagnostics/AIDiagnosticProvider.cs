using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace DevMark
{
    public class AIDiagnosticProvider : IDiagnosticProvider
    {
        ILogger<AIDiagnosticProvider> _logger;
        TelemetryClient _client;
        bool _enabled;

        public AIDiagnosticProvider(ILogger<AIDiagnosticProvider> logger, TelemetryClient client, bool enabled)
        {
            _client = client;
            _enabled = enabled;
            _logger = logger;
        }

        public IDisposable StartOperation(string name)
        {
            return new AIDiagnosticOperation(_enabled ? _client.StartOperation<RequestTelemetry>(name) : null);
        }

        public void Flush()
        {
            if (_enabled)
            {
                _logger.LogInformation("Waiting 5s to process diagnostics...");
                _client.Flush();
                Thread.Sleep(5000);
            }
        }
    }

}