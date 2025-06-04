namespace ECommerceNetApp.Domain.Authorization
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public class Permission(string action, string resource)
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        public string Action { get; } = action;

        public string Resource { get; } = resource;

        public override string ToString()
        {
            return $"{Action}:{Resource}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Permission other)
            {
                return Action == other.Action && Resource == other.Resource;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Action, Resource);
        }
    }
}
