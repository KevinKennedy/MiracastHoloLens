using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.Events;

#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
using Windows.Media.Miracast;
#endif

[System.Serializable]
public class MiracastSourceCreatedEvent : UnityEvent<string>
{
}

public class MiracastMonitor : MonoBehaviour
{
    public MiracastSourceCreatedEvent OnMediaSourceCreated = new MiracastSourceCreatedEvent();

#if WINDOWS_UWP
    private MiracastReceiver miracastReceiver;
    private MiracastReceiverSession miracastSession;

    // Start is called before the first frame update
    private async void Start()
    {
        await this.InitializeMiracastAsync();
    }

    private async Task InitializeMiracastAsync()
    {
        this.miracastReceiver = new MiracastReceiver();
        this.miracastReceiver.StatusChanged += (receiver, o) => { this.Log($"StatusChanged: {receiver.GetStatus().ListeningStatus}"); };

        MiracastReceiverSettings settings = this.miracastReceiver.GetDefaultSettings();
        settings.FriendlyName += "Miracast Sample";
        settings.AuthorizationMethod = MiracastReceiverAuthorizationMethod.None;
        settings.RequireAuthorizationFromKnownTransmitters = false;
        MiracastReceiverApplySettingsResult applyResult = await this.miracastReceiver.DisconnectAllAndApplySettingsAsync(settings);
        this.Log($"DisconnectAllAndApplySettingsAsync={applyResult.Status}");

        CoreApplicationView mainView = CoreApplication.MainView;
        this.Log($"CoreApplication.MainView=  null  {(mainView == null ? "null" : mainView.ToString())}");

        this.miracastSession = await this.miracastReceiver.CreateSessionAsync(null /* mainView */);
        this.Log($"CreateSession={this.miracastSession}");
        miracastSession.AllowConnectionTakeover = true;
        this.miracastSession.ConnectionCreated += (session, args) => { this.Log($"ConnectionCreated {args.Connection.Transmitter.Name}"); };
        this.miracastSession.Disconnected += (session, args) => { this.Log($"Disconnected {args.Connection.Transmitter.Name}"); };
        this.miracastSession.MediaSourceCreated += MiracastSessionMediaSourceCreated;

        MiracastReceiverSessionStartResult startResult = await this.miracastSession.StartAsync();
        this.Log($"Session.Start={startResult.Status}");

    }

    private void MiracastSessionMediaSourceCreated(MiracastReceiverSession sender, MiracastReceiverMediaSourceCreatedEventArgs args)
    {
        this.Log($"args={args.MediaSource.Uri}");
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            this.OnMediaSourceCreated.Invoke(args.MediaSource.Uri.ToString());
        }, false);
    }

    private void OnDestroy()
    {
        if (this.miracastSession != null)
        {
            this.miracastSession.MediaSourceCreated -= MiracastSessionMediaSourceCreated;
            this.miracastSession.Dispose();
            this.miracastSession = null;
        }
    }

    private void Log(string msg, [CallerMemberName] string methodName = null)
    {
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "MiracastMonitor::{0}  {1}", methodName, msg);
    }
#endif
}
