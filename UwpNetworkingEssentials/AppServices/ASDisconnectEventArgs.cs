namespace UwpNetworkingEssentials.AppServices
{
    public class ASDisconnectEventArgs : DisconnectEventArgsBase<ASConnection>
    {
        public ASDisconnectEventArgs(ASConnection connection, ConnectionCloseReason reason) :
            base(connection, reason)
        {
        }
    }
}
