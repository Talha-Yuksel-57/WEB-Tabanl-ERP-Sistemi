using ERP.Core.DTOs.Customer;
using FluentValidation;

namespace ERP.API.Validators
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
                .MaximumLength(150).WithMessage("Müşteri adı en fazla 150 karakter olabilir.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Phone)
                .Matches(@"^[0-9\s\+\-\(\)]{7,20}$").WithMessage("Geçerli bir telefon numarası giriniz.")
                .When(x => !string.IsNullOrEmpty(x.Phone));
        }
    }

    public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Müşteri adı boş olamaz.")
                .MaximumLength(150).WithMessage("Müşteri adı en fazla 150 karakter olabilir.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}
