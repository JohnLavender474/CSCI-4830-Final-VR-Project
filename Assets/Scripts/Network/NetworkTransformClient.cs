using Unity.Netcode.Components;

namespace Network
{
    public class NetworkTransformClient : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}