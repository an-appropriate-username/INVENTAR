using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace INVApp.Services
{
    public class SoundService
    {
        #region Fields

        private MediaElement _mediaElement;

        #endregion

        #region Constructor

        public SoundService()
        {
            _mediaElement = new MediaElement
            {
                IsVisible = false, 
                ShouldAutoPlay = false
            };
        }

        #endregion

        #region Public Methods

        /// <param name="mediaElement">The MediaElement instance to be used.</param>
        public void Initialize(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            PreloadSound("scan_beep.mp3"); 
        }

        /// <summary>
        /// Plays the specified sound file asynchronously.
        /// </summary>
        /// <param name="soundFileName">The name of the sound file to play.</param>
        public async Task PlaySoundAsync(string soundFileName)
        {
            if (_mediaElement != null)
            {
                try
                {
                    _mediaElement.Source = MediaSource.FromResource(soundFileName); 
                    _mediaElement.Play(); 
                    await Task.Delay(1000); 
                    _mediaElement.Stop(); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Adjusts the volume of the MediaElement.
        /// </summary>
        /// <param name="volume">The desired volume level (0 to 100).</param>
        public void ApplyVolume(double volume)
        {
            double scaledVolume = volume / 100.0; // Scale volume to range 0.0 to 1.0

            if (scaledVolume < 0.0 || scaledVolume > 1.0)
            {
                Console.WriteLine($"Invalid volume value: {volume}. It must be between 0 and 100.");
                return;
            }

            _mediaElement.Volume = scaledVolume;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Preloads a sound file to minimize playback delay.
        /// </summary>
        /// <param name="soundFileName">The name of the sound file to preload.</param>
        private void PreloadSound(string soundFileName)
        {
            if (_mediaElement != null)
            {
                try
                {
                    _mediaElement.Source = MediaSource.FromResource(soundFileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error preloading sound: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
