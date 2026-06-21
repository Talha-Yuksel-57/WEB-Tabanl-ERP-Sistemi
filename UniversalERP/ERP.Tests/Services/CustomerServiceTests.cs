using AutoMapper;
using ERP.Core.DTOs.Customer;
using ERP.Core.Entities;
using ERP.Core.Interfaces;
using ERP.Data.Services;
using ERP.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW;
        private readonly Mock<IRepository<Customer>> _mockRepo;
        private readonly IMapper _mapper;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _mockUoW = new Mock<IUnitOfWork>();
            _mockRepo = new Mock<IRepository<Customer>>();
            _mapper = MapperHelper.Create();

            _mockUoW.Setup(u => u.Customers).Returns(_mockRepo.Object);
            _service = new CustomerService(_mockUoW.Object, _mapper);
        }

        [Fact]
        public async Task GetAllAsync_MusteriListesiDoner()
        {
            // Arrange
            var customers = new List<Customer>
            {
                TestDataFactory.CreateCustomer(1, "Ahmet Yılmaz"),
                TestDataFactory.CreateCustomer(2, "Fatma Demir")
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.FullName == "Ahmet Yılmaz");
        }

        [Fact]
        public async Task CreateAsync_GecerliDto_MusteriOlusturulur()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                FullName = "Yeni Müşteri",
                Email = "yeni@test.com",
                Phone = "05551234567"
            };

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Customer>())).Returns(Task.CompletedTask);
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("Yeni Müşteri");
            result.Email.Should().Be("yeni@test.com");
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_GecerliMusteri_SoftDeleteYapar()
        {
            // Arrange
            var customer = TestDataFactory.CreateCustomer(1);
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockRepo.Setup(r => r.Update(It.IsAny<Customer>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            customer.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_GecerliMusteri_Guncellenir()
        {
            // Arrange
            var customer = TestDataFactory.CreateCustomer(1, "Eski Ad");
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _mockRepo.Setup(r => r.Update(It.IsAny<Customer>()));
            _mockUoW.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            var dto = new UpdateCustomerDto
            {
                FullName = "Yeni Ad",
                Email = "yeni@email.com"
            };

            // Act
            var result = await _service.UpdateAsync(1, dto);

            // Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Yeni Ad");
        }
    }
}
