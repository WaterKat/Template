﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaterKat.MathW;

namespace WaterKat.WKPlayer
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Jump : MonoBehaviour
    {
        public enum CalculationModes
        {
            Height_Time = 0,
            Gravity_Height = 1,
            Gravity_Time = 2,
        }

        public CalculationModes calculationMode = CalculationModes.Height_Time;

        public float Gravity = 10.0f;
        public float JumpHeight = 5.0f;
        public float JumpTime = 0.5f;

        [SerializeField]
        float JumpVelocity = 0.0f;

        Rigidbody playerRigidBody;
        Collider playerCollider;

        private void Start()
        {
            playerRigidBody = GetComponent<Rigidbody>();
            playerCollider = GetComponent<Collider>();

            CalculateJumpValues();
        }

        private void CalculateJumpValues()
        {
            switch (calculationMode)
            {
                case CalculationModes.Height_Time:
                    Gravity = (JumpHeight * 2) / (Mathf.Pow(JumpTime, 2));
                    break;
                case CalculationModes.Gravity_Height:
                    JumpTime = Mathf.Sqrt(2 * JumpHeight / Gravity);
                    break;
                case CalculationModes.Gravity_Time:
                    JumpHeight = (Gravity / 2 * Mathf.Pow(JumpTime, 2));
                    break;
            }
            JumpVelocity = Gravity * JumpTime * (1 + (0.05f / (JumpTime + 1)));
        }

        [ContextMenu("StartJump")]
        private void StartJump()
        {
            playerRigidBody.velocity = Vector3.up * JumpVelocity;
        }

        private void FixedUpdate()
        {
            CalculateJumpValues();
            playerRigidBody.velocity += Vector3.down * Gravity * UnityEngine.Time.fixedDeltaTime;
        }

        private void Update()
        {
            if (Input.GetKeyDown("space"))
            {
                StartJump();
            }
        }
    }
}