using HP.Omnicept.Messaging.Messages;
using HP.Omnicept;
using HP.Omnicept.Messaging;
using VRCOSC.Game.Modules.Bases.Heartrate;


namespace Omnicept_HeartRateModule
{
    public class OmniceptHeartRateProvider : HeartrateProvider
    {
        private Glia? _gliaClient;
        private GliaValueCache? _gliaValCache;
        private bool isConnected;
        private CancellationTokenSource IsRunningCollector;
        public override bool IsConnected => isConnected;

        private readonly Omnicept_HeartRateModule module;
        public OmniceptHeartRateProvider(Omnicept_HeartRateModule omnicept_HeartRateModule)
        {
            this.module = omnicept_HeartRateModule;
        }
        public override void Initialise()
        {
            IsRunningCollector = new();
            if (!IsRunningCollector.IsCancellationRequested)
            {
                IsRunningCollector.Cancel();
            }
            if (StartGlia())
            {
                Task.Run(() => FetchDataLoop());
            }
            module.LogDebug("Omnicept Heart rate Initialised");
            Log("Initialised Omnicept Heart rate");
        }
        public override Task Teardown()
        {
            if (!IsRunningCollector.IsCancellationRequested)
            {
                IsRunningCollector.Cancel();
            }
            IsRunningCollector.Dispose();
            StopGlia();
            module.LogDebug("Omnicept Heart rate Teardown completed");
            Log("Teardown Omnicept Heart rate");
            return Task.CompletedTask;
        }


        async void FetchDataLoop()
        {
            while (!IsRunningCollector!.IsCancellationRequested)
            {
                UpdateMessage();
                try
                {
                    await Task.Delay(5000, IsRunningCollector.Token); //Heartrate monitor runs once every 5 seconds
                }
                catch(TaskCanceledException) { module.LogDebug("Omnicept Heart rate data fetching loop await task ended"); }
                catch(ObjectDisposedException) { module.LogDebug("Omnicept Heart rate Fetch data loop await task ended unexpectedly"); }
            }
            module.LogDebug("Omnicept Heart rate data fetching loop ended correctly");
        }


        public bool StartGlia()
        {
            // Verify Glia is Disposed
            StopGlia();

            // Start Glia
            try
            {
                _gliaClient = new Glia("VRCOSCOmniceptHeartRateModule", new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
                _gliaValCache = new GliaValueCache(_gliaClient.Connection);
                SubscriptionList sl = new() {
                    Subscriptions = 
                        {
                           new Subscription(MessageTypes.ABI_MESSAGE_HEART_RATE, String.Empty, String.Empty, String.Empty, String.Empty, new MessageVersionSemantic("1.0.0")),
                        }
                    };
                _gliaClient.setSubscriptions(sl);
                isConnected = true;
            }
            catch (Exception e)
            {
                isConnected = false;
                module.LogDebug(e.Message);
                //Log fail
            }
            return isConnected;
        }

        public void StopGlia()
        {
            // Verify Glia is Disposed
            isConnected = false;
            if (_gliaValCache != null)
                _gliaValCache?.Stop();
            if (_gliaClient != null)
                _gliaClient?.Dispose();
            _gliaValCache = null;
            _gliaClient = null;
            isConnected = false;
            Glia.cleanupNetMQConfig();
        }


        void HandleMessage(ITransportMessage msg)
        {
            switch (msg.Header.MessageType)
            {
                case MessageTypes.ABI_MESSAGE_HEART_RATE:
                    HeartRate heartRate = _gliaClient!.Connection.Build<HeartRate>(msg);
                    OnHeartrateUpdate!.Invoke((int)heartRate.Rate);
                    break;
            }
        }

        ITransportMessage? RetrieveMessage()
        {
            ITransportMessage? msg = null;
            if (_gliaValCache != null)
            {
                try
                {
                    msg = _gliaValCache.GetNext();
                }
                catch (HP.Omnicept.Errors.TransportError e)
                {
                    module.LogDebug("Glia transport error when recieving: " +  e.Message);
                    //OmniceptModule.logger?.LogError("[VRCFTOmniceptModule] Failed to start Glia! {E}", e);
                }
            }
            return msg;
        }
        public void UpdateMessage()
        {
            try
            {
                if (isConnected)
                {
                    ITransportMessage? msg = RetrieveMessage();
                    if (msg != null)
                        HandleMessage(msg);
                }
            }
            catch (Exception e)
            {
                module.LogDebug("Glia transport error: " + e.Message);
                //Throw error
            }
        }
    }
}