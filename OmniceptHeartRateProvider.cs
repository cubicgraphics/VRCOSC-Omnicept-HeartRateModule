using HP.Omnicept.Messaging.Messages;
using VRCOSC.Game.Modules.Bases.Heartrate;
using VRCFTOmniceptModule;


namespace Omnicept_HeartRateModule
{
    public class OmniceptHeartRateProvider : HeartrateProvider
    {
        private readonly GliaManager _gliaManager;
        public override bool IsConnected => _gliaManager.m_isConnected;

        private readonly Omnicept_HeartRateModule module;
        public OmniceptHeartRateProvider(Omnicept_HeartRateModule omnicept_HeartRateModule)
        {
            _gliaManager = new GliaManager();
            this.module = omnicept_HeartRateModule;
        }
        public override void Initialise()
        {
            _gliaManager.StartGlia();
            _gliaManager.OnHeartRate += UpdateHeartRate;
            module.LogDebug("Omnicept Heart rate Initialised");
            Log("Initialised Omnicept Heart rate");
        }
        public override Task Teardown()
        {
            _gliaManager.StopGlia();
            module.LogDebug("Omnicept Heart rate Teardown completed");
            Log("Teardown Omnicept Heart rate");
            return Task.CompletedTask;
        }
        
        private void UpdateHeartRate(HeartRate heartRate)
        {
            OnHeartrateUpdate?.Invoke((int)heartRate.Rate);
        }
    }
}