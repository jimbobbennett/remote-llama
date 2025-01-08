using System.Text.Json;

namespace RemoteLlama;

/// <summary>
/// Manages application configuration settings, storing them in a JSON file in the user's AppData folder.
/// This static class provides thread-safe access to configuration values.
/// </summary>
internal static class ConfigManager
{
    /// <summary>
    /// The base directory path where configuration files are stored.
    /// Located in the user's AppData/Roaming/RemoteLlama folder.
    /// </summary>
    private static readonly string BaseDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RemoteLlama"
    );

    /// <summary>
    /// The full path to the configuration JSON file.
    /// </summary>
    private static readonly string ConfigPath = Path.Combine(BaseDirectory, "config.json");

    /// <summary>
    /// Cached configuration instance to avoid repeated file I/O operations.
    /// </summary>
    private static Config? _config;

    /// <summary>
    /// JSON serialization options used for reading and writing the config file.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Gets or sets the URL configuration value.
    /// Changes are automatically persisted to the configuration file.
    /// </summary>
    public static string Url
    {
        get => GetConfig().Url;
        set
        {
            var config = GetConfig();
            config.Url = value;
            SaveConfig(config);
        }
    }

    /// <summary>
    /// Gets or sets the model redirects configuration value.
    /// </summary>
    public static List<ModelRedirect> ModelRedirects
    {
        get => GetConfig().ModelRedirects;
        set
        {
            var config = GetConfig();
            config.ModelRedirects = value;
            SaveConfig(config);
        }
    }

    /// <summary>
    /// Retrieves the destination model for a given source model, if a redirect exists.
    /// If no redirect exists, the source model is returned.
    /// Redirects are used by the run command, and the /generate API to map one model to another.
    /// </summary>
    /// <param name="source">The source model for the redirect</param>
    /// <returns>The destination model for the redirect, or the source if there is no redirect</returns>
    public static string GetRedirectedModel(string source) => GetConfig().ModelRedirects.FirstOrDefault(r => r.Source == source)?.Destination ?? source;

    /// <summary>
    /// Retrieves the current configuration, creating it if it doesn't exist.
    /// </summary>
    /// <returns>The current configuration object.</returns>
    private static Config GetConfig()
    {
        if (_config != null) return _config;

        Directory.CreateDirectory(BaseDirectory);
        
        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            _config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }
        else
        {
            _config = new Config();
            SaveConfig(_config);
        }

        return _config;
    }

    /// <summary>
    /// Saves the configuration to the JSON file and updates the cached instance.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    private static void SaveConfig(Config config)
    {
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        File.WriteAllText(ConfigPath, json);
        _config = config;
    }

    public class ModelRedirect
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
    }

    /// <summary>
    /// Internal configuration class that defines the structure of the config file.
    /// </summary>
    private class Config
    {
        /// <summary>
        /// The URL configuration value.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Model redirects to use to redirect calls from one model to another
        /// </summary>
        public List<ModelRedirect> ModelRedirects { get; set; } = new();
    }
} 