using AutoMapper;
using ERP.Core.DTOs.Sale;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using ERP.Data.Services;
using ERP.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Services
{
    public class SaleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW;
        private readonly Mock<IRepository<Product>> _mockProductRepo;
        private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
        private readonly Mock<IRepository<Sale>> _mockSaleRepo;
        private readonly Mock<IAuditLogService> _mockAuditLog;
        private readonly Mock<INotificationService> _mockNotification;
        private readonly IMapper _mapper;
        private readonly SaleService _service;

        public SaleServiceTests()
        {
            _mockUoW = new Mock<IUnitOfWork>();
            _mockProductRepo = new Mock<IRepository<Product>>();
            _mockCustomerRepo = new Mock<IRepository<Customer>>();
            _mockSaleRepo = new Mock<IRepository<Sale>>();
            _mockAuditLog = new Mock<IAuditLogService>();
            _mockNotification = new Mock<INotificationService>();
            _mapper = MapperHelper.Create();

            _mockUoW.Setup(u => u.Products).Returns(_mockProductRepo.Object);
            _mockUoW.Setup(u => u.Customers).Returns(_mockCustomerRepo.Object);
            _mockUoW.Setup(u => u.Sales).Returns(_mockSaleRepo.Object);

            _service = new SaleService(
                _mockUoW.Object,
                _mapper,
                _mockAuditLog.Object,
                _mockNotification.Object);
        }

        // ─────────────────────────────────────────
        // CreateSaleAsync — ACID Transaction Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task CreateSaleAsync_YeterliStokVarsa_SatisOlusturulur()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, "Laptop", 5000m, stock: 20, minStock: 5);
            var customer = TestDataFactory.CreateCustomer(1, "Ali Veli");

            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockCustomerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockSaleRepo.Setup(r => r.AddAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mockAuditLog.Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var dto = new CreateSaleDto
            {
                ProductId = 1,
                CustomerId = 1,
                Quantity = 3,
                PaymentMethod = "Nakit"
            };

            // Act
            var result = await _service.CreateSaleAsync(dto, cashierId: 1);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(15000m);    // 5000 * 3
            result.ProductName.Should().Be("Laptop");
            result.CustomerName.Should().Be("Ali Veli");
        }

        [Fact]
        public async Task CreateSaleAsync_SatistanSonra_StokDuser()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, stock: 20, minStock: 5);
            var customer = TestDataFactory.CreateCustomer(1);

            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockCustomerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockSaleRepo.Setup(r => r.AddAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mockAuditLog.Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 5, PaymentMethod = "Nakit" };

            // Act
            await _service.CreateSaleAsync(dto, cashierId: 1);

            // Assert — stok 20'den 15'e düşmeli
            product.StockCount.Should().Be(15);
            _mockProductRepo.Verify(r => r.Update(It.Is<Product>(p => p.StockCount == 15)), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_YetersizStok_InvalidOperationException_Firlatir()
        {
            // Arrange — stokta 3 var, 10 isteniyor
            var product = TestDataFactory.CreateProduct(1, stock: 3, minStock: 5);
            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 10, PaymentMethod = "Nakit" };

            // Act & Assert
            var act = async () => await _service.CreateSaleAsync(dto, cashierId: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Yetersiz stok*");
        }

        [Fact]
        public async Task CreateSaleAsync_UrünYok_InvalidOperationException_Firlatir()
        {
            // Arrange
            _mockProductRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            var dto = new CreateSaleDto { ProductId = 99, CustomerId = 1, Quantity = 1, PaymentMethod = "Nakit" };

            // Act & Assert
            var act = async () => await _service.CreateSaleAsync(dto, cashierId: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*bulunamadı*");
        }

        [Fact]
        public async Task CreateSaleAsync_YetersizStok_SatisKaydiOlusturulmaz()
        {
            // Arrange — stok yetersiz
            var product = TestDataFactory.CreateProduct(1, stock: 2);
            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = 5, PaymentMethod = "Nakit" };

            // Act
            try { await _service.CreateSaleAsync(dto, cashierId: 1); } catch { }

            // Assert — exception fırlatıldı, satış kaydı eklenmedi
            _mockSaleRepo.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Never);
            _mockUoW.Verify(u => u.CompleteAsync(), Times.Never);
        }

        // ─────────────────────────────────────────
        // CancelSaleAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task CancelSaleAsync_TamamlanmisSatis_IptalEdilir_StokIadeEdilir()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, stock: 15);
            var sale = TestDataFactory.CreateSale(1, productId: 1, quantity: 3, status: "Completed");

            _mockSaleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);
            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockSaleRepo.Setup(r => r.Update(It.IsAny<Sale>()));
            _mockProductRepo.Setup(r => r.Update(It.IsAny<Product>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mockAuditLog.Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CancelSaleAsync(1);

            // Assert
            result.Should().BeTrue();
            sale.Status.Should().Be("Cancelled");
            product.StockCount.Should().Be(18);  // 15 + 3 iade
        }

        [Fact]
        public async Task CancelSaleAsync_ZatenIptalEdilmisSatis_FalseDoner()
        {
            // Arrange
            var sale = TestDataFactory.CreateSale(1, status: "Cancelled");
            _mockSaleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);

            // Act
            var result = await _service.CancelSaleAsync(1);

            // Assert
            result.Should().BeFalse();
            _mockUoW.Verify(u => u.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CancelSaleAsync_YanlisId_FalseDoner()
        {
            // Arrange
            _mockSaleRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Sale?)null);

            // Act
            var result = await _service.CancelSaleAsync(99);

            // Assert
            result.Should().BeFalse();
        }

        // ─────────────────────────────────────────
        // TotalAmount Hesaplama Testleri
        // ─────────────────────────────────────────

        [Theory]
        [InlineData(100m, 3, 300m)]   // Fiyat * Miktar
        [InlineData(50.5m, 4, 202m)]
        [InlineData(999.99m, 1, 999.99m)]
        public async Task CreateSaleAsync_ToplamTutar_DogruHesaplanir(
            decimal price, int quantity, decimal expected)
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, price: price, stock: 100);
            var customer = TestDataFactory.CreateCustomer(1);

            _mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockCustomerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockSaleRepo.Setup(r => r.AddAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
            _mockAuditLog.Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<object?>(), It.IsAny<object?>(), It.IsAny<string?>()))
                .Returns(Task.CompletedTask);

            var dto = new CreateSaleDto { ProductId = 1, CustomerId = 1, Quantity = quantity, PaymentMethod = "Nakit" };

            // Act
            var result = await _service.CreateSaleAsync(dto, cashierId: 1);

            // Assert
            result.TotalAmount.Should().Be(expected);
        }
    }
}
