using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine;

/*
 * ABILITY IDEAS:
 * AOE ATTACK SWORD SLICE AROUND YOU
 * DASH THAT DAMAGES
 * KNOCKBACK YOURSELF AND ENEMY
 * SHIELD?
 * RECALL LIKE TRACER
 * HARPOON
 * DAMAGE OVER TIME AS CREW BOARDS YOUR SHIP
 * ULT TO IMMEDIATELY BOARD YOUR SHIP
 * 
 */

public class PlayerController : Component, IUpdateable
{
    public Vector3 target { get; set; }
    public float TurnSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float distanceToTarget { get; set; } = 0.5f;
    public float aoeCooldown { get; set; } = 5f;
    public float harpoonCooldown { get; set; } = 5f;
    public float rushCooldown { get; set; } = 10f;
    public float ultCooldown { get; set; } = 25f;
    public enum State
    {
        Turning, Pathing, Shooting, ShootCooldown, Harpooning, Still
    }

    public State CurrentState { get; set; } = State.Still;
    public float TimeToShoot { get; set; } = 0.5f;
    private float shotTimer = 1.5f;
    private float aoeTimer = 5f;
    private float harpoonTimer { get; set; } = 5f;
    private float rushTimer = 10f;
    private float ultTimer = 25f;
    
    public PlayerController(Vector3 t)
    {
        target = t;
    }

    public void Update()
    {
        aoeTimer += Time.ElapsedGameTime;
        harpoonTimer += Time.ElapsedGameTime;
        shotTimer += Time.ElapsedGameTime;
        rushTimer += Time.ElapsedGameTime;
        ultTimer += Time.ElapsedGameTime;
        
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
        if (currentRot == targetRot)
        {
            return;
        }

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
                return;
            }
            else
            {
                CurrentState = State.ShootCooldown;
                return;
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

    public bool canAOE()
    {
        if (aoeTimer >= aoeCooldown)
        {
            aoeTimer = 0;
            return true;
        }
        return false;
    }

    public bool canHarpoon()
    {
        if (harpoonTimer >= harpoonCooldown)
        {
            harpoonTimer = 0;
            return true;
        }

        return false;
    }
    
    public bool canRush()
    {
        if (rushTimer >= rushCooldown)
        {
            rushTimer = 0;
            return true;
        }

        return false;
    }
    
    public bool canUlt(Vector3 enemyPos)
    {
        if (ultTimer >= ultCooldown && Vector3.Distance(Transform.Position, enemyPos) < 10)
        {
            ultTimer = 0;
            return true;
        }

        return false;
    }
}