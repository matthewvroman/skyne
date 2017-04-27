using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.Audio; 

public class SliderFunctions : MonoBehaviour 
{
	public Slider slider; 

	public enum SliderType {Music, SFX, Sensitivity}; 
	public SliderType sliderType; 

	// Use this for initialization
	void Start () 
	{
		if (sliderType == SliderType.Music)
		{
			//slider.value = GlobalManager.inst.GetMusicVolume(); 
			slider.value = PlayerPrefsManager.inst.GetSavedMusicVolume(0); 
		}
		else if (sliderType == SliderType.SFX)
		{
			//slider.value = GlobalManager.inst.GetSFXVolume(); 
			slider.value = PlayerPrefsManager.inst.GetSavedSFXVolume(0); 
		}
		else if (sliderType == SliderType.Sensitivity)
		{
			slider.value = PlayerPrefsManager.inst.GetSavedMouseSensitivity(GlobalManager.inst.defaultSensitivity); 
		}
	}

	// Invoked when the value of the slider changes.
	public void ValueChanged()
	{
		if (sliderType == SliderType.Music)
		{
			PlayerPrefsManager.inst.SaveMusicVolume(slider.value); 
			GlobalManager.inst.SetMusicVolume(slider.value); 
		}
		else if (sliderType == SliderType.SFX)
		{
			PlayerPrefsManager.inst.SaveSFXVolume(slider.value);
			GlobalManager.inst.SetSFXVolume(slider.value); 
		}
		else if (sliderType == SliderType.Sensitivity)
		{
			PlayerPrefsManager.inst.SaveMouseSensitivity(slider.value); 

			if (MainCameraControl.inst != null)
			{
				MainCameraControl.inst.SetMouseSensitivity(slider.value); 
			}
		}
	}

}
