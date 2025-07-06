using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Photon.Pun;
using BepInEx.Logging;

namespace ClimbTunes
{
    /// <summary>
    /// Component that handles radio functionality including YouTube audio playback
    /// </summary>
    public class ClimbTunesRadio : MonoBehaviourPunPV, IPunObservable
    {
        [Header("Radio Settings")]
        public float volume = 0.7f;
        public float range = 10f;
        public bool isPlaying = false;
        public bool isPaused = false;

        [Header("Audio Components")]
        public AudioSource audioSource;
        public AudioClip currentClip;

        [Header("Visual Components")]
        public MeshRenderer radioRenderer;
        public Light radioLight;
        public ParticleSystem musicParticles;

        [Header("Current Track Info")]
        public string currentURL = "";
        public string currentTitle = "No Track";
        public float currentTime = 0f;
        public float totalTime = 0f;

        // YouTube-dl or similar service integration
        private static readonly string YOUTUBE_API_SERVICE = "https://api.youtube.com/v1/"; // Placeholder
        private Coroutine currentPlaybackCoroutine;
        private List<string> playlist = new List<string>();
        private int currentPlaylistIndex = 0;

        // Audio visualization
        private float[] audioSpectrum = new float[256];
        private float bassLevel = 0f;
        private float midLevel = 0f;
        private float trebleLevel = 0f;

        // Radio state synchronization
        private bool needsSync = false;
        private float syncTimer = 0f;
        private const float SYNC_INTERVAL = 5f; // Sync every 5 seconds

        public void Initialize(float initialVolume)
        {
            volume = initialVolume;
            SetupAudioSource();
            SetupVisualComponents();
            SetupNetworking();

            ClimbTunesPlugin.Logger.LogInfo($"Radio initialized at position {transform.position}");
        }

        private void SetupAudioSource()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.maxDistance = range;
            audioSource.minDistance = 1f;
            audioSource.loop = false;
            audioSource.playOnAwake = false;
        }

        private void SetupVisualComponents()
        {
            // Setup radio material
            if (radioRenderer == null)
                radioRenderer = GetComponent<MeshRenderer>();

            if (radioRenderer != null)
            {
                Material radioMaterial = new Material(Shader.Find("Standard"));
                radioMaterial.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                radioMaterial.SetFloat("_Metallic", 0.5f);
                radioMaterial.SetFloat("_Smoothness", 0.3f);
                radioRenderer.material = radioMaterial;
            }

            // Setup indicator light
            GameObject lightObj = new GameObject("RadioLight");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = new Vector3(0, 0.1f, 0.2f);

            radioLight = lightObj.AddComponent<Light>();
            radioLight.type = LightType.Point;
            radioLight.color = Color.red;
            radioLight.intensity = 0.5f;
            radioLight.range = 1f;
            radioLight.enabled = false;

            // Setup music particles
            GameObject particleObj = new GameObject("MusicParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = new Vector3(0, 0.2f, 0);

            musicParticles = particleObj.AddComponent<ParticleSystem>();
            var main = musicParticles.main;
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.1f;
            main.startColor = Color.cyan;
            main.maxParticles = 50;

            var emission = musicParticles.emission;
            emission.rateOverTime = 10f;
            emission.enabled = false;

            var shape = musicParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
        }

        private void SetupNetworking()
        {
            // Ensure PhotonView is setup for synchronization
            if (photonView == null)
            {
                photonView = gameObject.AddComponent<PhotonView>();
            }

            photonView.observedComponents.Add(this);
            photonView.synchronization = ViewSynchronization.UnreliableOnChange;
        }

        private void Update()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                currentTime = audioSource.time;
                UpdateAudioVisualization();
                UpdateVisualEffects();
            }

