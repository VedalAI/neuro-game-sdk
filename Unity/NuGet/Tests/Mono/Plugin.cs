using BepInEx;
using BepInEx.Unity.Mono;
using NeuroSdk;

namespace Mono
{
    [BepInPlugin("Mono", "Mono", "1.0.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            NeuroSdkSetup.Initialize("Mono");
        }
    }
}
