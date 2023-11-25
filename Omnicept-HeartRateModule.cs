using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Bases.Heartrate;

namespace Omnicept_HeartRateModule
{
    [ModuleTitle("Omnicept_HeartRate")]
    [ModuleDescription("Gets heart rate from the HpOmnicept")]
    [ModuleAuthor("cubic")]
    [ModuleGroup(ModuleType.General)]
    public partial class Omnicept_HeartRateModule : HeartrateModule<OmniceptHeartRateProvider>
    {
        public Omnicept_HeartRateModule()
        {
            
        }

        protected override void CreateAttributes()
        {
            base.CreateAttributes();
        }

        protected override void OnModuleStart()
        {
            base.OnModuleStart();
        }

        protected override void OnModuleStop()
        {
            base.OnModuleStop();
        }

        internal new void LogDebug(string message)
        {
            base.LogDebug(message);
        }

        protected override OmniceptHeartRateProvider CreateProvider()
        {
            LogDebug("Creating provider");
            var provider = new OmniceptHeartRateProvider(this);
            return provider;
        }

        private enum Omnicept_HeartRateSetting
        {
        }

        private enum Omnicept_HeartRateParameter
        {
        }
    }
}
