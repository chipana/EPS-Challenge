using EPS.Challenge.Repositories.Interfaces;
using EPS.Challenge.Services.Interfaces;
using EPS.Challenge.Model;
using Grpc.Core;

namespace EPS.Challenge.Services
{
    public class DiscountService : Discount.DiscountBase, IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        public DiscountService(IDiscountRepository discountRepository)
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
                if (_discountRepository.SaveCodeToDatabase(code))
                    newCodes.Add(code);
            }

            return Task.FromResult(new GenerateResponse { Result = true, Codes = { newCodes } });
        }

        public override Task<UseCodeResponse> UseCode(UseCodeRequest request, ServerCallContext context)
        {
            var discountCode = _discountRepository.GetActiveCode(request.Code);
            if (discountCode != null)
            {
                if (discountCode.IsUsed)
                {
                    return Task.FromResult(new UseCodeResponse { Result = 1 }); // Already used
                }

                _discountRepository.MarkCodeAsUsed(request.Code);
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
