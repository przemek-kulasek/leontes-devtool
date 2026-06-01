namespace Leontes.DevTool.Application.Services;

/// <summary>Encrypted local key/value store for secrets (API tokens). Never written to artifacts.</summary>
public interface ISecretStore
{
    string? Get(string key);

    void Set(string key, string value);

    void Delete(string key);

    bool Has(string key);
}
