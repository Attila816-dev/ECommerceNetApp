namespace ECommerceNetApp.Domain.ValueObjects
{
    public record Money
    {
        private const string DefaultCurrency = "EUR";

        public Money(decimal amount, string? currency = null)
        {
            if (amount < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(amount));
            }

            Amount = amount;
            Currency = currency ?? DefaultCurrency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private Money()
        {
            Amount = 0;
            Currency = DefaultCurrency;
        }

        public decimal Amount { get; init; }

        public string Currency { get; init; } = DefaultCurrency;

        public static Money operator +(Money left, Money right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);

            if (!left.Currency.Equals(right.Currency, StringComparison.Ordinal))
            {
                throw new ArgumentException("Currencies must match for addition");
            }

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator *(Money money, int multiplier)
        {
            ArgumentNullException.ThrowIfNull(money);
            return new Money(money.Amount * multiplier, money.Currency);
        }

        public static Money Add(Money left, Money right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);
            return left + right;
        }

        public static Money Multiply(Money money, int multiplier)
        {
            ArgumentNullException.ThrowIfNull(money);
            return money * multiplier;
        }

        public static Money From(decimal amount) => new Money(amount);

        // With records, .Equals, GetHashCode are automatically implemented
        public override string ToString()
        {
            return $"{Amount:C} {Currency}";
        }
    }
}
