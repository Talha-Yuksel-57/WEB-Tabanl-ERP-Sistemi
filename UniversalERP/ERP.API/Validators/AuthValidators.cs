using ERP.Core.DTOs.Auth;
using FluentValidation;

namespace ERP.API.Validators
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        }
    }

    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad boş olamaz.")
                .MaximumLength(100).WithMessage("Ad Soyad en fazla 100 karakter olabilir.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0).WithMessage("Geçerli bir firma seçiniz.");

            RuleFor(x => x.Role)
                .Must(r => new[] { "TenantAdmin", "Manager", "Employee", "Cashier", "Technician" }.Contains(r))
                .WithMessage("Geçersiz rol seçimi.")
                .When(x => !string.IsNullOrEmpty(x.Role));
        }
    }
}
