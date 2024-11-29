using EPS.Challenge.Repositories;
using Grpc.Core;

namespace EPS.Challenge.Services
{
    public class DiscountService : Discount.DiscountBase
    {
        private readonly DiscountRepository _discountRepository;
        public DiscountService(DiscountRepository discountRepository)
        {
            _discountRepository = discountRepository;
        }

        public override Task<GenerateResponse> GenerateCodes(GenerateRequest request, ServerCallContext context)
        {
            if (request.Count > 2000 || request.Length < 7 || request.Length > 8)
            {
                return Task.FromResult(new GenerateResponse { Result = false });
            }

            var newCodes = new List<string>();
            var random = new Random();
            for (var i = 0; i < request.Count; i++)
            {
                var code = GenerateRandomCode(request.Length, random);
                if (_discountRepository.AddActiveCode(code))
                {
                    newCodes.Add(code);
                    _discountRepository.SaveCodeToDatabase(code);
                }
            }

            return Task.FromResult(new GenerateResponse { Result = true, Codes = { newCodes } });
        }

        public override Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context)
        {
            if (_discountRepository.GetActiveCode(request.Code, out var isUsed))
            {
                if (isUsed)
                {
                    return Task.FromResult(new UseCodeResponse { Result = 1 }); // Already used
                }

                _discountRepository.SetCodeAsUsed(request.Code);
                _discountRepository.MarkCodeAsUsedInDatabase(request.Code);
                return Task.FromResult(new UseCodeResponse { Result = 0 }); // Success
            }

            return Task.FromResult(new UseCodeResponse { Result = 2 }); // Not found
        }

        private static string GenerateRandomCode(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
