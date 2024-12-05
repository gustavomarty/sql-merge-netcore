namespace Contracts.Data.Data.Entities.Base
{
    public interface IBaseEntity
    {
        Guid Id { get; set; }
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? UpdateAt { get; set; }
        string? UpdateBy { get; set; }
    }
}
