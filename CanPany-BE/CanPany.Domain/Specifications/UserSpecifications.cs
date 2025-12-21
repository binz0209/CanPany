using CanPany.Domain.Entities;

namespace CanPany.Domain.Specifications;

public static class UserSpecifications
{
    public static ISpecification<User> IsActive()
    {
        return new ActiveUserSpecification();
    }

    public static ISpecification<User> HasRole(string role)
    {
        return new HasRoleSpecification(role);
    }

    public static ISpecification<User> IsVerified()
    {
        return new VerifiedUserSpecification();
    }
}

internal class ActiveUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        // Assuming User has an IsActive or Status property
        // Adjust based on actual User entity structure
        return true; // Placeholder - adjust based on actual User entity
    }
}

internal class HasRoleSpecification : ISpecification<User>
{
    private readonly string _role;

    public HasRoleSpecification(string role)
    {
        _role = role;
    }

    public bool IsSatisfiedBy(User entity)
    {
        return entity.Role == _role;
    }
}

internal class VerifiedUserSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User entity)
    {
        // Adjust based on actual User entity structure
        return true; // Placeholder
    }
}

