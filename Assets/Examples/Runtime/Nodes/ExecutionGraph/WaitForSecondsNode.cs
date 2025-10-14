using GraphProcessor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BBBirder.Graphs
{
    [System.Serializable, NodeMenuItem("Wait/" + DisplayName)]
    public partial class WaitForSecondsNode : LinearEXNode
    {
        const string DisplayName = "Wait For Seconds";

        [Input, ShowAsDrawer]
        public float seconds = 1;

        public bool unscaledTime;

        public override string name => DisplayName;
        private float expireTimeInSeconds;
        public override void Enter()
        {
            expireTimeInSeconds = GetTime(unscaledTime) + seconds;
        }

        public override bool MoveNext()
        {
            var now = GetTime(unscaledTime);
            if (now < expireTimeInSeconds)
            {
                return true;
            }
            else
            {
                EnqueueExecutionPort(outputPorts[0]);
                return false;
            }
        }

        public float GetTime(bool unscaledTime)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (unscaledTime)
                {
                    return Time.realtimeSinceStartup;
                }
                else
                {
                    return Time.time;
                }
            }
            else
            {
                return (float)EditorApplication.timeSinceStartup;
            }
#else
            if (unscaledTime)
            {
                return Time.realtimeSinceStartup;
            }
            else
            {
                return Time.time;
            }
#endif
        }
    }
}
