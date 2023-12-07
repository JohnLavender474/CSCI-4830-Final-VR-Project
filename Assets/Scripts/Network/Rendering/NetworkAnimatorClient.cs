using Unity.Netcode.Components;

namespace Network
{
    public class NetworkAnimatorClient : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}