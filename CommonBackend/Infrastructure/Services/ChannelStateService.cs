using CommonBackend.Application.Interfaces;
using CommonBackend.Domain.Entities;
using CommonBackend.Domain.Enums;

public class ChannelStateService : IChannelStateService
{
    private readonly Dictionary<ChannelType, ChannelState> _states = new();

    public void Update(ChannelState state)
    {
        _states[state.Channel] = state;
    }

    public IReadOnlyDictionary<ChannelType, ChannelState> GetAll()
    {
        return _states;
    }
}