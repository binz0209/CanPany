using CanPany.Domain.Entities;

namespace CanPany.Domain.Events;

public class ProposalAcceptedEvent : BaseDomainEvent
{
    public Proposal Proposal { get; }
    public string ProjectId { get; }
    public string FreelancerId { get; }
    public string ClientId { get; }

    public ProposalAcceptedEvent(Proposal proposal, string projectId, string freelancerId, string clientId)
    {
        Proposal = proposal;
        ProjectId = projectId;
        FreelancerId = freelancerId;
        ClientId = clientId;
    }
}

