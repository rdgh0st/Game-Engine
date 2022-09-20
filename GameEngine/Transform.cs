using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine;

public class Transform
{
    private Vector3 localPosition;
    private Quaternion localRotation;
    private Vector3 localScale;
    private Matrix world;
    private Transform parent;
    
    public Transform Parent
    {
        get { return parent; }
        set
        {
            if(parent != null) parent.Children.Remove(this);
            parent = value;
            if(parent != null) parent.Children.Add(this);
            UpdateWorld();
        }
    }
    private List<Transform> Children { get; set; }
    public Vector3 Position
    {
        get { return World.Translation; }
    }

    public Transform()
    {
        localPosition = Vector3.Zero;
        localRotation = Quaternion.Identity;
        localScale = Vector3.One;
        Children = new List<Transform>();
        UpdateWorld();
    }

    public Vector3 LocalPosition
    {
        get { return localPosition;} 
        set { localPosition = value;
            UpdateWorld();
        }
    }

    public Vector3 LocalScale
    {
        get { return localScale;} 
        set { localScale = value;
            UpdateWorld();
        }
    }

    public Quaternion LocalRotation
    {
        get { return localRotation;} 
        set { localRotation = value;
            UpdateWorld();
        }
    }
    
    public Matrix World { get { return world; } }
    
    public Vector3 Forward { get { return world.Forward; } }
    public Vector3 Backward { get { return world.Backward; } }
    public Vector3 Right { get { return world.Right; } }
    public Vector3 Left { get { return world.Left; } }
    public Vector3 Up { get { return world.Up; } }
    public Vector3 Down { get { return world.Down; } }

    private void UpdateWorld()
    {
        world = Matrix.CreateScale(localScale) * Matrix.CreateFromQuaternion(localRotation) *
                Matrix.CreateTranslation(localPosition);
        if(parent != null)
            world *= parent.World;
        foreach (Transform child in Children)
            child.UpdateWorld();
    }

    public void Rotate(Vector3 axis, float angle)
    {
        LocalRotation *= Quaternion.CreateFromAxisAngle(axis, angle);
    }

    

}