using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

//[ExecuteInEditMode]
public class ImageSequence : MonoBehaviour 
{
	[System.Serializable]
	public struct Sequence
	{
		public GameObject imageObj; 
		public Image img;  
		public Vector3 moveVelocity;
		public float zoomMultiplier; 
		public float fadeStartDelay; 
		public float fadeInSpeed; 
		public float fadeOutSpeed; 
		public float stayTime; 
	}

	public Sequence[] imageSequences;  

	public bool imageSequencesFinished; 

	public int curSequence;  

	public void StartSequence()
	{
		StartCoroutine("SequenceProgression"); 
	}

	void Start()
	{
		// Set all image alphas to 0
		for (int i = 0; i < imageSequences.Length; i++)
		{
			imageSequences[i].img.color = new Color (1, 1, 1, 0); 
		}

		// If SkipToEnd() was called first from TitleScreen, don't allow the last one to be set to 0 alpha
		if (curSequence == imageSequences.Length - 1)
		{
			imageSequences[imageSequences.Length - 1].img.color = new Color (1, 1, 1, 1); 
		}
	}

	public void SkipToEnd()
	{
		curSequence = imageSequences.Length - 1; 
		imageSequencesFinished = true; 
		imageSequences[curSequence].img.color = new Color (1, 1, 1, 1); 
	}

	/*
	 * Step 1
	 * Fade in the first image
	 * Immediately start applying movement to that image
	 * Once fade in has finished, start decrementing stay time
	 * Once stay time has expired, fade out the curImage and simultaneously start fading in the next
	 */ 

	public IEnumerator SequenceProgression()
	{
		int thisSequence = curSequence; 
		float startDelayTimer = imageSequences[thisSequence].fadeStartDelay; 
		float a = 0;
		float timer = 0; 
		bool sequenceDone = false; 
		bool fadeInDone = false; 
		bool fadeOutImage = false; 
		Vector3 zoom; 

		while (!sequenceDone)
		{
			if (startDelayTimer > 0)
			{
				startDelayTimer -= Time.unscaledDeltaTime;
				yield return null; 
				continue; 
			}

			if (!fadeInDone)
			{
				// Update the alpha
				a += imageSequences[thisSequence].fadeInSpeed * Time.unscaledDeltaTime; 

				if (a > 1)
				{
					a = 1;
					fadeInDone = true;
					timer = imageSequences[thisSequence].stayTime; 
				}

				// Set the image fade
				imageSequences[thisSequence].img.color = new Color (1, 1, 1, a);	 
			}
			else if (!fadeOutImage)
			{
				timer -= Time.deltaTime; 

				// When the timer reaches zero, end this sequence
				if (timer <= 0)
				{
					timer = 0; 

					// If another sequence is available, fade out the current image and start the next sequence
					if (curSequence + 1 < imageSequences.Length)
					{
						curSequence++; 
						fadeOutImage = true; 
						StartCoroutine("SequenceProgression"); 
					}
					else
					{
						sequenceDone = true; 
						imageSequencesFinished = true; 
					}
				}
			}
			// If not the last image in the sequence, fade out
			else
			{
				// Update the alpha
				a -= imageSequences[thisSequence].fadeOutSpeed * Time.unscaledDeltaTime;

				if (a <= 0)
				{
					a = 0; 
					sequenceDone = true; 
				}

				// Set the image fade
				imageSequences[thisSequence].img.color = new Color (1, 1, 1, a);	 
			}


			// Make the image pan
			imageSequences[thisSequence].imageObj.transform.Translate(imageSequences[thisSequence].moveVelocity);

			// Make the object zoom
			zoom = imageSequences[thisSequence].imageObj.transform.localScale; 
			imageSequences[thisSequence].imageObj.transform.localScale = new Vector3 (zoom.x + (imageSequences[thisSequence].zoomMultiplier / 100), zoom.y + (imageSequences[thisSequence].zoomMultiplier / 100), zoom.z); 

			yield return null; 
		}
	}

	/*
	public bool addIndexToStart;
	public bool removeIndexFromStart; 

	void Update()
	{
		if (addIndexToStart)
		{
			addIndexToStart = false; 
			AddIndexToStart(); 
		}
		if (removeIndexFromStart)
		{
			removeIndexFromStart = false; 
			RemoveIndexFromStart(); 
		}
	}


	void AddIndexToStart()
	{
		Sequence[] backup = new Sequence[imageSequences.Length]; 

		for (int i = 0; i < imageSequences.Length; i++)
		{
			backup[i] = imageSequences[i]; 
		}

		imageSequences = new Sequence[backup.Length + 1]; 

		imageSequences[0] = new Sequence (); 

		for (int i = 0; i < backup.Length; i++)
		{
			imageSequences[i + 1] = backup[i]; 
		}
	}

	void RemoveIndexFromStart()
	{
		if (imageSequences.Length == 0)
		{
			return; 
		}
		if (imageSequences.Length == 1)
		{
			imageSequences = new Sequence[0]; 
			return; 
		}

		Sequence[] backup = new Sequence[imageSequences.Length]; 

		for (int i = 0; i < imageSequences.Length; i++)
		{
			backup[i] = imageSequences[i]; 
		}

		imageSequences = new Sequence[backup.Length - 1]; 

		for (int i = 1; i < backup.Length; i++)
		{
			imageSequences[i - 1] = backup[i]; 
		}
	}
	*/ 
}
