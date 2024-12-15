using Grpc.Core;

namespace Msa.Protos.Common
{
    public static class ExpectExtensions
    {
        public static void Exception(this Expect expect)
        {
            var shouldThrow = expect == Expect.Failure || (expect == Expect.Random && Random.Shared.Next(0, 2) == 1);
            if (shouldThrow)
                throw new RpcException(new Status(StatusCode.Internal, "Failure"));
        }
    }
}
