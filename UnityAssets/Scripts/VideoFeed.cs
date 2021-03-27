using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class VideoFeed : MonoBehaviour
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    public Blur blur;

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private Texture2D _Texture;
    private byte[] _Data;
    private List<PixelDrawing> pixelDrawings = new List<PixelDrawing>();
    private List<PixelDrawing> randomDrawings = new List<PixelDrawing>();
    private List<Vector2Int> blurredPixels = new List<Vector2Int>();


    public Texture2D GetColorTexture()
    {
        return _Texture;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;

            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            _Data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();

            if (frame != null)
            {
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Rgba);
                _Texture.LoadRawTextureData(_Data);
                DrawAllPixels(_Texture);
                DrawAllRandomPixels(_Texture);
                BlurAllPixels(_Texture);
                _Texture.Apply();

                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    private void DrawAllPixels(Texture2D t)
    {
        foreach (PixelDrawing pd in pixelDrawings)
        {
            t.SetPixel(pd.location.x, pd.location.y, pd.color);
        }
        //t.Apply();
        pixelDrawings.Clear();
    }

    private void DrawAllRandomPixels(Texture2D t)
    {
        foreach (PixelDrawing pd in randomDrawings)
        {
            t.SetPixel(pd.location.x, pd.location.y, pd.color);
        }
        //t.Apply();
        randomDrawings.Clear();
    }

    private void BlurAllPixels(Texture2D t)
    {
        foreach (Vector2Int v in blurredPixels)
        {
            blur.BlurPixel(t, v.x, v.y);
        }
        //t.Apply();
        blurredPixels.Clear();
    }

    public void DrawDot(Vector2Int location, Color color)
    {
        pixelDrawings.Add(new PixelDrawing(location, color));
    }

    public void DrawCircleRandom(Vector2Int location, int radius)
    {
        float rSquared = radius * radius;
        for (int u = location.x - radius; u < location.x + radius + 1; u++)
        {
            for (int v = location.y - radius; v < location.y + radius + 1; v++)
            {
                if ((location.x - u) * (location.x - u) + (location.y - v) * (location.y - v) < rSquared)
                    pixelDrawings.Add(new PixelDrawing(new Vector2Int(u, v), new Color(Random.Range(0f, .7f), Random.Range(0f, .7f), Random.Range(0f, .7f))));
            }
        }
    }

    public void DrawCircle(Vector2Int location, int radius, Color color)
    {
        float rSquared = radius * radius;
        for (int u = location.x - radius; u < location.x + radius + 1; u++)
        {
            for (int v = location.y - radius; v < location.y + radius + 1; v++)
            {
                if ((location.x - u) * (location.x - u) + (location.y - v) * (location.y - v) < rSquared)
                    pixelDrawings.Add(new PixelDrawing(new Vector2Int(u, v), color));
            }
        }
    }

    public void BlurCircle(Vector2Int location, int radius)
    {
        float rSquared = radius * radius;
        for (int u = location.x - radius; u < location.x + radius + 1; u++)
        {
            for (int v = location.y - radius; v < location.y + radius + 1; v++)
            {
                if ((location.x - u) * (location.x - u) + (location.y - v) * (location.y - v) < rSquared)
                    blurredPixels.Add(new Vector2Int(u, v));
            }
        }
    }
}

public struct PixelDrawing
{
    public PixelDrawing(Vector2Int l, Color c)
    {
        location = l;
        color = c;
    }

    public Vector2Int location;
    public Color color;
}