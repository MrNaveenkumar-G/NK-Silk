namespace NKSilk.Application.Common.Interfaces;

/// <summary>
/// Resolves a stored media path to a delivery URL. When a CDN base is configured, relative
/// asset paths are rewritten to be served from the CDN; otherwise paths are returned as-is.
/// </summary>
public interface IMediaUrlResolver
{
    string Resolve(string? path);
}
