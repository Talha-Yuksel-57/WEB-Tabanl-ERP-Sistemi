using ERP.Core.DTOs.Sale;
using FluentValidation;

namespace ERP.API.Validators
{
    public class CreateSaleValidator : AbstractValidator<CreateSaleDto>
    {
        public CreateSaleValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Geçerli bir ürün seçiniz.");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Geçerli bir müşteri seçiniz.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Satış miktarı en az 1 olmalıdır.");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Ödeme yöntemi boş olamaz.")
                .Must(m => new[] { "Nakit", "Kredi Kartı", "Havale" }.Contains(m))
                .WithMessage("Ödeme yöntemi Nakit, Kredi Kartı veya Havale olmalıdır.");
        }
    }
}
