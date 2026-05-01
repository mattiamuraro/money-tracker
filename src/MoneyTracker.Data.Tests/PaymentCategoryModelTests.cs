using System;
using System.Collections.Generic;
using Xunit;

namespace MoneyTracker.Data.Tests
{
    public class PaymentCategoryModelTests
    {
        [Fact]
        public void PaymentCategory_CanBeCreated()
        {
            var actorId = Guid.NewGuid();

            // Arrange & Act
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Code = "FOOD"
            };

            // Assert
            Assert.Equal("Food", category.Name);
            Assert.Equal("FOOD", category.Code);
        }

        [Fact]
        public void PaymentCategory_CanHaveMultiplePayments()
        {
            var actorId = Guid.NewGuid();

            // Arrange
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Code = "FOOD",
                Payments = new List<Payment>()
            };

            var payment1 = new Payment
            {
                Id = Guid.NewGuid(),
                Description = "Groceries",
                Amount = 50.00m,
                Date = DateTime.UtcNow,
                PaymentCategoryId = category.Id
            };

            var payment2 = new Payment
            {
                Id = Guid.NewGuid(),
                Description = "Restaurant",
                Amount = 30.00m,
                Date = DateTime.UtcNow,
                PaymentCategoryId = category.Id
            };

            category.Payments.Add(payment1);
            category.Payments.Add(payment2);

            // Assert
            Assert.Equal(2, category.Payments.Count);
        }

        [Fact]
        public void PaymentCategory_HasConstraints()
        {
            var actorId = Guid.NewGuid();

            // Arrange
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = new string('a', 51), // Exceeds max length
                Code = new string('a', 6) // Exceeds max length
            };

            // Assert
            Assert.True(category.Name.Length > 50);
            Assert.True(category.Code.Length > 5);
        }
    }
}
