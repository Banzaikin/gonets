using CommonBackend.Domain.Entities;
using CommonBackend.Domain.Enums;

namespace CommonBackend.Application.Interfaces;

public interface IChannelStateService
{
    void Update(ChannelState state);
    IReadOnlyDictionary<ChannelType, ChannelState> GetAll();
}