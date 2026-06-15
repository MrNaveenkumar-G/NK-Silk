namespace NKSilk.Application.Common.Interfaces;

/// <summary>Ambient accessor for the acting user, used for audit attribution.</summary>
public interface ICurrentUser
{
    int? CustomerId { get; }
    string Name { get; }
}
