using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

public class ProjectCreatedEvent : BaseDomainEvent
{
    public Project Project { get; }
    public string OwnerId { get; }

    public ProjectCreatedEvent(Project project, string ownerId)
    {
        Project = project;
        OwnerId = ownerId;
    }
}

