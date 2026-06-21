using ERP.Core.DTOs.ProjectTask;
using ERP.Core.DTOs.ServiceOrder;
using FluentValidation;

namespace ERP.API.Validators
{
    public class CreateServiceOrderValidator : AbstractValidator<CreateServiceOrderDto>
    {
        public CreateServiceOrderValidator()
        {
            RuleFor(x => x.DeviceName)
                .NotEmpty().WithMessage("Cihaz adı boş olamaz.")
                .MaximumLength(200).WithMessage("Cihaz adı en fazla 200 karakter olabilir.");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Geçerli bir müşteri seçiniz.");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Müşteri adı boş olamaz.");

            RuleFor(x => x.IssueDescription)
                .NotEmpty().WithMessage("Arıza açıklaması boş olamaz.")
                .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");

            RuleFor(x => x.ServiceFee)
                .GreaterThanOrEqualTo(0).WithMessage("Servis ücreti negatif olamaz.");

            RuleFor(x => x.PartCost)
                .GreaterThanOrEqualTo(0).WithMessage("Parça maliyeti negatif olamaz.");
        }
    }

    public class UpdateServiceOrderStatusValidator : AbstractValidator<UpdateServiceOrderStatusDto>
    {
        private static readonly string[] ValidStatuses =
            { "Beklemede", "Tamirde", "Tamamlandı", "Teslim Edildi", "İptal" };

        public UpdateServiceOrderStatusValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Durum boş olamaz.")
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage($"Geçerli durumlar: {string.Join(", ", ValidStatuses)}");
        }
    }

    public class CreateProjectTaskValidator : AbstractValidator<CreateProjectTaskDto>
    {
        public CreateProjectTaskValidator()
        {
            RuleFor(x => x.TaskTitle)
                .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
                .MaximumLength(300).WithMessage("Başlık en fazla 300 karakter olabilir.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Görev açıklaması boş olamaz.");

            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Geçerli bir proje seçiniz.");

            RuleFor(x => x.Priority)
                .Must(p => new[] { "Low", "Medium", "High" }.Contains(p))
                .WithMessage("Öncelik Low, Medium veya High olmalıdır.")
                .When(x => !string.IsNullOrEmpty(x.Priority));
        }
    }

    public class UpdateProjectTaskValidator : AbstractValidator<UpdateProjectTaskDto>
    {
        public UpdateProjectTaskValidator()
        {
            RuleFor(x => x.TaskTitle)
                .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
                .MaximumLength(300).WithMessage("Başlık en fazla 300 karakter olabilir.");

            RuleFor(x => x.HoursWorked)
                .GreaterThanOrEqualTo(0).WithMessage("Çalışılan saat negatif olamaz.");

            RuleFor(x => x.Status)
                .Must(s => new[] { "Yapılacak", "Yapılıyor", "Bitti" }.Contains(s))
                .WithMessage("Geçerli durumlar: Yapılacak, Yapılıyor, Bitti")
                .When(x => !string.IsNullOrEmpty(x.Status));
        }
    }
}
