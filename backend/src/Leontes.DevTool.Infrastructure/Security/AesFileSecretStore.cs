using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Leontes.DevTool.Application.Services;

namespace Leontes.DevTool.Infrastructure.Security;

/// <summary>
/// Secrets at rest in a single encrypted file. Values are sealed with AES-256-GCM under a key
/// derived (PBKDF2) from a per-machine/user identity, so the file is useless if copied elsewhere.
/// </summary>
public sealed class AesFileSecretStore : ISecretStore
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("leontes.devtool.secretstore.v1");

    private readonly string _filePath;
    private readonly byte[] _key;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, string> _entries;

    public AesFileSecretStore(string filePath)
    {
        _filePath = filePath;
        _key = DeriveKey();
        _entries = Load();
    }

    public string? Get(string key)
    {
        lock (_gate)
            return _entries.TryGetValue(key, out var sealed_) ? Decrypt(sealed_) : null;
    }

    public bool Has(string key)
    {
        lock (_gate)
            return _entries.ContainsKey(key);
    }

    public void Set(string key, string value)
    {
        lock (_gate)
        {
            _entries[key] = Encrypt(value);
            Persist();
        }
    }

    public void Delete(string key)
    {
        lock (_gate)
        {
            if (_entries.Remove(key))
                Persist();
        }
    }

    private string Encrypt(string plaintext)
    {
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        var combined = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, combined, NonceSize + TagSize, cipher.Length);
        return Convert.ToBase64String(combined);
    }

    private string Decrypt(string sealedValue)
    {
        var combined = Convert.FromBase64String(sealedValue);
        var nonce = combined.AsSpan(0, NonceSize);
        var tag = combined.AsSpan(NonceSize, TagSize);
        var cipher = combined.AsSpan(NonceSize + TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    private Dictionary<string, string> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private void Persist() =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_entries));

    private static byte[] DeriveKey()
    {
        var identity = $"{Environment.MachineName}|{Environment.UserName}";
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(identity), Salt, iterations: 200_000, HashAlgorithmName.SHA256, outputLength: 32);
    }
}
