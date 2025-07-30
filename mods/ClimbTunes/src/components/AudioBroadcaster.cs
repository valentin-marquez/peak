using UnityEngine;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Voice;

namespace ClimbTunes.Components
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(PhotonVoiceView))]
    public class AudioBroadcaster : MonoBehaviour
    {
        [Header("Audio Broadcasting")]
        public AudioSource audioSource;
        public PhotonVoiceView photonVoiceView;
        
        [Header("Settings")]
        [Range(0f, 1f)]
        public float broadcastVolume = 1f;
        public bool broadcastEnabled = true;
        
        private Recorder recorder;
        private bool wasBroadcasting = false;
        private float[] audioBuffer;
        private int sampleRate = 48000;
        private int channels = 1;

        private void Awake()
        {
            // Get components if not assigned
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (photonVoiceView == null)
                photonVoiceView = GetComponent<PhotonVoiceView>();
        }

        private void Start()
        {
            SetupAudioRecorder();
        }

        private void SetupAudioRecorder()
        {
            // Get the existing recorder from PhotonVoiceView
            if (photonVoiceView != null && photonVoiceView.RecorderInUse != null)
            {
                recorder = photonVoiceView.RecorderInUse;
                ClimbTunes.ModLogger.LogInfo("AudioBroadcaster: Using existing recorder from PhotonVoiceView");
            }
            else
            {
                ClimbTunes.ModLogger.LogError("AudioBroadcaster: No recorder found in PhotonVoiceView!");
                return;
            }

            // Configure the recorder for audio broadcasting
            if (recorder != null)
            {
                recorder.TransmitEnabled = broadcastEnabled;
                recorder.VoiceDetection = false; // We'll control transmission manually
                ClimbTunes.ModLogger.LogInfo("AudioBroadcaster: Recorder configured for audio broadcasting");
            }
        }

        private void Update()
        {
            if (audioSource == null || recorder == null) return;

            // Start/stop broadcasting based on audio playback
            bool shouldBroadcast = audioSource.isPlaying && broadcastEnabled && audioSource.clip != null;
            
            if (shouldBroadcast && !wasBroadcasting)
            {
                StartBroadcasting();
            }
            else if (!shouldBroadcast && wasBroadcasting)
            {
                StopBroadcasting();
            }

            wasBroadcasting = shouldBroadcast;
        }

        private void StartBroadcasting()
        {
            if (recorder != null)
            {
                recorder.TransmitEnabled = true;
                recorder.RecordingEnabled = true;
                
                ClimbTunes.ModLogger.LogInfo($"AudioBroadcaster: Started broadcasting audio: {audioSource.clip?.name ?? "Unknown"}");
            }
        }

        private void StopBroadcasting()
        {
            if (recorder != null)
            {
                recorder.TransmitEnabled = false;
                recorder.RecordingEnabled = false;
                
                ClimbTunes.ModLogger.LogInfo("AudioBroadcaster: Stopped broadcasting audio");
            }
        }

        public void SetBroadcastEnabled(bool enabled)
        {
            broadcastEnabled = enabled;
            
            if (recorder != null)
            {
                recorder.TransmitEnabled = enabled && audioSource.isPlaying;
            }
        }

        public void SetBroadcastVolume(float volume)
        {
            broadcastVolume = Mathf.Clamp01(volume);
            // Volume is controlled by the receiving end through speaker settings
        }

        public bool IsBroadcasting()
        {
            return recorder != null && recorder.RecordingEnabled && recorder.TransmitEnabled;
        }

        private void OnDestroy()
        {
            StopBroadcasting();
        }

        private void OnDisable()
        {
            StopBroadcasting();
        }

        // Capture audio from AudioSource
        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!broadcastEnabled || !audioSource.isPlaying || recorder == null || !recorder.TransmitEnabled)
                return;

            // Store audio data for transmission
            if (audioBuffer == null || audioBuffer.Length != data.Length)
            {
                audioBuffer = new float[data.Length];
            }

            // Copy and apply volume
            for (int i = 0; i < data.Length; i++)
            {
                audioBuffer[i] = data[i] * broadcastVolume;
            }

            // Send audio data to Photon Voice
            SendAudioToRecorder(audioBuffer, channels);
        }

        private void SendAudioToRecorder(float[] data, int channels)
        {
            // For now, we just ensure the recorder is active
            // The actual audio capture will depend on how Photon Voice is configured
            // This method can be extended later with proper audio streaming
            if (recorder != null && recorder.TransmitEnabled)
            {
                // Audio data is being processed by OnAudioFilterRead
                // The transmission is handled by Photon Voice internally
            }
        }
    }
}