namespace ApiTemplate.Application.Core.Entities;

public abstract class AuditableEntity : EntityBase
{
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
}
