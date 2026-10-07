using UnityEngine;
using UnityEngine.Video;

public class TVController : MonoBehaviour
{
    [Header("Видео")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip[] videos;      // список видео (может играть по кругу)

    [Header("Экран")]
    [SerializeField] private RenderTexture screenTexture;

    [Header("Настройки")]
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool muteAudio = true;   // звук ТВ, если не нужен

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.targetTexture = screenTexture;
        videoPlayer.isLooping = loop;

        if (muteAudio)
            videoPlayer.SetDirectAudioMute(0, true);

        if (videos != null && videos.Length > 0)
            videoPlayer.clip = videos[0];

        if (playOnStart)
            videoPlayer.Play();
    }

    public void PlayNext()
    {
        if (videos == null || videos.Length == 0) return;

        int current = System.Array.IndexOf(videos, videoPlayer.clip);
        int next = (current + 1) % videos.Length;
        videoPlayer.clip = videos[next];
        videoPlayer.Play();
    }
}