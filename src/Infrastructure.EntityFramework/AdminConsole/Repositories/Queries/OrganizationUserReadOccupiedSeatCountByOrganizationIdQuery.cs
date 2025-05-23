﻿using Bit.Core.Enums;
using Bit.Infrastructure.EntityFramework.Models;

namespace Bit.Infrastructure.EntityFramework.Repositories.Queries;

public class OrganizationUserReadOccupiedSeatCountByOrganizationIdQuery : IQuery<OrganizationUser>
{
    private readonly Guid _organizationId;

    public OrganizationUserReadOccupiedSeatCountByOrganizationIdQuery(Guid organizationId)
    {
        _organizationId = organizationId;
    }

    public IQueryable<OrganizationUser> Run(DatabaseContext dbContext)
    {
        var orgUsersQuery = from ou in dbContext.OrganizationUsers
                            where ou.OrganizationId == _organizationId && ou.Status >= OrganizationUserStatusType.Invited
                            select new OrganizationUser { Id = ou.Id, OrganizationId = ou.OrganizationId, Status = ou.Status };

        // As of https://bitwarden.atlassian.net/browse/PM-17772, a seat is also occupied by a Families for Enterprise sponsorship sent by an
        // organization admin, even if the user sent the invitation doesn't have a corresponding OrganizationUser in the Enterprise organization.
        var sponsorshipsQuery = from os in dbContext.OrganizationSponsorships
                                where os.SponsoringOrganizationId == _organizationId &&
                                      os.IsAdminInitiated &&
                                      !os.ToDelete
                                select new OrganizationUser
                                {
                                    Id = os.Id,
                                    OrganizationId = _organizationId,
                                    Status = OrganizationUserStatusType.Invited
                                };

        return orgUsersQuery.Concat(sponsorshipsQuery);
    }
}
