namespace ECommerceNetApp.Domain.ValueObjects
{
    public record Money
    {
        private const string DefaultCurrency = "EUR";

        private Money(decimal amount, string? currency = null)
        {
            Amount = amount;
            Currency = currency ?? DefaultCurrency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private Money()
            : this(0)
        {
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

            return Create(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator *(Money money, int multiplier)
        {
            ArgumentNullException.ThrowIfNull(money);
            return Create(money.Amount * multiplier, money.Currency);
        }

        public static Money Create(decimal amount, string? currency = null)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Price cannot be negative");
            }

            return new Money(amount, currency);
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

        public static Money From(decimal amount)
            => Create(amount);

        // With records, .Equals, GetHashCode are automatically implemented
        public override string ToString()
        {
            return $"{Amount:C} {Currency}";
        }
    }
}
