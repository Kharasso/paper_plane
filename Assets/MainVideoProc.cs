using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MainVideoProc : MonoBehaviour
{
    Color[] colorIn;
    Color[] colorInPrev;
    Color[] accumulator;
    Color[] colorOut;
    Texture2D procTexture;
    Vector2 centroid = new Vector2(0, 0);
    Vector2 Cen;
    Vector2 CenInPrev;
    List<MotionPacket> samples = new List<MotionPacket>();
    List<MotionPacket> intsamples = new List<MotionPacket>();

    int w;
    int h;
    float time;
    bool StartVisualAnalysis;
    float speedmost;
    float accmost;
    bool ComputeNextMove;
    int currentindex = 0;
    int nn = 0;

    //WebCamTexture web;
    public Sonificator soni;
    public GameObject centroidObject;
    public Material screenMaterialRaw;
    public Material screenMaterialProc;
    public Material videoScreen;
    public RenderTexture videoTexture;
    public VideoPlayer videoPlayer;
    public float ratio = 0.5f;
    public float timescale = 3.0f;
    public Color vColor;
    public Color aColor;
    public float AlphaRatio = 0.5f;
    public float colorratio = 0.5f;
    public float dtThresh = 0.3f;
    public float planeScale = 0.2f;


    void Start()
    {
        //web = new WebCamTexture();
        //web.Play();
        //videoScreen.mainTexture = web;


        w = videoScreen.mainTexture.width;
        h = videoScreen.mainTexture.height;
        procTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        colorOut = new Color[w * h];
        accumulator = new Color[w * h];
        StartVisualAnalysis = false;
        time = 0.0f;
        speedmost = 0.0f;
        accmost = 0.0f;
        ComputeNextMove = true;

        centroidObject.SetActive(false);


        for (int i = 0; i < colorOut.Length; i++)
        {
            colorOut[i] = Color.white;
        }

        for (int i = 0; i < accumulator.Length; ++i)
            accumulator[i] = Color.white;

        startRecording();
        screenMaterialProc.mainTexture = procTexture;
    }

    bool isRecording = false;

    void startRecording()
    {
        isRecording = true;
        samples.Clear();
        StartVisualAnalysis = false;
    }

    void endRecording()
    {
        isRecording = false;
        StartVisualAnalysis = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isRecording)
            {
                sonification();
                endRecording();
                //resample();
                centroidObject.SetActive(true);
            }
            else startRecording();
        }

        if (isRecording)
        {
            analysis();
        }

        if (StartVisualAnalysis && (time < timescale * samples[samples.Count - 1].time))
        {
            time += Time.deltaTime;

            if (ComputeNextMove)
            {
                
                visualization(nn);
            }

            if (time > samples[nn].time * timescale)
            {
                if(nn < samples.Count - 1)
                {
                    nn++;
                }
                ComputeNextMove = true;
            }
            else
                ComputeNextMove = false;
        }
    }

    float startRecTime = 0.0f;


    void analysis()
    {
        Texture tex = videoScreen.mainTexture;
        
        Color[] cs=null;
        if (tex is RenderTexture)
        {
            RenderTexture rt = tex as RenderTexture;
            cs = getPixelsFromTexture(rt);
        }
        else if (tex is WebCamTexture)
        {
            WebCamTexture ct = tex as WebCamTexture;
            cs = ct.GetPixels();
        }
        else
        {
            return;
        }
   
        if (colorInPrev == null) colorInPrev = cs;
        else colorInPrev = colorIn;
        colorIn = cs;

        float cenx = 0.0f;
        float ceny = 0.0f;
        float totalMovement = 0.0f;
        int k = 0;
        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                float dr = colorIn[k].r - colorInPrev[k].r;
                float dg = colorIn[k].g - colorInPrev[k].g;
                float db = colorIn[k].b - colorInPrev[k].b;
                float dt = dr * dr + dg * dg + db * db;

                if (dt < dtThresh)
                    dt = 0f;

                totalMovement += dt;
                cenx += i * dt;
                ceny += j * dt;

                ++k;
            }
        } 

        if (totalMovement > 0.001f)
        {
            cenx /= totalMovement;
            ceny /= totalMovement;
            cenx *= (1.0f / w);
            ceny *= (1.0f / w);
            Vector2 centroidNow = new Vector2(cenx, ceny);

            if (CenInPrev == null) CenInPrev = centroidNow;
            else
            {
                CenInPrev = Cen;
                Cen = centroidNow;
            }

            //smoothening
            centroid = centroid * ratio + centroidNow * (1 - ratio);

            MotionPacket m = new MotionPacket();
            m.centroidInst = centroidNow;
            m.centroid = centroid;
            m.totalM = totalMovement;

            if (samples.Count == 0) startRecTime = Time.time;
            m.time = Time.time-startRecTime;
            samples.Add(m);

            if (samples.Count > 1024) samples.RemoveAt(0);
        }
    }



    void sonification()
    {
        PostProcess();
        soni.Generate(samples, timescale);
    }


    void PostProcess()
    {
        if (samples.Count < 3) return;

        //compute velocity
        for(int i=0; i<samples.Count; ++i)
        {
            int i1 = i + 1;
            if (i1 >= samples.Count) i1 = i;

            int i0 = i - 1;
            if (i0 <0) i0 = 0;

            MotionPacket s0 = samples[i0];
            MotionPacket s = samples[i];
            MotionPacket s1 = samples[i1];

            float dt = s1.time - s0.time;
            
            s.v = (s1.centroid - s0.centroid)/dt;
            s.speed = s.v.magnitude;
            s.v_3 = Vector2ToVector3(s.v);

            if (s.speed > speedmost)
                speedmost = s.speed;
        }

        //compute acceleration
        for (int i = 0; i < samples.Count; ++i)
        {
            MotionPacket s0 = samples[i==0?i:i-1];
            MotionPacket s = samples[i];
            MotionPacket s1 = samples[i == samples.Count - 1 ? i : i + 1];

            float dt = s1.time - s0.time;

            s.a = (s1.v - s0.v)/dt;
            s.aMag = s.a.magnitude;
            s.a_3 = Vector2ToVector3(s.a);
            
            if (accmost < s.aMag)
                accmost = s.aMag;
        }
    }


    void resample()
    {
        float totaltime = samples[samples.Count - 1].time;
        float TimeInterval = totaltime / 20.0f;

        for(float t = 0.0f; t <totaltime; t += TimeInterval)
        {
            MotionPacket m = evalAtTime(t);
            intsamples.Add(m);
            
            if (intsamples.Count > 1024) intsamples.RemoveAt(0);
        }
    }

  
    void visualization(int currentindex)
    {
        if (samples.Count == 0) return;

        // Put centroid object to correct place
        Vector3 centroidWorld = viewport2World(samples[currentindex].centroid);
        centroidObject.transform.position = centroidWorld;
        
        if (timescale * samples[currentindex].time > time)
            currentindex++;

        if (currentindex >= samples.Count) currentindex = 0;

        // Get velocity and acceleration and their normalized values
        Vector3 ovv = samples[currentindex].v_3;
        Vector3 ova = samples[currentindex].a_3;
        Vector3 vv = Vector3.Normalize(ovv);
        Vector3 va = Vector3.Normalize(ova);
        
        // transform the centroid object to correct scale and angle
        centroidObject.transform.localScale = new Vector3(ovv.magnitude * planeScale, 0f, 0.2f * planeScale);
        centroidObject.transform.rotation = Quaternion.identity;
        centroidObject.transform.RotateAround(new Vector3(0, 1, 0), Mathf.Atan2(-vv.z, vv.x));

        Vector2Int px = viewport2Pixel(samples[currentindex].centroid);
        if (!(px.x < 0 || px.x >= w || px.y < 0 || px.y >= h) && ComputeNextMove)
        {
            Vector3 px_3 = Vector2ToVector3(px);

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int kk = i * w + j;
                    Vector3 temp = new Vector3(j, 0, i);
                    Vector3 v = Vector3.Normalize(temp - px_3);

                    //vColor.a = intsamples[currentindex].speed / speedmost * AlphaRatio;
                    //aColor.a = intsamples[currentindex].aMag / accmost * AlphaRatio;

                    if (Vector3.Dot(Vector3.Cross(vv, va), Vector3.Cross(vv, v)) > 0 &&
                        Vector3.Dot(Vector3.Cross(v, va), Vector3.Cross(v, vv)) > 0)
                    {
                        colorOut[kk] = (1 - colorratio) * colorOut[kk] + colorratio * vColor;
                    }
                    else if (Vector3.Dot(Vector3.Cross(vv, va), Vector3.Cross(vv, v)) > 0 &&
                        Vector3.Dot(Vector3.Cross(v, va), Vector3.Cross(v, vv)) < 0)
                    {
                        colorOut[kk] = (1 - colorratio) * colorOut[kk] + colorratio * aColor;
                    }
                    else if (Vector3.Dot(Vector3.Cross(vv, va), Vector3.Cross(vv, v)) < 0 &&
                        Vector3.Dot(Vector3.Cross(v, va), Vector3.Cross(v, vv)) > 0)
                    {
                        colorOut[kk] = (1 - colorratio) * colorOut[kk] + colorratio * Color.yellow;
                    }
                    else if (Vector3.Dot(Vector3.Cross(vv, va), Vector3.Cross(vv, v)) < 0 &&
                        Vector3.Dot(Vector3.Cross(v, va), Vector3.Cross(v, vv)) < 0)
                    {
                        colorOut[kk] = (1 - colorratio) * colorOut[kk] + colorratio * Color.green;
                    }
                    else
                    {
                        colorOut[kk] = Color.white;
                    }
                }
            }

            //write line
            //Vector2 _v = samples[currentindex].v.normalized;
            //float k = _v.y / _v.x;
            //int _x = px.x;
            //int _y = px.y;
            //float acc_y = _y;

            //for (int i = _x; i < w - 1; i++)
            //{
            //    acc_y += k;

            //    int new_y = (int)(acc_y + 0.5f);
            //    if (new_y < 0 || new_y > h || i < 0 || i > w)
            //    {
            //        return;
            //    }

            //    int kkk = w * new_y + i + 1;
            //    if (kkk < colorOut.Length)
            //    {
            //        colorOut[kkk] = Color.black;
            //    }
            //}

            //acc_y = _y;
            //for (int i = _x; i > 1; i--)
            //{
            //    acc_y -= k;

            //    int new_y = (int)(acc_y - 0.5f);
            //    if (new_y < 0 || new_y > h || i < 0 || i > w)
            //    {
            //        return;
            //    }

            //    int kkk = w * new_y + i - 1;
            //    if (kkk < colorOut.Length)
            //    {
            //        colorOut[kkk] = Color.black;
            //    }
            //}

            //Vector2 _a = samples[currentindex].a.normalized;
            //float k_a = _a.y / _a.x;
            //acc_y = _y;

            //for (int i = _x; i < w - 1; i++)
            //{
            //    acc_y += k_a;

            //    int new_y = (int)(acc_y + 0.5f);
            //    if (new_y < 0 || new_y > h || i < 0 || i > w)
            //    {
            //        return;
            //    }

            //    int kkk = w * new_y + i + 1;
            //    if (kkk < colorOut.Length)
            //    {
            //        colorOut[kkk] = Color.black;
            //    }
            //}

        }
        procTexture.SetPixels(colorOut);
        procTexture.Apply();
    }

    Vector3 viewport2World(Vector2 vp)
    {
        return new Vector3(vp.x * 10.0f, 0.05f, vp.y * 10.0f - 3f);
    }

    Vector2Int viewport2Pixel(Vector2 vp)
    {
        return new Vector2Int((int)(vp.x * w + 0.5f), (int)(vp.y * w + 0.5f));
    }


    Vector3 Vector2ToVector3(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }


    Color InvertColor(Color c)
    {
        return new Color(1 - c.r, 1 - c.g, 1 - c.b);
    }


    public MotionPacket evalAtTime(float t)
    {
        MotionPacket mp = new MotionPacket();

        if (samples.Count == 0) return mp;
        mp = samples[0];
        MotionPacket p0, p1;

        if (t <= samples[0].time) { return samples[0]; }
        if (t >= samples[samples.Count - 1].time) { return samples[samples.Count - 1]; }

        for (int i = 0; i < samples.Count; ++i)
        {
            if (t <= samples[i].time)
            {
                p1 = samples[i];
                p0 = samples[i - 1];

                float nt = (t - p0.time) / (p1.time - p0.time);

                mp.centroid = p0.centroid * (1.0f - nt) + p1.centroid * nt;
                mp.speed = p0.speed * (1.0f - nt) + p1.speed * nt;
                mp.aMag = p0.aMag * (1.0f - nt) + p1.aMag * nt;
                mp.a = p0.a * (1.0f - nt) + p1.a * nt;
                mp.v = p0.v * (1.0f - nt) + p1.v * nt;
                mp.a_3 = p0.a_3 * (1.0f - nt) + p1.a_3 * nt;
                mp.v_3 = p0.v_3 * (1.0f - nt) + p1.v_3 * nt;
                mp.time = t;
                return mp;
            }
        }

        return mp;
    }


    Texture2D blitTargetTexture;
    Color[] getPixelsFromTexture(RenderTexture t)
    {
        if (blitTargetTexture==null || blitTargetTexture.width!=t.width || blitTargetTexture.height!=t.height)
        {
            if (blitTargetTexture != null) Texture2D.Destroy(blitTargetTexture);
            blitTargetTexture= new Texture2D(t.width, t.height, TextureFormat.RGBA32, false);
        }

        RenderTexture currentRT = RenderTexture.active;
        
        RenderTexture.active = t;
        blitTargetTexture.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
        blitTargetTexture.Apply();

        RenderTexture.active = currentRT;
        return blitTargetTexture.GetPixels();
    }
}


public class MotionPacket
{
    public Vector2 centroid; //smooth
    public Vector2 centroidInst;
    public float totalM;
    public float time;
    public Vector2 v;
    public float speed;
    public Vector2 a;
    public float aMag;

    //for computing cross product
    public Vector3 v_3;
    public Vector3 a_3;
}