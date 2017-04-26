using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicMusic : MonoBehaviour {

	AudioSource[] musicCon;
	AudioSource baseMusicCon;
	AudioSource fightMusicCon;
	AudioSource ambushMusicCon;
	AudioSource bossMusicCon;

	public float maxMusicVolume;

	public AudioClip fightMusic;
	public AudioClip normMusic;
	public AudioClip bossMusic;
	public AudioClip ambushMusic;

	public float fadeInSmooth;
	public float fadeOutSmooth;

	bool isInAR = false;

	public Enemy[] enemies;
	bool isSeen;

	bool fightingFound;

	float musTimer = 1;

	// Use this for initialization
	void Start () {
		musicCon = GameObject.Find ("MusicController").GetComponents<AudioSource> ();

		baseMusicCon = musicCon[0];
		fightMusicCon = musicCon[1];
		ambushMusicCon = musicCon[2];
		bossMusicCon = musicCon [3];

		baseMusicCon.Play ();
		fightMusicCon.Play ();
		ambushMusicCon.Play ();
		bossMusicCon.Play ();

		fightMusicCon.volume = 0;
		ambushMusicCon.volume = 0;
		bossMusicCon.volume = 0;
	}
	
	// Update is called once per frame
	void Update () {
		enemies = Object.FindObjectsOfType<Enemy> ();

		if (!baseMusicCon.isPlaying)
		{
			baseMusicCon.Play ();
		}

		if (!fightMusicCon.isPlaying)
		{
			baseMusicCon.Play ();
		}

		if (!ambushMusicCon.isPlaying)
		{
			baseMusicCon.Play ();
		}

		if (!bossMusicCon.isPlaying)
		{
			baseMusicCon.Play ();
		}

		fightingFound = false;
		for (int i = 0; i < enemies.Length; i++)
		{
			if (enemies [i].GetIsIdling () == false && enemies [i].tag != "Boss")
			{
				fightingFound = true;
				break;
			}
		}

		if (isInAR == false && GameState.inst.inBossRoom == false)
		{
			if (fightingFound == true)
			{
				if (musTimer > 0)
				{
					musTimer -= Time.deltaTime;
				}
				else if (musTimer <= 0)
				{
					musTimer = 0;
					SetVolume (baseMusicCon, 0, fadeOutSmooth);
					SetVolume (ambushMusicCon, 0, fadeOutSmooth);
					SetVolume (fightMusicCon, maxMusicVolume, fadeInSmooth);
					SetVolume (bossMusicCon, 0, fadeOutSmooth);
					//musicCon.clip = fightMusic;
				}
			}
			else if (fightingFound == false)
			{
				musTimer = 1;
				//musicCon.clip = normMusic;
				SetVolume (baseMusicCon, maxMusicVolume, fadeInSmooth);
				SetVolume (fightMusicCon, 0, fadeOutSmooth);
				SetVolume (ambushMusicCon, 0, fadeOutSmooth);
				SetVolume (bossMusicCon, 0, fadeOutSmooth);
			}
		}
		else if (GameState.inst.inBossRoom == true)
		{
			SetVolume (baseMusicCon, 0, fadeOutSmooth);
			SetVolume (fightMusicCon, 0, fadeOutSmooth);
			SetVolume (ambushMusicCon, 0, fadeOutSmooth);
			SetVolume (bossMusicCon, maxMusicVolume, fadeInSmooth);
		} 
		else if (isInAR == true) {
			SetVolume (baseMusicCon, 0, fadeOutSmooth);
			SetVolume (fightMusicCon, 0, fadeOutSmooth);
			SetVolume (ambushMusicCon, maxMusicVolume, fadeInSmooth);
			SetVolume (bossMusicCon, 0, fadeOutSmooth);
		}
	}

	public void SetVolume(AudioSource music, float desigVol, float smooth) {
		if (desigVol != music.volume)
		{
			music.volume = Mathf.Lerp (music.volume, desigVol, Time.deltaTime * smooth);

			if (desigVol == 0)
			{
				if (music.volume < 0.1)
				{
					music.volume = 0;
				}
			}
			else if (desigVol == maxMusicVolume)
			{
				if (music.volume > (maxMusicVolume - 0.1))
				{
					music.volume = maxMusicVolume;
				}
			}
		}
	}

	public void setIsInAR (bool b)
	{
		isInAR = b;
	}
}
