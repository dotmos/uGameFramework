using UnityEngine;
using System.Collections;
using UniRx;
using System;

namespace Service.Camera{
    public class CameraService : GameComponent, ICameraService{
        /// <summary>
        /// Gets or sets the position offset which will be used while following a target
        /// </summary>
        /// <value>The position offset.</value>
        public Vector3 PositionOffset{get; set;}
        /// <summary>
        /// Gets or sets the rotation offset which will be used while following a target
        /// </summary>
        /// <value>The rotation offset.</value>
        public Vector3 DirectionOffset{get; set;}
        /// <summary>
        /// Gets or sets the position damping which will be used while following a target. Range: 0-1. If setting this to 1, camera will not move at all
        /// </summary>
        /// <value>The position damping.</value>
        public float PositionDamping{get; set;}
        /// <summary>
        /// Gets or sets the rotation damping which will be used while following a target.. Range: 0-1. If setting this
        /// to 0, camera will not rotate at all
        /// </summary>
        /// <value>The rotation damping.</value>
        public float RotationDamping{get; set;}

        /// <summary>
        /// Follow the target
        /// </summary>
        public bool DoFollowTarget { get; set; }

        /// <summary>
        /// The target follow disposable.
        /// </summary>
        IDisposable targetFollowDisposable;
        /// <summary>
        /// The target property.
        /// </summary>
        ReactiveProperty<Transform> TargetProperty = new ReactiveProperty<Transform>();
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        Transform Target {
            get{return TargetProperty.Value;}
            set{
                //Stop following the current target
                if(targetFollowDisposable != null) targetFollowDisposable.Dispose();

                //Set new target
                TargetProperty.Value = value;

                //Start following the new target if not null
                if(Target != null) targetFollowDisposable = Observable.EveryUpdate().Where(e => Target != null).Subscribe(e => FollowTarget()).AddTo(this);
            }
        }

        /// <summary>
        /// The camera property.
        /// </summary>
        ReactiveProperty<UnityEngine.Camera> CameraProperty = new ReactiveProperty<UnityEngine.Camera>();
        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        UnityEngine.Camera Camera {
            get{return CameraProperty.Value;}
            set{CameraProperty.Value = value;}
        }

        protected override void PreBind()
        {
            base.PreBind();
        }

        /// <summary>
        /// Sets the camera to use with the service
        /// </summary>
        /// <param name="camera">Camera.</param>
        public void SetCamera(UnityEngine.Camera camera){
            Camera = camera;
        }
        /// <summary>
        /// Set the position of the camera. Setting this will stop following a target (if set)
        /// </summary>
        /// <param name="position">Position.</param>
        public void SetPosition(Vector3 position){
            if(Camera == null) return;
            SetTarget(null);
            Camera.transform.position = position;
        }
        /// <summary>
        /// Sets the direction of the camera. Setting this will stop following a target (if set)
        /// </summary>
        /// <param name="direction">Direction.</param>
        public void SetDirection(Vector3 direction){
            if(Camera == null) return;
            SetTarget(null);
            Camera.transform.rotation = Quaternion.LookRotation(direction);
        }
        /// <summary>
        /// Start following the target, using PositionOffset and RotationOffset
        /// </summary>
        /// <param name="target">Target.</param>
        public void SetTarget(Transform target){
            Target = target;
            DoFollowTarget = true;
        }


        void FollowTarget(){
            if(Camera == null || !DoFollowTarget) return;
            
            //Set new camera position
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, Target.position + PositionOffset, 1-PositionDamping);
            //Make camera look at target
//            Vector3 dirToTarget = (Target.position - Camera.transform.position).normalized;
            //Offset direction
//            if(DirectionOffset.magnitude != 0) dirToTarget = Quaternion.LookRotation(DirectionOffset) * dirToTarget;
            //Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, Quaternion.LookRotation(dirToTarget), 1-RotationDamping);
        }
    }
}
