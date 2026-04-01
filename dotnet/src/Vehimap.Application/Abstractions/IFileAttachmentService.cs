namespace Vehimap.Application.Abstractions;

public interface IFileAttachmentService
{
    string ResolveManagedAttachmentPath(VehimapDataRoot dataRoot, string relativePath);
}
