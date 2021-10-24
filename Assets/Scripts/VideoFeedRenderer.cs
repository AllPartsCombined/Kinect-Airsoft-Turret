using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class VideoFeedRenderer : MonoBehaviour
{
    public VideoFeed videoFeed;
    public RawImage rawImage;
    
    void Start ()
    {
        //gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void Update()
    {
        if (videoFeed == null)
        {
            return;
        }
        //gameObject.GetComponent<Renderer>().material.mainTexture = videoFeed.GetColorTexture();
        rawImage.texture = videoFeed.GetColorTexture();
    }
}
