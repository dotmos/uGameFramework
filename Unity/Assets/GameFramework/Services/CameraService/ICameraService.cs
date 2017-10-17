using UnityEngine;
using Zenject;

namespace Service.Camera{
    public interface ICameraService{
        /// <summary>
        /// Sets the camera to use with the service
        /// </summary>
        /// <param name="camera">Camera.</param>
        void SetCamera(UnityEngine.Camera camera);
        /// <summary>
        /// Set the position of the camera. Setting this will stop following a target (if set)
        /// </summary>
        /// <param name="position">Position.</param>
        void SetPosition(Vector3 position);
        /// <summary>
        /// Sets the direction of the camera. Setting this will stop following a target (if set)
        /// </summary>
        /// <param name="direction">Direction.</param>
        void SetDirection(Vector3 direction);
        /// <summary>
        /// Start following the target, using PositionOffset, DirectionOffset, PositionDamping and RotationDamping
        /// </summary>
        /// <param name="target">Target.</param>
        void SetTarget(Transform target);
        /// <summary>
        /// Gets or sets the position offset which will be used while following a target
        /// </summary>
        /// <value>The position offset.</value>
        Vector3 PositionOffset{get; set;}
        /// <summary>
        /// Gets or sets the rotation offset which will be used while following a target
        /// </summary>
        /// <value>The rotation offset.</value>
        Vector3 DirectionOffset{get; set;}
        /// <summary>
        /// Gets or sets the position damping which will be used while following a target. Range: 0-1. If setting this to 1, camera will not move at all
        /// </summary>
        /// <value>The position damping.</value>
        float PositionDamping{get; set;}
        /// <summary>
        /// Gets or sets the rotation damping which will be used while following a target.. Range: 0-1. If setting this to 1, camera will not rotate at all
        /// </summary>
        /// <value>The rotation damping.</value>
        float RotationDamping{get; set;}
        /// <summary>
        /// Follow the target
        /// </summary>
        bool DoFollowTarget { get; set; }
    }
}
