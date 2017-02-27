namespace UwpNetworkingEssentials
{
    public interface IObjectSerializer
    {
        string Serialize(object o);

        object Deserialize(string s);
    }
}
