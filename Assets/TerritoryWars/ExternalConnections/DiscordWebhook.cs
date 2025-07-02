using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace TerritoryWars.ExternalConnections
{
    [Serializable]
    public class DiscordWebhook
    {
        private readonly string webhookUrl;
        private readonly string username;

        public DiscordWebhook(string webhookUrl, string username = "Unity Reporter")
        {
            this.webhookUrl = webhookUrl;
            this.username = username;
        }

        [Serializable]
        private class EmbedField
        {
            public string name;
            public string value;
            public bool inline;
        }

        [Serializable]
        private class EmbedImage
        {
            public string url;
        }

        [Serializable]
        private class Embed
        {
            public string title;
            public string description;
            public int color;
            public List<EmbedField> fields = new List<EmbedField>();
            public EmbedImage image;
        }

        [Serializable]
        private class AllowedMentions
        {
            public List<string> users;
        }

        [Serializable]
        private class Payload
        {
            public string username;
            public string content;
            public AllowedMentions allowed_mentions;
            public List<Embed> embeds = new List<Embed>();
        }

        public IEnumerator SendEmbed(
            string title,
            string description,
            int color,
            List<(string name, string value, bool inline)> fields = null,
            byte[] imageData = null,
            string imageFileName = null,
            byte[] fileData = null,
            string fileName = null,
            string content = null,
            List<string> allowedMentionsUserIds = null)
        {
            WWWForm form = new WWWForm();

            var embed = new Embed
            {
                title = title,
                description = description,
                color = color,
                image = (imageData != null && !string.IsNullOrEmpty(imageFileName)) ? new EmbedImage { url = $"attachment://{imageFileName}" } : null
            };
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    embed.fields.Add(new EmbedField { name = f.name, value = f.value, inline = f.inline });
                }
            }

            var payload = new Payload
            {
                username = username,
                content = content,
                allowed_mentions = allowedMentionsUserIds != null ? new AllowedMentions { users = allowedMentionsUserIds } : null,
                embeds = new List<Embed> { embed }
            };

            string payloadJson = JsonUtility.ToJson(payload);
            form.AddField("payload_json", payloadJson);

            if (imageData != null && !string.IsNullOrEmpty(imageFileName))
            {
                form.AddBinaryData("files[0]", imageData, imageFileName, "image/png");
            }
            if (fileData != null && !string.IsNullOrEmpty(fileName))
            {
                form.AddBinaryData("files[1]", fileData, fileName, "text/plain");
            }

            using (UnityWebRequest www = UnityWebRequest.Post(webhookUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to send Discord webhook: {www.error}");
                }
            }
        }
    }
} 