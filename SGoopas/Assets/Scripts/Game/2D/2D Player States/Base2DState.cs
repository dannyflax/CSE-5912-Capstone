﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    public abstract class Base2DState : BasePlayerState
    {
        private float airMoveForce = 1f;
        private float jumpForce = 1f;
        private float airDashForce = 1f;
        private float walkForce = 1f;
        private float dJumpForce = 1f;
        private float maxHoriSpeed = 1f;
        private float maxVertSpeed = 1f;
        private float dashStartAngle = 90f;
        private float dashDistance = 5f;
        private float enemyDetectionAngle = 15f;
        private float dashTime = 0.0f;
        private const float boxCastHeight = 0.1f;

        protected Rigidbody2D rb;
        private Vector2 linearVelocity;
        private float angularVelocity;
        private Transform groundCheck;
        private Vector3 original2DPosition;
        private Movement2DConfig mc;
        private bool lookingForGroundedPosition = true;
        private static GameObject swapMarker;
        private GameObject swapMarkerPrefab;

        protected Animator anim;
        protected Color mat;
        protected float characterWidth;
        protected bool dJump;
        protected bool dash;

        protected Base2DState(BasePlayerState previousState) : base(previousState)
        {
            if (previousState is Base2DState)
            {
                Base2DState previousState2D = (Base2DState)previousState;
                mc = previousState2D.mc;
                rb = previousState2D.rb;

                swapMarkerPrefab = previousState2D.swapMarkerPrefab;
                anim = previousState2D.anim;
                mat = previousState2D.mat;
                characterWidth = previousState2D.characterWidth;
                linearVelocity = previousState2D.linearVelocity;
                angularVelocity = previousState2D.angularVelocity;
                groundCheck = previousState2D.groundCheck;
                dJump = previousState2D.dJump;
                dash = previousState2D.dash;
                original2DPosition = previousState2D.original2DPosition;
                lookingForGroundedPosition = previousState2D.lookingForGroundedPosition;
            }
        }

        protected Base2DState(GameObject player, MasterPlayerStateMachine playerStateMachine, Transform groundCheck) : base(player, playerStateMachine)
        {
            mc = PlayerObject.GetComponent<Movement2DConfig>();
            rb = PlayerObject.GetComponent<Rigidbody2D>();

            anim = PlayerObject.GetComponent<Animator>();

            characterWidth = PlayerObject.GetComponent<Collider2D>().bounds.size.x/1.2f;

            mat = PlayerObject.GetComponent<SpriteRenderer>().color;
            mat.a = 0.7f;
            PlayerObject.GetComponent<SpriteRenderer>().color = mat;

            swapMarkerPrefab = (GameObject)Resources.Load("swap");

            linearVelocity = new Vector2();
            angularVelocity = 0.0f;
            this.groundCheck = groundCheck;
            dash = true;
            dJump = true;
        }

        protected Transform GroundCheck {
            get
            {
                return groundCheck;
            }
        }

        public float AirMoveForce
        {
            get
            {
                if (mc)
                    return mc.airMoveForce;
                return airMoveForce;
            }
        }

        public float DashStartAngle
        {
            get
            {
                if (mc)
                    return mc.dashStartAngle;
                return dashStartAngle;
            }
        }
        public float DashTime
        {
            get
            {
                if (mc)
                    return mc.dashTime;
                return dashTime;
            }
        }
        public float DashDistance
        {
            get
            {
                if (mc)
                    return mc.dashDistance;
                return dashDistance;
            }
        }
        public float EnemyDetectionAngle
        {
            get
            {
                if (mc)
                    return mc.enemyDetectionAngle;
                return enemyDetectionAngle;
            }
        }


        public float JumpForce
        {
            get
            {
                if (mc)
                    return mc.jumpForce;
                return jumpForce;
            }
        }

        public float AirDashForce
        {
            get
            {
                if (mc)
                    return mc.airDashForce;
                return airDashForce;
            }
        }

        public float WalkForce
        {
            get
            {
                if (mc)
                    return mc.walkForce;
                return walkForce;
            }
        }

        public float DJumpForce
        {
            get
            {
                if (mc)
                    return mc.dJumpForce;
                return dJumpForce;
            }
        }

        public float MaxHoriSpeed
        {
            get
            {
                if (mc)
                    return mc.maxHoriSpeed;
                return maxHoriSpeed;
            }
        }

        public float MaxVertSpeed
        {
            get
            {
                if (mc)
                    return mc.maxVertSpeed;
                return maxVertSpeed;
            }
        }

        public override void MoveUp()
        {
            // Commented out because MoveUp() is handled in the controller as an axis and will continually call Jump() while held down
            // Jump();
        }

        public override void StoreState()
        {
            rb.isKinematic = true;
            linearVelocity = rb.velocity;
            rb.velocity = new Vector2();
            angularVelocity = rb.angularVelocity;
            rb.angularVelocity = 0.0f;
        }

        public override void RestoreState()
        {
            rb.isKinematic = false;

            rb.velocity = linearVelocity;
            rb.angularVelocity = angularVelocity;
            lookingForGroundedPosition = true;
        }

        public override void Update()
        {
            if (IsGrounded && lookingForGroundedPosition) {
                // Respawn player 2D at the first ground position found after a switch.
                // This prevents the case where the player gets stuck in the spot of death due to the user switching to 3D right beforehand.
                original2DPosition = rb.gameObject.transform.position;
                lookingForGroundedPosition = false;

                //Also place a swap marker at that position
                if (swapMarker != null)
                    Object.Destroy(swapMarker);
                swapMarker = Object.Instantiate(swapMarkerPrefab, PlayerObject.transform.root);
                swapMarker.transform.position = PlayerObject.transform.position;
                swapMarker.transform.localScale = PlayerObject.transform.localScale;
            }
        }

        public override void LateUpdate()
        {

        }

        public override void Death() {
            SetState(new DeadState2D(this));
        }

        protected void ResetState()
        {
            rb.velocity = new Vector2();
            rb.angularVelocity = 0;
            rb.gameObject.transform.position = original2DPosition;
        }

        protected bool IsGrounded
        {
            get {
                Vector3 playerPosition = PlayerObject.transform.position;
                Vector3 ground = GroundCheck.position;

                return Physics2D.BoxCast(playerPosition, new Vector2(characterWidth, boxCastHeight), 0f, ground - playerPosition, (ground - playerPosition).magnitude, ~(1 << LayerMask.NameToLayer("Player")));

            }
        }

        protected Vector2 DashVector
        {
            get
            {
                Vector2 dashVector;
                if(PlayerObject.transform.localScale.x < 0)
                    dashVector = Quaternion.Euler(0, 0, DashStartAngle) * PlayerObject.transform.up;
                else
                    dashVector = Quaternion.Euler(0, 0, -DashStartAngle) * PlayerObject.transform.up;
                bool trig = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = true;
                Collider2D[] enemies = Physics2D.OverlapCircleAll(PlayerObject.transform.position, DashDistance, 1 << LayerMask.NameToLayer("Enemy"));
                Physics2D.queriesHitTriggers = trig;
                if (enemies.Length != 0)
                {
                    Dictionary<Collider2D, float> angleEnemies = new Dictionary<Collider2D, float>();
                    foreach(Collider2D enemy in enemies)
                    {
                        float angle = Vector2.Angle((enemy.transform.position - PlayerObject.transform.position), dashVector);
                        if (angle <= EnemyDetectionAngle)
                            angleEnemies.Add(enemy, angle);
                    }
                    Collider2D minEnemy = null;
                    float minAngle = float.MaxValue;
                    foreach(KeyValuePair<Collider2D, float> enemy in angleEnemies)
                    {
                        if(enemy.Value <= minAngle)
                        {
                            minAngle = enemy.Value;
                            minEnemy = enemy.Key;
                        }
                    }
                    if (minEnemy != null)
                        dashVector = minEnemy.transform.position - PlayerObject.transform.position;
                }
                return dashVector.normalized;
            }
        }

        public override void ExitLevel()
        {
            anim.SetBool("exit", true);
            SetState(new ExitLevel2D(this));
        }

        public override void EnemyCollision(GameObject Enemy)
        {

        }

        protected void MakeSpriteFaceRight()
        {
            Vector3 prevScale = PlayerObject.transform.localScale;
            prevScale.x = Mathf.Abs(prevScale.x);
            PlayerObject.transform.localScale = prevScale;
        }

        protected void MakeSpriteFaceLeft()
        {
            Vector3 prevScale = PlayerObject.transform.localScale;
            prevScale.x = -Mathf.Abs(prevScale.x);
            PlayerObject.transform.localScale = prevScale;
        }

    }
}
