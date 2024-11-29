using EPS.Challenge.Model;

namespace EPS.Challenge.Repositories.Interfaces
{
    public interface IDiscountRepository
    {
        DiscountCode? GetActiveCode(string code);
        bool MarkCodeAsUsed(string code);
        bool SaveCodeToDatabase(string code);
    }
}