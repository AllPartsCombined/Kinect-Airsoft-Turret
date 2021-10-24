using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blur : MonoBehaviour
{
    public int kernalSize = 3;
    private float avgR = 0;
    private float avgG = 0;
    private float avgB = 0;
    private float avgA = 0;
    private float blurPixelCount = 0;

    public void FastBlur(Texture2D image, int radius, int iterations)
    {


        for (var i = 0; i < iterations; i++)
        {

            BlurImage(image, radius, true);
            BlurImage(image, radius, false);
        }

    }

    void BlurImage(Texture2D image, int blurSize, bool horizontal)
    {

        int _W = image.width;
        int _H = image.height;
        int xx, yy, x, y;

        if (horizontal)
        {

            for (yy = 0; yy < _H; yy++)
            {

                for (xx = 0; xx < _W; xx++)
                {

                    ResetPixel();

                    //Right side of pixel
                    for (x = xx; (x < xx + blurSize && x < _W); x++)
                    {

                        AddPixel(image.GetPixel(x, yy));
                    }

                    //Left side of pixel
                    for (x = xx; (x > xx - blurSize && x > 0); x--)
                    {

                        AddPixel(image.GetPixel(x, yy));
                    }

                    CalcPixel();

                    for (x = xx; x < xx + blurSize && x < _W; x++)
                    {

                        image.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));
                    }
                }
            }
        }

        else
        {

            for (xx = 0; xx < _W; xx++)
            {

                for (yy = 0; yy < _H; yy++)
                {

                    ResetPixel();

                    //Over pixel
                    for (y = yy; (y < yy + blurSize && y < _H); y++)
                    {

                        AddPixel(image.GetPixel(xx, y));
                    }

                    //Under pixel
                    for (y = yy; (y > yy - blurSize && y > 0); y--)
                    {

                        AddPixel(image.GetPixel(xx, y));
                    }

                    CalcPixel();

                    for (y = yy; y < yy + blurSize && y < _H; y++)
                    {

                        image.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));
                    }
                }
            }
        }

        //blurred.Apply();
        //return blurred;
    }

    public void BlurPixel(Texture2D image, int xx, int yy)
    {
        int _W = image.width;
        int _H = image.height;

        int x, y;

        ResetPixel();

        //Right side of pixel
        for (x = xx; (x < xx + kernalSize && x < _W); x++)
        {

            AddPixel(image.GetPixel(x, yy));
        }

        //Left side of pixel
        for (x = xx; (x > xx - kernalSize && x > 0); x--)
        {

            AddPixel(image.GetPixel(x, yy));
        }

        CalcPixel();

        for (x = xx; x < xx + kernalSize && x < _W; x++)
        {

            image.SetPixel(x, yy, new Color(avgR, avgG, avgB, 1.0f));
        }

        ResetPixel();

        //Over pixel
        for (y = yy; (y < yy + kernalSize && y < _H); y++)
        {

            AddPixel(image.GetPixel(xx, y));
        }

        //Under pixel
        for (y = yy; (y > yy - kernalSize && y > 0); y--)
        {

            AddPixel(image.GetPixel(xx, y));
        }

        CalcPixel();

        for (y = yy; y < yy + kernalSize && y < _H; y++)
        {

            image.SetPixel(xx, y, new Color(avgR, avgG, avgB, 1.0f));
        }

        //blurred.Apply();
        //return blurred;
    }


    void AddPixel(Color pixel)
    {

        avgR += pixel.r;
        avgG += pixel.g;
        avgB += pixel.b;
        blurPixelCount++;
    }

    void ResetPixel()
    {

        avgR = 0.0f;
        avgG = 0.0f;
        avgB = 0.0f;
        blurPixelCount = 0;
    }

    void CalcPixel()
    {

        avgR = avgR / blurPixelCount;
        avgG = avgG / blurPixelCount;
        avgB = avgB / blurPixelCount;
    }
}
