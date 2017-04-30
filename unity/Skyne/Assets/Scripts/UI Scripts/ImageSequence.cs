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

		public bool useTargetPos; 
		[HideInInspector] public Vector3 startPos; 
		public Vector3 targetPos; 

		public bool easeToTargetPos; 

		//public Vector3 moveVelocity;
		public float zoomMultiplier; 
		public float fadeStartDelay; 
		public float fadeInSpeed; 
		public float fadeOutSpeed; 
		public float stayTime;

		//[TextArea(3,10)]
		//public string panelText;  

		public Text sequenceText; 
	}

	public Sequence[] imageSequences;  

	public bool imageSequencesFinished; 

	public int curSequence;  

	public Text textObj; 

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

		for (int i = 0; i < imageSequences.Length; i++)
		{
			if (imageSequences[i].sequenceText != null)
			{
				imageSequences[i].sequenceText.text = ""; 
			}
		}

		imageSequences[imageSequences.Length - 1].imageObj.GetComponent<RectTransform>().anchoredPosition = imageSequences[imageSequences.Length - 1].targetPos; 
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
		float timer = imageSequences[thisSequence].stayTime; 
		bool sequenceDone = false; 
		bool fadeInDone = false; 
		bool fadeOutImage = false; 
		Vector3 zoom; 

		// Movement stuff
		RectTransform rect = imageSequences[thisSequence].imageObj.GetComponent<RectTransform>(); 
		//Vector3 startPos = imageSequences[thisSequence].imageObj.transform.position; 
		Vector3 startPos = rect.anchoredPosition; 
		float currentTime = 0; 

		//textObj.text = imageSequences[thisSequence].panelText;
		Color textColor = new Color(1, 1, 1, 0);
		if (imageSequences[thisSequence].sequenceText != null)
		{
			textColor = imageSequences[thisSequence].sequenceText.color; 
			imageSequences[thisSequence].sequenceText.color = new Color (textColor.r, textColor.g, textColor.b, a); 
		}

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
				timer -= Time.deltaTime;

				// Update the alpha
				a += imageSequences[thisSequence].fadeInSpeed * Time.unscaledDeltaTime; 

				if (a > 1)
				{
					a = 1;
					fadeInDone = true; 
				}

				// Set the image fade
				imageSequences[thisSequence].img.color = new Color (1, 1, 1, a);

				// Set the text fade
				if (imageSequences[thisSequence].sequenceText != null)
				{
					imageSequences[thisSequence].sequenceText.color = new Color (textColor.r, textColor.g, textColor.b, a); 
				}
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

				// Set the text color
				if (imageSequences[thisSequence].sequenceText != null)
				{
					imageSequences[thisSequence].sequenceText.color = new Color (textColor.r, textColor.g, textColor.b, a); 
				}
			}

			if (imageSequences[thisSequence].stayTime != 0)
			{
				// The stayTime + 1.5f adds some buffer to account for fade out time
				if (imageSequences[thisSequence].fadeOutSpeed != 0)
				{
					currentTime = (imageSequences[thisSequence].stayTime - timer) / (imageSequences[thisSequence].stayTime + 1.5f); 
				}
				else
				{
					currentTime = (imageSequences[thisSequence].stayTime - timer) / (imageSequences[thisSequence].stayTime); 
				}
				Debug.Log("CurrentTime: " + currentTime); 
			}

			// Make the image pan
			//imageSequences[thisSequence].imageObj.transform.Translate(imageSequences[thisSequence].moveVelocity);
			//imageSequences[thisSequence].imageObj.transform.position = Vector3.Lerp(startPos, imageSequences[thisSequence].targetPos, currentTime); 
			if (imageSequences[thisSequence].useTargetPos)
			{
				rect.anchoredPosition = Vector3.Lerp(startPos, imageSequences[thisSequence].targetPos, currentTime);
			}
			else if (imageSequences[thisSequence].easeToTargetPos)
			{
				rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, imageSequences[thisSequence].targetPos, (0.6f + currentTime * 10) * Time.deltaTime);
			}

			// Make the object zoom
			zoom = imageSequences[thisSequence].imageObj.transform.localScale; 
			//imageSequences[thisSequence].imageObj.transform.localScale = new Vector3 (zoom.x + (imageSequences[thisSequence].zoomMultiplier / 100), zoom.y + (imageSequences[thisSequence].zoomMultiplier / 100), zoom.z); 
			imageSequences[thisSequence].imageObj.transform.localScale = new Vector3 (zoom.x + (imageSequences[thisSequence].zoomMultiplier * Time.deltaTime), zoom.y + (imageSequences[thisSequence].zoomMultiplier * Time.deltaTime), zoom.z); 

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
