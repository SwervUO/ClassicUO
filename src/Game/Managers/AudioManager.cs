﻿#region license

//  Copyright (C) 2019 ClassicUO Development Community on Github
//
//	This project is an alternative client for the game Ultima Online.
//	The goal of this is to develop a lightweight client considering 
//	new technologies.  
//      
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using ClassicUO.Configuration;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using ClassicUO.IO.Audio;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.Game.Managers
{
    internal class AudioManager
    {
        private UOMusic _currentMusic;

        private bool _canReproduceAudio = true;

        public void Initialize()
        {
            try
            {
                new DynamicSoundEffectInstance(0, AudioChannels.Stereo)
                    .Dispose();
            }
            catch (NoAudioHardwareException ex)
            {
                Log.Message(LogTypes.Warning, ex.ToString());
                _canReproduceAudio = false;
            }
        }

        public void PlaySound(int index, AudioEffects effect = AudioEffects.None, bool spamCheck = false)
        {
            if (!_canReproduceAudio)
                return;

            if (ProfileManager.Current == null || !ProfileManager.Current.EnableSound)
                return;

            float volume = ProfileManager.Current.SoundVolume / Constants.SOUND_DELTA;

            if (CUOEnviroment.Client.IsActive)
            {
                if (!ProfileManager.Current.ReproduceSoundsInBackground) volume = ProfileManager.Current.SoundVolume / Constants.SOUND_DELTA;
            }
            else if (!ProfileManager.Current.ReproduceSoundsInBackground) volume = 0;


            if (volume < -1 || volume > 1f)
                return;

            FileManager.Sounds.GetSound(index)?.Play(true, effect, volume, spamCheck);
        }

        public void PlaySoundWithDistance(int index, float volume, bool spamCheck = false)
        {
            if (!_canReproduceAudio)
                return;

            if (ProfileManager.Current == null || !ProfileManager.Current.EnableSound || !CUOEnviroment.Client.IsActive && !ProfileManager.Current.ReproduceSoundsInBackground)
                return;

            if (!CUOEnviroment.Client.IsActive && !ProfileManager.Current.ReproduceSoundsInBackground) volume = 0;

            if (volume < -1 || volume > 1f)
                return;

            FileManager.Sounds.GetSound(index)?.Play(true, AudioEffects.None, volume, spamCheck);
        }

        public void PlayMusic(int music)
        {
            if (!_canReproduceAudio)
                return;

            if (music >= Constants.MAX_MUSIC_DATA_INDEX_COUNT)
                return;

            float volume;

            if (CUOEnviroment.Client.Scene is LoginScene)
            {
                if (!Settings.GlobalSettings.LoginMusic)
                    return;

                volume = Settings.GlobalSettings.LoginMusicVolume / Constants.SOUND_DELTA;
            }
            else
            {
                if (ProfileManager.Current == null || !ProfileManager.Current.EnableMusic)
                    return;

                volume = ProfileManager.Current.MusicVolume / Constants.SOUND_DELTA;
            }


            if (volume < -1 || volume > 1f)
                return;

            Sound m = FileManager.Sounds.GetMusic(music);

            if (m == null && _currentMusic != null)
                StopMusic();
            else if (m != null && m != _currentMusic)
            {
                StopMusic();
                _currentMusic = (UOMusic) m;
                _currentMusic.Play(false, volume: volume);
            }
        }

        public void UpdateCurrentMusicVolume()
        {
            if (!_canReproduceAudio)
                return;

            if (_currentMusic != null)
            {
                if (ProfileManager.Current == null || !ProfileManager.Current.EnableMusic)
                    return;

                float volume = ProfileManager.Current.MusicVolume / Constants.SOUND_DELTA;

                if (volume < -1 || volume > 1f)
                    return;

                _currentMusic.Volume = volume;
            }
        }

        public void StopMusic()
        {
            if (_currentMusic != null)
            {
                _currentMusic.Stop();
                _currentMusic.Dispose();
                _currentMusic = null;
            }
        }

        public void Update()
        {
            if (!_canReproduceAudio)
                return;

            if (_currentMusic != null && ProfileManager.Current != null)
            {
                if (CUOEnviroment.Client.IsActive)
                {
                    if (!ProfileManager.Current.ReproduceSoundsInBackground) _currentMusic.Volume = ProfileManager.Current.MusicVolume / Constants.SOUND_DELTA;
                }
                else if (!ProfileManager.Current.ReproduceSoundsInBackground && _currentMusic.Volume != 0) _currentMusic.Volume = 0;
            }

            _currentMusic?.Update();
        }
    }
}