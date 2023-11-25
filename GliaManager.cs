using HP.Omnicept;
using HP.Omnicept.Messaging.Messages;

namespace VRCFTOmniceptModule;

public class GliaManager
{
    private Glia? m_gliaClient;
    private GliaLastValueCacheCustom? m_gliaValCache;

    public bool m_isConnected { get; private set; }
    public Action<HeartRate> OnHeartRate;

    public void StopGlia()
    {
        // Verify Glia is Disposed
        if (m_gliaValCache != null)
            m_gliaValCache?.Stop();
        if (m_gliaClient != null)
            m_gliaClient?.Dispose();
        m_gliaValCache = null;
        m_gliaClient = null;
        m_isConnected = false;
        Glia.cleanupNetMQConfig();
    }

    public bool StartGlia()
    {
        // Verify Glia is Disposed
        StopGlia();

        // Start Glia
        try
        {
            m_gliaClient = new Glia("VRCOSC_OmniceptHeartRate",
                new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
            m_gliaValCache = new GliaLastValueCacheCustom(m_gliaClient.Connection);
            SubscriptionList sl = new SubscriptionList
            {
                Subscriptions =
                {
                    new Subscription(MessageTypes.ABI_MESSAGE_HEART_RATE, String.Empty, String.Empty,
                        String.Empty, String.Empty, new MessageVersionSemantic("1.0.0"))
                }
            };
            m_gliaClient.setSubscriptions(sl);
            m_isConnected = true;
        }
        catch (Exception e)
        {
            m_isConnected = false;
        }
        m_gliaValCache!.OnHeartRate += (HeartTrack) => { OnHeartRate?.Invoke(HeartTrack); };
        return m_isConnected;
    }
}