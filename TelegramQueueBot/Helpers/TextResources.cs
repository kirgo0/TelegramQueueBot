using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Reflection;
using TelegramQueueBot.Models;
using TelegramQueueBot.Repository.Interfaces;

namespace TelegramQueueBot.Helpers
{
    public class TextResources
    {
        private const string NotFoundValue = "NOT FOUND";
        private static Dictionary<string, Text> texts = new();
        private static ILogger _log;
        public string this[string index]
        {
            get => GetValue(index);
        }

        public static async Task Load(ILogger<TextResources> log, ITextRepository repository, Type keysClass)
        {
            var keys = keysClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => f.GetRawConstantValue()?.ToString())
                .ToList();

            if (!keys.Any())
            {
                log.LogWarning("No constant strings found in the class of type {typeName}", keysClass.Name);
                return;
            }

            var missingKeys = new List<string>();
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(key)) continue;
                var text = await repository.GetByKeyAsync(key);
                if (text is null)
                {
                    missingKeys.Add(key);
                    continue;
                }
                texts.Add(key, text);
            }

            log.LogInformation("Loaded {texts}/{keys} text values from {repositoryName}",texts.Count, keys.Count, repository.GetType().Name);
            if (missingKeys.Count > 0)
            {
                log.LogWarning("Missing value/s for {list} key/s",string.Join(",", missingKeys));
            }
        }

        public static string GetValue(string key)
        {
            texts.TryGetValue(key, out var text);
            if (string.IsNullOrEmpty(text?.Value))
            {
                _log?.LogWarning("Missing the value for {key}", key);
            }
            return text?.Value ?? NotFoundValue;
        }
    }
}
