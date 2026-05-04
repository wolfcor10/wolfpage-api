using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class WorkspaceMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? InvitedByUserId { get; set; }
    public WorkspaceMemberStatus Status { get; set; } = WorkspaceMemberStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
    public Workspace Workspace { get; set; } = default!;
    public User? InvitedByUser { get; set; }
}
