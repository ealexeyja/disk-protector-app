using System.IO;

namespace DiskProtectorApp
{
    public class DriveInfoEx
    {
        public DriveInfo Drive { get; }
        public bool IsSystemDrive { get; }

        public DriveInfoEx(DriveInfo drive, bool isSystemDrive)
        {
            Drive = drive;
            IsSystemDrive = isSystemDrive;
        }
    }
}
