using System;

namespace UwpNetworkingEssentials
{
    public class ResponseAlreadySentException : InvalidOperationException
    {
        public ResponseAlreadySentException()
            : base("The request has already been responded to. Further responses cannot be sent.")
        {
        }
    }
}
