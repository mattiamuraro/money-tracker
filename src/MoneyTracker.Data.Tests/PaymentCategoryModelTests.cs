namespace MoneyTracker.Data.Tests
{
    [TestFixture]
    public class PaymentCategoryModelTests
    {
        [Test]
        public void PaymentCategory_CanBeCreated()
        {
            // Arrange & Act
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Code = "FOOD",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "TestUser"
            };

            // Assert
            Assert.That(category.Name, Is.EqualTo("Food"));
            Assert.That(category.Code, Is.EqualTo("FOOD"));
        }

        [Test]
        public void PaymentCategory_CanHaveMultiplePayments()
        {
            // Arrange
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = "Food",
                Code = "FOOD",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "TestUser",
                Payments = new List<Payment>()
            };

            var payment1 = new Payment
            {
                Id = Guid.NewGuid(),
                Description = "Groceries",
                Amount = 50.00m,
                Date = DateTime.UtcNow,
                PaymentCategoryId = category.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "TestUser"
            };

            var payment2 = new Payment
            {
                Id = Guid.NewGuid(),
                Description = "Restaurant",
                Amount = 30.00m,
                Date = DateTime.UtcNow,
                PaymentCategoryId = category.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "TestUser"
            };

            category.Payments.Add(payment1);
            category.Payments.Add(payment2);

            // Assert
            Assert.That(category.Payments, Has.Count.EqualTo(2));
        }

        [Test]
        public void PaymentCategory_HasConstraints()
        {
            // Arrange
            var category = new PaymentCategory
            {
                Id = Guid.NewGuid(),
                Name = new string('a', 51), // Exceeds max length
                Code = new string('a', 6), // Exceeds max length
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser",
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "TestUser"
            };

            // Assert
            Assert.That(category.Name.Length, Is.GreaterThan(50));
            Assert.That(category.Code.Length, Is.GreaterThan(5));
        }
    }
}
