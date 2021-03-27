using System;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class PersonTracker : MonoBehaviour 
{
    public BodySourceManager BodyManager;
    public VideoFeed colorSourceManager;
    public bool paintAllJoints;
    public int defaultRadius = 20;
    public Color defaultColor = Color.red;
    public List<TrackedJoint> trackedJoints = new List<TrackedJoint>();

    private Kinect.KinectSensor sensor;

    public event Action<string, Vector3> OnTrackedJointUpdated;


    private void Start()
    {
        sensor = Kinect.KinectSensor.GetDefault();
    }

    void Update () 
    {

        if (BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                RefreshBodyObject(body);
            }
        }
    }
    
    private void RefreshBodyObject(Kinect.Body body)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            
            TrackedJoint paintJoint = null;
            foreach (var pj in trackedJoints)
            {
                if (pj.jointType == jt)
                {
                    paintJoint = pj;
                    break;
                }
            }
            if (paintAllJoints || paintJoint != null)
            {
                Kinect.CameraSpacePoint pointToTransform = sourceJoint.Position;
                if (paintJoint != null)
                {
                    pointToTransform.X += paintJoint.offset_meters.x;
                    pointToTransform.Y += paintJoint.offset_meters.y;
                    pointToTransform.Z += paintJoint.offset_meters.z;
                }
                if (OnTrackedJointUpdated != null)
                    OnTrackedJointUpdated(paintJoint.name, GetVector3FromColorSpacePoint(pointToTransform));
                var colorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(pointToTransform);
                if (paintJoint != null && paintJoint.randomize)
                {
                    colorSourceManager.DrawCircleRandom(new Vector2Int((int)colorPoint.X, (int)colorPoint.Y),
                    paintJoint == null ? defaultRadius : (paintJoint.scaleWithDistance ? (int)(paintJoint.radius / sourceJoint.Position.Z) : paintJoint.radius));
                }
                else if (paintJoint != null && paintJoint.blur)
                {
                    colorSourceManager.BlurCircle(new Vector2Int((int)colorPoint.X, (int)colorPoint.Y),
                    paintJoint == null ? defaultRadius : (paintJoint.scaleWithDistance ? (int)(paintJoint.radius / sourceJoint.Position.Z) : paintJoint.radius));
                }
                else
                {
                    colorSourceManager.DrawCircle(new Vector2Int((int)colorPoint.X, (int)colorPoint.Y),
                        paintJoint == null ? defaultRadius : (paintJoint.scaleWithDistance ? (int)(paintJoint.radius / sourceJoint.Position.Z) : paintJoint.radius),
                        paintJoint == null ? defaultColor : paintJoint.color);
                }
            }
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    private static Vector3 GetVector3FromColorSpacePoint(Kinect.CameraSpacePoint point)
    {
        return new Vector3(point.X, point.Y, point.Z);
    }
}

[System.Serializable]
public class TrackedJoint
{
    public string name;
    public Kinect.JointType jointType;
    public int radius;
    public Vector3 offset_meters;
    public bool scaleWithDistance;
    public Color color;
    public bool blur;
    public bool randomize;
}