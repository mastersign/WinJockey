#nullable enable

using System.Text.RegularExpressions;

namespace Mastersign.WinJockey;

partial class CommandConfiguration
{
    private string? lastKnownMqttBaseTopic = null;
    private string? lastKnownMqttTopic = null;
    private Regex? mqttTopicPattern = null;

    public bool MatchesMqttTopic(string baseTopic, string topic)
    {
        if (!string.Equals(baseTopic, lastKnownMqttBaseTopic) ||
            !string.Equals(MqttTopic, lastKnownMqttTopic))
        {
            mqttTopicPattern = new Regex(
                "^" 
                + Regex.Escape($"{baseTopic}{MqttTopic}")
                    .Replace("\\+", "[^/]+")
                    .Replace("#", ".*")
                + "$",
                RegexOptions.Compiled);
            lastKnownMqttBaseTopic = baseTopic;
            lastKnownMqttTopic = MqttTopic;
        }
        return mqttTopicPattern != null && mqttTopicPattern.IsMatch(topic);
    }

    internal string Source { get; set; }
}
