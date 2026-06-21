namespace Vehimap.Storage.Legacy;

public sealed class LegacyBackupException : InvalidOperationException
{
    public LegacyBackupException(string backupPath, string message, Exception innerException)
        : base(message, innerException)
    {
        BackupPath = backupPath;
    }

    public string BackupPath { get; }
}
