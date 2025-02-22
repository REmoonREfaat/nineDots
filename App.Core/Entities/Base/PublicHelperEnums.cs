namespace App.Core.Entities.Base
{
    [Flags]
    public enum RecordStatus : short
    {
        Enabled = 0,
        Disabled = 1,
        Deleted = 2,
        Blocked = 3,
        Deactivated = 4,
        Archived = 5,

    }
}
