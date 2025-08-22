using BepInEx;
using BepInEx.Unity.IL2CPP;
using NeuroSdk;

namespace Il2Cpp
{
    [BepInPlugin("Il2Cpp", "Il2Cpp", "1.0.0")]
    // ReSharper disable once UnusedType.Global UnusedMember.Global
    public sealed class Plugin : BasePlugin
    {
        public override void Load()
        {
            NeuroSdkSetup.Initialize("Il2Cpp");
        }
    }
}
