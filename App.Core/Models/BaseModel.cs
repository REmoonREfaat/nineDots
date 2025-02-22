using App.Core.Entities.Base;

namespace App.Core.Models
{
    public class BaseModel
    {
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
