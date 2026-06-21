using ERP.Core.DTOs.Product;
using FluentValidation;

namespace ERP.API.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");

            RuleFor(x => x.StockCount)
                .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");

            RuleFor(x => x.MinStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stok seviyesi negatif olamaz.");
        }
    }

    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ürün adı boş olamaz.")
                .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");

            RuleFor(x => x.MinStockLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stok seviyesi negatif olamaz.");
        }
    }

    public class UpdateStockValidator : AbstractValidator<UpdateStockDto>
    {
        public UpdateStockValidator()
        {
            RuleFor(x => x.NewStock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz.");
        }
    }
}
