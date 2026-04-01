namespace Vehimap.Domain.Models;

public sealed record ManagedAttachment(
    string RelativePath,
    byte[] Content);
