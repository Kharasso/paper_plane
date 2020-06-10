using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageRecorder : MonoBehaviour
{
    public int samplerate = 44100;
    public float frequency = 440;
    public int position = 0;
    public Texture2D graphTexture;
    public int[] dataChain;
    public AudioSource aud;
    
    // Start is called before the first frame update
    void Start()
    {
        int w = graphTexture.width;
        int h = graphTexture.height;
        Color[] colors = graphTexture.GetPixels();

        dataChain = new int[w];
        
            for (int i = 0; i < w; i++)
        {
            int j = 0;
            while (j < h-1)
            {
                if (colors[j * w + i] != Color.black)
                {
                    dataChain[i] = j;
                }

                j++;
            }
        }

        AudioClip myClip = AudioClip.Create("Mytune", samplerate * 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        aud.clip = myClip;
        aud.Play();
    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while(count < data.Length)
        {
            data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate)*dataChain[count%dataChain.Length];
            position++;
            count++;
        }
    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
