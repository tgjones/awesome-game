using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AwesomeGame
{
	public static class Sound
	{
		private static AudioEngine engine;
		private static WaveBank wavebank;
		private static SoundBank soundbank;

		public static Cue Play(string name)
		{
			Cue returnValue = soundbank.GetCue(name);
			returnValue.Play();
			return returnValue;
		}

		public static void Stop(Cue cue)
		{
			cue.Stop(AudioStopOptions.Immediate);
		}

		/// <summary>
		/// Starts up the sound code
		/// </summary>
		public static void Initialize()
		{
			engine = new AudioEngine("Sound\\Sound.xgs");
			wavebank = new WaveBank(engine, "Sound\\Wave Bank.xwb");
			soundbank = new SoundBank(engine, "Sound\\Sound Bank.xsb");
		}

		public static void Update()  //  Added
		{
			engine.Update();
		}

		/// <summary>
		/// Shuts down the sound code tidily
		/// </summary>
		public static void Shutdown()
		{
			soundbank.Dispose();
			wavebank.Dispose();
			engine.Dispose();
		}
	}
}