            // Handle networking synchronization
            if (photonView.IsMine)
            {
                syncTimer += Time.deltaTime;
                if (syncTimer >= SYNC_INTERVAL)
                {
                    syncTimer = 0f;
                    needsSync = true;
                }
            }
        }

        private void UpdateAudioVisualization()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                // Get audio spectrum data
                audioSource.GetSpectrumData(audioSpectrum, 0, FFTWindow.BlackmanHarris);

                // Calculate frequency bands
                bassLevel = 0f;
                midLevel = 0f;
                trebleLevel = 0f;

                for (int i = 0; i < 64; i++)
                {
                    bassLevel += audioSpectrum[i];
                }

                for (int i = 64; i < 128; i++)
                {
                    midLevel += audioSpectrum[i];
                }

                for (int i = 128; i < 256; i++)
                {
                    trebleLevel += audioSpectrum[i];
                }

                bassLevel /= 64f;
                midLevel /= 64f;
                trebleLevel /= 128f;
            }
        }

        private void UpdateVisualEffects()
        {
            if (radioLight != null)
            {
                if (isPlaying && !isPaused)
                {
                    radioLight.enabled = true;
                    radioLight.intensity = 0.5f + bassLevel * 2f;
                    radioLight.color = Color.Lerp(Color.red, Color.green, midLevel * 10f);
                }
                else
                {
                    radioLight.enabled = false;
                }
            }

            if (musicParticles != null)
            {
                var emission = musicParticles.emission;
                if (isPlaying && !isPaused)
                {
                    emission.enabled = true;
                    emission.rateOverTime = 10f + (bassLevel + midLevel + trebleLevel) * 20f;
                }
                else
                {
                    emission.enabled = false;
                }
            }
        }

        public void PlayURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                ClimbTunesPlugin.Logger.LogWarning("Cannot play empty URL");
                return;
            }

            ClimbTunesPlugin.Logger.LogInfo($"Playing URL: {url}");

            currentURL = url;

            // Stop current playback
            Stop();

            // Start new playback
            if (IsYouTubeURL(url))
            {
                StartCoroutine(PlayYouTubeURL(url));
            }
            else
            {
                StartCoroutine(PlayDirectURL(url));
            }
        }

        private bool IsYouTubeURL(string url)
        {
            return url.Contains("youtube.com") || url.Contains("youtu.be");
        }

        private IEnumerator PlayYouTubeURL(string url)
        {
            // In a real implementation, you would use a service like youtube-dl
            // For demo purposes, we'll simulate the process
            ClimbTunesPlugin.Logger.LogInfo("Processing YouTube URL...");

            // Simulate processing time
            yield return new WaitForSeconds(2f);

            // For this demo, we'll create a placeholder audio clip
            // In reality, you would extract the audio stream from YouTube
            currentTitle = "YouTube Audio (Simulated)";
            currentClip = CreateDemoAudioClip();

            if (currentClip != null)
            {
                PlayAudioClip(currentClip);
            }
            else
            {
                ClimbTunesPlugin.Logger.LogError("Failed to load YouTube audio");
            }
        }

        private IEnumerator PlayDirectURL(string url)
        {
            ClimbTunesPlugin.Logger.LogInfo($"Loading direct audio URL: {url}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    currentClip = DownloadHandlerAudioClip.GetContent(www);
                    currentTitle = ExtractTitleFromURL(url);

                    if (currentClip != null)
                    {
                        PlayAudioClip(currentClip);
                    }
                }
                else
                {
                    ClimbTunesPlugin.Logger.LogError($"Failed to load audio from URL: {www.error}");
                }
            }
        }

        private string ExtractTitleFromURL(string url)
        {
            try
            {
                string[] parts = url.Split('/');
                string filename = parts[parts.Length - 1];
                return filename.Split('.')[0];
            }
            catch
            {
                return "Unknown Track";
            }
        }

        private AudioClip CreateDemoAudioClip()
        {
            // Create a simple demo audio clip (sine wave)
            int sampleRate = 44100;
            int duration = 30; // 30 seconds
            int samples = sampleRate * duration;

            float[] audioData = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                audioData[i] = Mathf.Sin(2 * Mathf.PI * 440 * t) * 0.3f; // 440 Hz sine wave
            }

            AudioClip clip = AudioClip.Create("DemoTrack", samples, 1, sampleRate, false);
            clip.SetData(audioData, 0);

            return clip;
        }

        private void PlayAudioClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();

                isPlaying = true;
                isPaused = false;
                totalTime = clip.length;

                ClimbTunesPlugin.Logger.LogInfo($"Now playing: {currentTitle} ({totalTime:F1}s)");

                // Sync with other players
                if (photonView.IsMine)
                {
                    photonView.RPC("SyncPlayback", RpcTarget.Others, currentURL, currentTitle, true, false);
                }
            }
        }

        public void Pause()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                isPaused = true;

                ClimbTunesPlugin.Logger.LogInfo("Playback paused");

                if (photonView.IsMine)
                {
                    photonView.RPC("SyncPlayback", RpcTarget.Others, currentURL, currentTitle, isPlaying, isPaused);
                }
            }
        }

        public void Resume()
        {
            if (audioSource != null && isPaused)
            {
                audioSource.UnPause();
                isPaused = false;

                ClimbTunesPlugin.Logger.LogInfo("Playback resumed");

                if (photonView.IsMine)
                {
                    photonView.RPC("SyncPlayback", RpcTarget.Others, currentURL, currentTitle, isPlaying, isPaused);
                }
            }
        }

        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }

            isPlaying = false;
            isPaused = false;
            currentTime = 0f;
            totalTime = 0f;

            ClimbTunesPlugin.Logger.LogInfo("Playback stopped");

            if (photonView.IsMine)
            {
                photonView.RPC("SyncPlayback", RpcTarget.Others, "", "", false, false);
            }
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        public void LoadPlaylist(string playlistUrl)
        {
            ClimbTunesPlugin.Logger.LogInfo($"Loading playlist: {playlistUrl}");

            // In a real implementation, you would parse the playlist
            // For demo purposes, we'll add some placeholder URLs
            playlist.Clear();
            playlist.Add(playlistUrl);

            // Simulate playlist loading
            for (int i = 1; i <= 5; i++)
            {
                playlist.Add($"{playlistUrl}&track={i}");
            }

            currentPlaylistIndex = 0;
            ClimbTunesPlugin.Logger.LogInfo($"Playlist loaded with {playlist.Count} tracks");
        }

        public void NextTrack()
        {
            if (playlist.Count > 0)
            {
                currentPlaylistIndex = (currentPlaylistIndex + 1) % playlist.Count;
                PlayURL(playlist[currentPlaylistIndex]);
            }
        }

        public void PreviousTrack()
        {
            if (playlist.Count > 0)
            {
                currentPlaylistIndex = (currentPlaylistIndex - 1 + playlist.Count) % playlist.Count;
                PlayURL(playlist[currentPlaylistIndex]);
            }
        }

        // Network synchronization methods
        [PunRPC]
        private void SyncPlayback(string url, string title, bool playing, bool paused)
        {
            if (!photonView.IsMine)
            {
                currentURL = url;
                currentTitle = title;
                isPlaying = playing;
                isPaused = paused;

                // Apply the synchronized state
                if (playing && !string.IsNullOrEmpty(url))
                {
                    if (currentClip == null || audioSource.clip != currentClip)
                    {
                        // Load the audio if we don't have it
                        StartCoroutine(LoadSyncedAudio(url));
                    }
                }
                else
                {
                    Stop();
                }
            }
        }

        private IEnumerator LoadSyncedAudio(string url)
        {
            if (IsYouTubeURL(url))
            {
                yield return StartCoroutine(PlayYouTubeURL(url));
            }
            else
            {
                yield return StartCoroutine(PlayDirectURL(url));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send data to other players
                stream.SendNext(isPlaying);
                stream.SendNext(isPaused);
                stream.SendNext(currentTime);
                stream.SendNext(currentURL);
                stream.SendNext(currentTitle);
                stream.SendNext(volume);
            }
            else
            {
                // Receive data from other players
                isPlaying = (bool)stream.ReceiveNext();
                isPaused = (bool)stream.ReceiveNext();
                currentTime = (float)stream.ReceiveNext();
                currentURL = (string)stream.ReceiveNext();
                currentTitle = (string)stream.ReceiveNext();
                volume = (float)stream.ReceiveNext();

                // Apply received volume
                if (audioSource != null)
                {
                    audioSource.volume = volume;
                }
            }
        }

        private void OnDestroy()
        {
            Stop();

            if (currentPlaybackCoroutine != null)
            {
                StopCoroutine(currentPlaybackCoroutine);
            }
        }

        // Interaction methods for players
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ClimbTunesPlugin.Logger.LogInfo($"Player entered radio range: {other.name}");
                // Show interaction prompt
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ClimbTunesPlugin.Logger.LogInfo($"Player left radio range: {other.name}");
                // Hide interaction prompt
            }
        }

        // Public methods for UI integration
        public string GetCurrentTrackInfo()
        {
            if (isPlaying)
            {
                return $"{currentTitle} ({currentTime:F0}s / {totalTime:F0}s)";
            }
            return "Not playing";
        }

        public float GetPlaybackProgress()
        {
            if (totalTime > 0)
            {
                return currentTime / totalTime;
            }
            return 0f;
        }

        public Dictionary<string, float> GetAudioLevels()
        {
            return new Dictionary<string, float>
            {
                { "bass", bassLevel },
                { "mid", midLevel },
                { "treble", trebleLevel }
            };
        }
    }
}
