using Application.Common.Interfaces;
using Application.Helpers;
using Infrastructure.Shared;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class SecurityService : ISecurityService
{
    private readonly SecuritySettings _securitySettings;

    public SecurityService(IOptions<SecuritySettings> securitySettingsOptions)
    {
        _securitySettings = securitySettingsOptions.Value;
    }

    public string Hash(string input, string salt)
    {
        return input.HashPasswordWith(salt, _securitySettings.Pepper);
    }
}