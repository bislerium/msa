using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Msa.Extensions.Random;
using Msa.Protos.Common;

namespace Msa.Services.ServiceB.Services
{
    public class ActorService(ILogger<ActorService> logger) : Protos.Actor.ActorService.ActorServiceBase
    {

        public override async Task<Empty> Act(ResponseExpectationRequest request, ServerCallContext context)
        {
            logger.LogInformation("Performing ActionService's Action");

            await Random.Shared.Delay();

            request.Expect.Exception();

            return new Empty();
        }
    }
}
