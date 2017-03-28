using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.Audio; 

public class SoundSlider : MonoBehaviour 
{
	public Slider slider; 

	public enum SoundSliderType {Music, SFX}; 
	public SoundSliderType soundType; 

	// Use this for initialization
	void Start () 
	{
		if (soundType == SoundSliderType.Music)
		{
			slider.value = GlobalManager.inst.GetMusicVolume(); 
		}
		else if (soundType == SoundSliderType.SFX)
		{
			slider.value = GlobalManager.inst.GetSFXVolume(); 
		}
	}

	// Invoked when the value of the slider changes.
	public void ValueChanged()
	{
		if (soundType == SoundSliderType.Music)
		{
			GlobalManager.inst.SetMusicVolume(slider.value); 
		}
		else if (soundType == SoundSliderType.SFX)
		{
			GlobalManager.inst.SetSFXVolume(slider.value); 
		}
	}

}
