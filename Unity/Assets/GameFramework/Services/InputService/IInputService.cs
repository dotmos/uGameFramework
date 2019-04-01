using System;

namespace Service.Input{    
    public interface IInputService {

        void Tick(float deltaTime);

        /// <summary>
        /// Enables or disables input
        /// </summary>
        /// <param name="enable"></param>
        void EnableInput(bool enable);

        /// <summary>
        /// Registers a new button. If the input already exists, an additional handler will be added, resulting in the input being set by multiple handlers
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="downHandler">Down handler.</param>
        /// <param name="upHandler">Up handler.</param>
        /// <param name="holdHandler">Hold handler.</param>
        void RegisterButton(object input, Func<bool> downHandler, Func<bool> upHandler, Func<bool> holdHandler);

        /// <summary>
        /// Removes the button.
        /// </summary>
        /// <param name="input">Input.</param>
        void RemoveButton(object input);

        /// <summary>
        /// Registers a new axis. If the axis already exists, an additional handler will be added, resulting in the input being set by multiple handlers
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="axisHandler">Axis handler.</param>
        void RegisterAxis(object input, Func<float> axisHandler);

        /// <summary>
        /// Removes the axis.
        /// </summary>
        /// <param name="input">Input.</param>
        void RemoveAxis(object input);

        /// <summary>
        /// Return true if the input has just been pressed down in this frame
        /// </summary>
        /// <returns><c>true</c>, if button down was gotten, <c>false</c> otherwise.</returns>
        /// <param name="input">Input.</param>
        bool GetButtonDown(object input);

        /// <summary>
        /// Returns true if the input has just been released in this frame
        /// </summary>
        /// <returns><c>true</c>, if button up was gotten, <c>false</c> otherwise.</returns>
        /// <param name="input">Input.</param>
        bool GetButtonUp(object input);

        /// <summary>
        /// Returns true if the input is being hold down
        /// </summary>
        /// <returns><c>true</c>, if button was gotten, <c>false</c> otherwise.</returns>
        /// <param name="input">Input.</param>
        bool GetButton(object input);

        /// <summary>
        /// Returns the current value of the supplied axis
        /// </summary>
        /// <returns>The axis.</returns>
        /// <param name="input">Input.</param>
        float GetAxis(object input);
    }
}