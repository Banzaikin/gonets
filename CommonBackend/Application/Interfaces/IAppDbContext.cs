using CommonBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommonBackend.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<IncomingMessage> IncomingMessages { get; set; }
    DbSet<OutgoingMessage> OutgoingMessages { get; set; }
    DbSet<SentMessage> SentMessages { get; set; }
    DbSet<User> Users { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}