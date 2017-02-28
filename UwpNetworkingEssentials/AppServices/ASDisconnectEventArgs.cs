namespace UwpNetworkingEssentials.AppServices
{
    public class ASDisconnectEventArgs : DisconnectEventArgsBase<ASConnection>
    {
        public ASDisconnectEventArgs(ASConnection connection, DisconnectReason reason) :
            base(connection, reason)
        {
        }
    }
}
