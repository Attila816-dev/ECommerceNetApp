namespace ECommerceNetApp.Domain.ValueObjects
{
    public class Money : IEquatable<Money>
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
        }

        public decimal Amount { get; }

        public string Currency { get; } = DefaultCurrency; // Default currency

        public static Money operator +(Money left, Money right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);

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

        public bool Equals(Money? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }
    }
}
