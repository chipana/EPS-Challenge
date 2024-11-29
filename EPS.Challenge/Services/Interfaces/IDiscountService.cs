using Grpc.Core;

namespace EPS.Challenge.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<GenerateResponse> GenerateCodes(GenerateRequest request, ServerCallContext context);
        Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context);
    }
}