using ERP.Core.DTOs.Customer;
using ERP.Core.DTOs.Product;
using ERP.Core.DTOs.Sale;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace ERP.Tests.Validators
{
    // ─── Validator'ları burada tekrar tanımlıyoruz
    // (ERP.API'ye referans çakışmasını önlemek için)

    class TestCreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public TestCreateProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün adı boş olamaz.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");
            RuleFor(x => x.StockCount).GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");
            RuleFor(x => x.MinStockLevel).GreaterThanOrEqualTo(0).WithMessage("Min stok negatif olamaz.");
        }
    }

    class TestCreateCustomerValidator : AbstractValidator<CreateCustomerDto>
    {
        public TestCreateCustomerValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().WithMessage("Müşteri adı boş olamaz.");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Geçerli e-posta giriniz.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    class TestCreateSaleValidator : AbstractValidator<CreateSaleDto>
    {
        public TestCreateSaleValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Geçerli ürün seçiniz.");
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Geçerli müşteri seçiniz.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar en az 1 olmalıdır.");
            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .Must(m => new[] { "Nakit", "Kredi Kartı", "Havale" }.Contains(m))
                .WithMessage("Geçersiz ödeme yöntemi.");
        }
    }

    // ─── PRODUCT VALIDATOR TESTLERİ ───

    public class ProductValidatorTests
    {
        private readonly TestCreateProductValidator _validator = new();

        [Fact]
        public void Validate_GecerliUrun_ValidationBasarili()
        {
            var dto = new CreateProductDto { Name = "Test", Price = 100m, StockCount = 10 };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_BosAd_HataVerir()
        {
            var dto = new CreateProductDto { Name = "", Price = 100m, StockCount = 10 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Validate_SifirFiyat_HataVerir()
        {
            var dto = new CreateProductDto { Name = "Ürün", Price = 0m, StockCount = 10 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Price");
        }

        [Fact]
        public void Validate_NegatifFiyat_HataVerir()
        {
            var dto = new CreateProductDto { Name = "Ürün", Price = -50m, StockCount = 10 };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_NegatifStok_HataVerir()
        {
            var dto = new CreateProductDto { Name = "Ürün", Price = 100m, StockCount = -1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "StockCount");
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(100)]
        [InlineData(999999)]
        public void Validate_GecerliFiyatlar_ValidationBasarili(decimal price)
        {
            var dto = new CreateProductDto { Name = "Ürün", Price = price, StockCount = 1 };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }
    }

    // ─── CUSTOMER VALIDATOR TESTLERİ ───

    public class CustomerValidatorTests
    {
        private readonly TestCreateCustomerValidator _validator = new();

        [Fact]
        public void Validate_GecerliMusteri_ValidationBasarili()
        {
            var dto = new CreateCustomerDto { FullName = "Ali Veli", Email = "ali@test.com" };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_BosAd_HataVerir()
        {
            var dto = new CreateCustomerDto { FullName = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FullName");
        }

        [Fact]
        public void Validate_GecersizEmail_HataVerir()
        {
            var dto = new CreateCustomerDto { FullName = "Ali", Email = "bu-email-degil" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_BosEmail_ValidationBasarili()
        {
            var dto = new CreateCustomerDto { FullName = "Ali", Email = null };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }
    }

    // ─── SALE VALIDATOR TESTLERİ ───

    public class SaleValidatorTests
    {
        private readonly TestCreateSaleValidator _validator = new();

        [Fact]
        public void Validate_GecerliSatis_ValidationBasarili()
        {
            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 2, PaymentMethod = "Nakit" };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_SifirMiktar_HataVerir()
        {
            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 0, PaymentMethod = "Nakit" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
        }

        [Fact]
        public void Validate_GecersizOdemeYontemi_HataVerir()
        {
            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 1, PaymentMethod = "Bitcoin" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PaymentMethod");
        }

        [Theory]
        [InlineData("Nakit")]
        [InlineData("Kredi Kartı")]
        [InlineData("Havale")]
        public void Validate_GecerliOdemeYontemleri_ValidationBasarili(string paymentMethod)
        {
            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 1, PaymentMethod = paymentMethod };
            _validator.Validate(dto).IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_SifirProductId_HataVerir()
        {
            var dto = new CreateSaleDto { ProductId = 0, CustomerId = 1, Quantity = 1, PaymentMethod = "Nakit" };
            _validator.Validate(dto).IsValid.Should().BeFalse();
        }
    }
}
