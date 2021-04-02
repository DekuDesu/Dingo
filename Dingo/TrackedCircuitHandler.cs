using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dingo
{
    public class TrackedCircuitHandler : CircuitHandler
    {
        private readonly ILogger<TrackedCircuitHandler> logger;

        public static HashSet<string> Ids { get; set; } = new();

        public TrackedCircuitHandler(ILogger<TrackedCircuitHandler> logger)
        {
            this.logger = logger;
        }
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            if (Ids.Contains(circuit.Id))
            {
                Ids.Remove(circuit.Id);
            }

            logger.LogInformation("Circuit closed {CircuitId}, total circuits {CircuitCount}", circuit.Id, Ids.Count);

            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
        public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogInformation("Circuit opened {CircuitId}", circuit.Id);

            logger.LogInformation("Circuit opened {CircuitId}, total circuits {CircuitCount}", circuit.Id, Ids.Count);

            await base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }
        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogInformation("Circuit lost connection {CircuitId}", circuit.Id);
            return base.OnConnectionDownAsync(circuit, cancellationToken);
        }
        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogInformation("Circuit reconnected {CircuitId}", circuit.Id);
            return base.OnConnectionUpAsync(circuit, cancellationToken);
        }
    }
}
