// CanPany.Application/Interfaces/Services/IVnPayService.cs
namespace CanPany.Application.Interfaces.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(string txnRef, long amount, string ipAddress, string? orderInfo = null);
    bool ValidateSecureHash(IDictionary<string, string> queryParams);
}
