using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sonificator : MonoBehaviour
{
    int samplerate = 44100;
    public AudioSource audioSource;
    public float dur;
    public float amp = 1;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void Generate(List<MotionPacket> motion, float timescale)
    {

        float duration = ((motion[motion.Count - 1].time - motion[0].time) * timescale + 2.0f);

        int sampleCount = (int)(duration * samplerate);

        if (sampleCount < 100) return;

        //Debug.Log(duration);


        AudioClip clip = AudioClip.Create("Mytune", sampleCount, 1, samplerate, false);

        float[] wave = new float[sampleCount];
        //float[] scaledwave = new float[sampleCount * timescale];

        //for(int i=0; i<sampleCount; ++i)
        //{
        //    wave[i] = 0.0f;
        //}

        //float t0 = motion[0].time;

        //float factor = clipOriginal.length / motion.Count;
        //float x_prev = 0;
        //float y_prev = 0;
        //float displaceY_prev = 0;
        //float displaceX_prev = 0;
        //float t_prev = motion[0].time;

        //foreach(MotionPacket m in motion)
        //{

        //    float dur = 0.03f;
        //    float vol = 0.1f;
        //    float amp_x;
        //    float amp_y;
        //    float hz_x = m.centroid.x*1000.0f + 200.0f;
        //    float hz_y = m.centroid.y*1000.0f + 200.0f;
        //    float phase = 0.0f;
        //    float startT = m.time - t0;
        //    float endT = startT + dur;

        //    if (y_prev == 0 && x_prev == 0)
        //    {
        //        amp_x = vol * 0.5f;
        //        amp_y = vol * 0.5f;
        //    }
        //    else
        //    {
        //        float v_x = Mathf.Sqrt((m.centroid.x - x_prev) * (m.centroid.x - x_prev) / (m.time - t_prev) * (m.time - t_prev));
        //        amp_x = vol * (v_x + 1);
        //        float v_y = Mathf.Sqrt((m.centroid.y - y_prev) * (m.centroid.y - y_prev) / (m.time - t_prev) * (m.time - t_prev));
        //        amp_y = vol * (v_y + 1);
        //    }




        //    int startI = (int)(startT * samplerate);
        //    int endI = (int)(endT * samplerate);

        //    if (startI < 0) startI = 0;
        //    if (endI >= sampleCount) continue;

        //    float hzfac_x = hz_x * Mathf.PI * 2.0f;
        //    float hzfac_y = hz_y * Mathf.PI * 2.0f ;
        //    for (int i=startI; i<endI; ++i)
        //    {
        //        float t = i / (float)samplerate;
        //        wave[i] += amp_x * Mathf.Sin(t* hzfac_x) + amp_y * Mathf.Cos(t * hzfac_y);
        //    }

        //    displaceX_prev = m.centroid.x - x_prev;
        //    displaceY_prev = m.centroid.y - y_prev;
        //    x_prev = m.centroid.x;
        //    y_prev = m.centroid.y;
        //    t_prev = m.time;

        //}

        //Debug.Log(wave.Length);


        for (int i = 0; i < sampleCount; ++i)
        {
            wave[i] = 0.0f;
        }



        foreach (MotionPacket m in motion)
        {
            addTonePoint(m, wave, motion, timescale);
            //tone_vx(m, 0.03f, wave, motion);
            //tone_cenx(m, 0.03f, wave, motion);
            //tone_ax(m, 0.03f, wave, motion);
            //tone_vy(m, 0.03f, wave, motion);
            //tone_ceny(m, 0.03f, wave, motion);
            //tone_ay(m, 0.03f, wave, motion);
        }

        //Debug.Log(wave.Length);

        //scaleTone(wave, scaledwave, Mathf.RoundToInt(timescale));

        //for(int i=0; i<wave.Length; ++i)
        //{
        //    float t = i / (float)samplerate;
        //    wave[i] = 0.5f*Mathf.Sin(t * 400.0f * Mathf.PI * 2.0f);
        //}


        clip.SetData(wave, 0);

        audioSource.clip = clip;
        audioSource.Play();
    }


    //void tone_vx(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;

    //    float startT = m.time - t0;
    //    float endT = startT + dur;


    //    addSinTone(startT, endT, 0.4f, 200.0f * (m.v.x + 1.0f), 0, wave, samplerate);


    //}

    //void tone_vy(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;

    //    float startT = m.time - t0;
    //    float endT = startT + dur;


    //    addSinTone(startT, endT, 0.4f, 200.0f * (m.v.y + 1.0f), 0, wave, samplerate);


    //}

    //void tone_cenx(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;
    //    float startT = m.time - t0;
    //    float endT = startT + dur;

    //    addSinTone(startT, endT, m.centroid.x, 200.0f, 0, wave, samplerate);
    //}

    //void tone_ceny(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;
    //    float startT = m.time - t0;
    //    float endT = startT + dur;

    //    addSinTone(startT, endT, m.centroid.y, 200.0f, 0, wave, samplerate);
    //}

    //void tone_ax(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;
    //    float startT = m.time - t0;
    //    float endT = startT + dur;

    //    addSinTone(startT, endT, 0.4f, 200.0f, m.a.x, wave, samplerate);
    //}

    //void tone_ay(MotionPacket m, float dur, float[] wave, List<MotionPacket> motion)
    //{
    //    float t0 = motion[0].time;
    //    float startT = m.time - t0;
    //    float endT = startT + dur;

    //    addSinTone(startT, endT, 0.4f, 200.0f, m.a.y, wave, samplerate);
    //}

    public void addTonePoint(MotionPacket m, float[] wave, List<MotionPacket> motion, float timescale)
    {
        float t0 = motion[0].time;
        float startT = (m.time - t0) * 3;
        float endT;


        if (m.aMag <= 1.0f)
        {
            dur = m.aMag * 0.2f;
        }
        else if (Mathf.Log(m.aMag) <= 4.0f)
        {
            dur = Mathf.Log(m.aMag) * 0.2f;
        }
        else
        {
            dur = 0.5f;
        }

        endT = startT + dur * timescale;

        addSpeedTone(startT, endT, m.speed, m, wave, samplerate);


    }

    public void addSpeedTone(float t0, float t1, float f, MotionPacket m, float[] wave, int sampleRate_)
    {
        int i0 = (int)(t0 * sampleRate_);
        int i1 = (int)(t1 * sampleRate_);

        float t = t0;
        float dt = (t1 - t0) / (float)(i1 - i0);
        Vector3 xaxis = new Vector3(1, 0, 0);
        float coefficient = -Vector3.Dot(m.v_3, xaxis) * 2 + 3.0f;
        float ampcoefficient = m.centroid.y;


        float hz = f * 180 * Mathf.PI * 2.0f + 200.0f;
        for (int i = i0; i < i1 && i < wave.Length; ++i)
        {
            wave[i] += amp*ampcoefficient * Mathf.Sin(t * hz * coefficient);
            t += dt;
        }

    }

    //public void scaleTone(float[] wave, float[] scaledwave, int timescale)
    //{

    //}
    //public static void addSinTone(float t0, float t1, float f, float[] wave, int sampleRate_)
    //{
    //    int i0 = (int)(t0 * sampleRate_);
    //    int i1 = (int)(t1 * sampleRate_);

    //    float t = t0;
    //    float dt = (t1 - t0) / (float)(i1 - i0);

    //    float hz = f * Mathf.PI * 2.0f;
    //    for (int i = i0; i < i1 && i < wave.Length; ++i)
    //    {
    //        wave[i] += a * Mathf.Sin(t * hz + theta);
    //        t += dt;
    //    }

    //}

    // Update is called once per frame
    void Update()
    {

    }
}
