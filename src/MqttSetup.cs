using System;

namespace Mastersign.WinJockey
{
    partial class MqttSetup
    {
        private string lastKnownBaseTopic = null;
        private string expandedBaseTopic = null;

        public string ExpandedBaseTopic
        {
            get
            {
                if (!string.Equals(BaseTopic, lastKnownBaseTopic))
                {
                    if (BaseTopic is null)
                    {
                        expandedBaseTopic = null;
                        lastKnownBaseTopic = null;
                    }
                    else
                    {
                        expandedBaseTopic = Environment.ExpandEnvironmentVariables(BaseTopic);
                        lastKnownBaseTopic = BaseTopic;
                    }
                }
                return expandedBaseTopic;
            }
        }
    }
}
