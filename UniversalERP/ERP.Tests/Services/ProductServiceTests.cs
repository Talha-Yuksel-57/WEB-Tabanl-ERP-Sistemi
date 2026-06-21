using AutoMapper;
using ERP.Core.DTOs.Product;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using ERP.Data.Services;
using ERP.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW;
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly IMapper _mapper;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockUoW = new Mock<IUnitOfWork>();
            _mockRepo = new Mock<IRepository<Product>>();
            _mapper = MapperHelper.Create();

            _mockUoW.Setup(u => u.Products).Returns(_mockRepo.Object);
            _service = new ProductService(_mockUoW.Object, _mapper);
        }

        // ─────────────────────────────────────────
        // GetAllAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_VarikenUrunlerMevcutsa_UrunListesiDoner()
        {
            // Arrange
            var products = new List<Product>
            {
                TestDataFactory.CreateProduct(1, "Laptop", 15000m, 10),
                TestDataFactory.CreateProduct(2, "Mouse", 500m, 30)
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "Laptop");
            result.Should().Contain(p => p.Name == "Mouse");
        }

        [Fact]
        public async Task GetAllAsync_HicUrunYoksa_BosListeDoner()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ─────────────────────────────────────────
        // GetByIdAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_GecerliIdVerilince_UrunDoner()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, "iPhone", 60000m);
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("iPhone");
            result.Price.Should().Be(60000m);
        }

        [Fact]
        public async Task GetByIdAsync_YanlisIdVerilince_NullDoner()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.GetByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        // ─────────────────────────────────────────
        // CreateAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_GecerliDtoIle_UrunOlusturulur()
        {
            // Arrange
            var dto = new CreateProductDto
            {
                Name = "Yeni Ürün",
                Price = 250m,
                StockCount = 100,
                MinStockLevel = 10
            };

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Yeni Ürün");
            result.Price.Should().Be(250m);
            result.StockCount.Should().Be(100);

            // Repository ve UoW'nin çağrıldığını doğrula
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockUoW.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_YeniUrun_IsActiveTrue_OlarakOlusur()
        {
            // Arrange
            var dto = new CreateProductDto { Name = "Ürün", Price = 100m, StockCount = 5 };
            Product? capturedProduct = null;

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Callback<Product>(p => capturedProduct = p)
                .Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _service.CreateAsync(dto);

            // Assert — IsActive otomatik true olmalı
            capturedProduct.Should().NotBeNull();
            capturedProduct!.IsActive.Should().BeTrue();
        }

        // ─────────────────────────────────────────
        // UpdateAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_GecerliIdIle_UrunGuncellenir()
        {
            // Arrange
            var existing = TestDataFactory.CreateProduct(1, "Eski Ad", 100m);
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _mockRepo.Setup(r => r.Update(It.IsAny<Product>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            var updateDto = new UpdateProductDto
            {
                Name = "Yeni Ad",
                Price = 200m,
                MinStockLevel = 10,
                IsActive = true
            };

            // Act
            var result = await _service.UpdateAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Yeni Ad");
            result.Price.Should().Be(200m);
            _mockRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_YanlisIdIle_NullDoner()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.UpdateAsync(999, new UpdateProductDto { Name = "X", Price = 1m });

            // Assert
            result.Should().BeNull();
            _mockRepo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        }

        // ─────────────────────────────────────────
        // DeleteAsync (Soft Delete) Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_GecerliUrun_SoftDeleteYapar()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1);
            product.IsDeleted = false;

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockRepo.Setup(r => r.Update(It.IsAny<Product>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            product.IsDeleted.Should().BeTrue();  // Gerçekten silinmedi, sadece işaretlendi
            _mockRepo.Verify(r => r.Update(It.Is<Product>(p => p.IsDeleted == true)), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_YanlisId_FalseDoner()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            // Act
            var result = await _service.DeleteAsync(99);

            // Assert
            result.Should().BeFalse();
        }

        // ─────────────────────────────────────────
        // UpdateStockAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task UpdateStockAsync_GecerliUrun_StokGuncellenir()
        {
            // Arrange
            var product = TestDataFactory.CreateProduct(1, stock: 10);
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _mockRepo.Setup(r => r.Update(It.IsAny<Product>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.UpdateStockAsync(1, 99);

            // Assert
            result.Should().BeTrue();
            product.StockCount.Should().Be(99);
        }

        // ─────────────────────────────────────────
        // GetLowStockAsync Testleri
        // ─────────────────────────────────────────

        [Fact]
        public async Task GetLowStockAsync_KritikStokluUrunlerDoner()
        {
            // Arrange — stok 3, min 5 → kritik
            var lowStock = TestDataFactory.CreateProduct(1, "Az Kalan", stock: 3, minStock: 5);
            _mockRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { lowStock });

            // Act
            var result = await _service.GetLowStockAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().IsLowStock.Should().BeTrue();
        }
    }
}
