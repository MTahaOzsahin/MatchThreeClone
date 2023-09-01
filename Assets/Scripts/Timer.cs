using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    // reference to countdown Text 
    public Text timeLeftText;

    // reference to radially filled clock image
    public Image clockImage;

    // the starting time in seconds
    private int mMaxTime = 60;

    // do we stop the timer?
    public bool paused;

    // time window when we start flashing the clock face
    public int flashTimeLimit = 10;

    // sound to play for each second during the flash routine
    public AudioClip flashBeep;

    // how long it takes to flash to a different color and change back
    public float flashInterval = 1f;

    // color for flash effect
    public Color flashColor = Color.red;

    // reference to Flash coroutine
    private IEnumerator mFlashRoutine;

    // save the starting time and initialize the clock image, time left text, etc.
    public void InitTimer(int maxTime = 60)
    {
        mMaxTime = maxTime;

        // make sure the image is using Radial360 fillMethod with origin at top
        if (clockImage != null)
        {
            clockImage.type = Image.Type.Filled;
            clockImage.fillMethod = Image.FillMethod.Radial360;
            clockImage.fillOrigin = (int) Image.Origin360.Top;
        }

        if (timeLeftText != null)
        {
            timeLeftText.text = maxTime.ToString();
        }
    }

    // update the clock image and time left text
    public void UpdateTimer(int currentTime)
    {
        if (paused)
        {
            return;
        }

        if (clockImage != null)
        {
            // update the clock fill
            clockImage.fillAmount =  currentTime / (float) mMaxTime;

            // flash and play extra beep if within the danger zone
            if (currentTime <= flashTimeLimit)
            {
                // start the flash effect
                mFlashRoutine = FlashRoutine(clockImage, flashColor, flashInterval);
                StartCoroutine(mFlashRoutine);

                if (SoundManager.Instance != null && flashBeep != null)
                {
                    SoundManager.Instance.PlayClipAtPoint(flashBeep, Vector3.zero, SoundManager.Instance.fxVolume, false);
                }
            }
        }

        // update countdown text
        if (timeLeftText != null)
        {
            timeLeftText.text = currentTime.ToString();
        }
    }
  
    // change an image to a flash color for a fraction of a interval and change the color back
    IEnumerator FlashRoutine(Image image, Color targetColor, float interval)
    {
        if (image != null)
        {
            Color originalColor = image.color;
            image.CrossFadeColor(targetColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);

            image.CrossFadeColor(originalColor, interval * 0.3f, true, true);
            yield return new WaitForSeconds(interval * 0.5f);
        }
    }

    // fade off any ScreenFaders components
    public void FadeOff()
    {
        // stop any flash effect currently running
        if (mFlashRoutine != null)
        {
            StopCoroutine(mFlashRoutine);
        }

        ScreenFader[] screenFaders = GetComponentsInChildren<ScreenFader>();
        foreach (ScreenFader fader in screenFaders)
        {
            fader.FadeOff();
        }
    }
}
