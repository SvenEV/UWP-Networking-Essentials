namespace UwpNetworkingEssentials.Channels
{
    public interface IConnectionReceiveEventArgs
    {
        IConnectionListener Listener { get; }
    }
}
