using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        float JumpVelocity;

        Rigidbody playerRigidBody;
        Collider playerCollider;

        private void Start()
        {
            playerRigidBody = GetComponent<Rigidbody>();
            playerCollider = GetComponent<Collider>();

            SetVelocity();
        }

        private void SetVelocity()
        {
            JumpVelocity = Gravity * JumpTime;
        }

        [ContextMenu("StartJump")]
        private void StartJump()
        {
            playerRigidBody.velocity = Vector3.up * JumpVelocity * 2;
        }

        private void Update()
        {
            playerRigidBody.velocity += Vector3.down * Gravity * Time.deltaTime;
        }
    }
}