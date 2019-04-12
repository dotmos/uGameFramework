using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UserInterface {
    public class GameInfoController : GameComponent
    {
        public Text systemStartOutput;
        public Text fpsOutput;

        const float fpsMeasurePeriod = 0.5f;

        private int frameCounter = 0;
        private float timeCounter = 0;

        private float nextFPSPeriod = 0;
        private int currentFPS;
        const string display = "{0} ({1}ms)";

        protected override void AfterBind() {
            base.AfterBind();

            nextFPSPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;

            this.OnEvent<Service.DevUIService.Events.GameInfoChanged>().Subscribe(e => {
                OutputTimeSinceStartup(e.systemStartupTime);
            }).AddTo(this);
        }

        public void OutputTimeSinceStartup(float time) {
            systemStartOutput.text = (Mathf.Round(time * 10) / 10).ToString() + "s";
        }

        private void Update() {
            // measure average frames per second
            frameCounter++;
            timeCounter += Time.deltaTime;

            if (Time.realtimeSinceStartup > nextFPSPeriod) {
                currentFPS = (int)(frameCounter / fpsMeasurePeriod);

                object[] args = new object[] { currentFPS, Mathf.Round((timeCounter / (float)frameCounter) * 10000f) / 10f };
                fpsOutput.text = string.Format(display, args);

                //Reset
                nextFPSPeriod += fpsMeasurePeriod;
                timeCounter = 0;
                frameCounter = 0;
            }
        }
    }
}
