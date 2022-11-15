using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine;

public class PlayerController : Component, IUpdateable
{
    public Vector3 target { get; set; }
    public float TurnSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float distanceToTarget { get; set; } = 0.5f;
    public enum State
    {
        Turning, Pathing, Shooting, ShootCooldown, Interacting, Still
    }

    public State CurrentState { get; set; } = State.Still;
    public float TimeToShoot { get; set; } = 0.5f;
    private float shotTimer = 1.5f;
    
    public PlayerController(Vector3 t)
    {
        target = t;
    }

    public void Update()
    {
        switch (CurrentState)
        {
            case State.Still:
                return;
            case State.Turning:
                Rotate();
                break;
            case State.Pathing:
                Move();
                break;
            case State.Shooting:
                shotTimer = 0;
                CurrentState = State.ShootCooldown;
                break;
            case State.ShootCooldown:
                shotTimer += Time.ElapsedGameTime;
                if (shotTimer >= TimeToShoot)
                {
                    CurrentState = State.Shooting;
                }
                break;
        }
    }

    private void Rotate()
    {
        Vector3 newForward = Vector3.Normalize(target - Transform.Position);

        Quaternion currentRot = GetRotation(Vector3.Forward, Transform.Forward, Vector3.Up, out var currentAngle);
        Quaternion targetRot = GetRotation(Vector3.Forward, newForward, Vector3.Up, out var rotAngle);

        if (Math.Abs(currentAngle - rotAngle) < 0.15f)
        {
            CurrentState = State.Pathing;
        }
            
        float realTurnSpeed = TurnSpeed / Math.Abs(rotAngle - currentAngle);

        Transform.Rotation = Quaternion.Slerp(currentRot, targetRot, realTurnSpeed * Time.ElapsedGameTime);
    }

    private void Move()
    {
        float distance = Vector3.Distance(Transform.Position, target);
        if (Math.Abs(distance) < distanceToTarget)
        {
            if (distanceToTarget == 0.5f)
            {
                CurrentState = State.Still;
            }
            else
            {
                CurrentState = State.ShootCooldown;
            }
        }
        float finalSpeed = (distance / MoveSpeed);
        Transform.Position = Vector3.Lerp(Transform.Position, target, Time.ElapsedGameTime / finalSpeed);
    }

    private static Quaternion GetRotation(Vector3 source, Vector3 dest, Vector3 up, out float rotAngle)
    {
        float dot = Vector3.Dot(source, dest);
        rotAngle = (float)Math.Acos(dot);

        if (Math.Abs(dot - (-1.0f)) < 0.000001f)
        {
            // vector a and b point exactly in the opposite direction, 
            // so it is a 180 degrees turn around the up-axis
            return new Quaternion(up, MathHelper.ToRadians(180.0f));
        }
        if (Math.Abs(dot - (1.0f)) < 0.000001f)
        {
            // vector a and b point exactly in the same direction
            // so we return the identity quaternion
            return Quaternion.Identity;
        }

        Vector3 rotAxis = Vector3.Cross(source, dest);
        rotAxis = Vector3.Normalize(rotAxis);
        return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
    }
}