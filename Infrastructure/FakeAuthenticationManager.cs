using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace ClinicApp.Infrastructure;

public class FakeAuthenticationManager : IAuthenticationManager
{
    public ClaimsPrincipal User { get; set; } = new ClaimsPrincipal(new ClaimsIdentity());
    public AuthenticationResponseChallenge AuthenticationResponseChallenge { get; set; }
    public AuthenticationResponseGrant AuthenticationResponseGrant { get; set; }
    public AuthenticationResponseRevoke AuthenticationResponseRevoke { get; set; }

    public Task<IEnumerable<AuthenticateResult>> AuthenticateAsync(string[] authenticationTypes)
    {
        throw new NotImplementedException();
    }

    void IAuthenticationManager.Challenge(AuthenticationProperties properties, params string[] authenticationTypes)
    {
        throw new NotImplementedException();
    }

    void IAuthenticationManager.Challenge(params string[] authenticationTypes)
    {
        throw new NotImplementedException();
    }

    public AuthenticationResponseChallenge Challenge(AuthenticationProperties properties, params string[] authenticationTypes) => new AuthenticationResponseChallenge(new string[0], new AuthenticationProperties());
    public AuthenticationResponseChallenge Challenge(params string[] authenticationTypes) => new AuthenticationResponseChallenge(new string[0], new AuthenticationProperties());
    public IEnumerable<AuthenticationDescription> GetAuthenticationTypes() => new List<AuthenticationDescription>();
    public IEnumerable<AuthenticationDescription> GetAuthenticationTypes(Func<AuthenticationDescription, bool> predicate) => new List<AuthenticationDescription>();
    public Task<AuthenticateResult> AuthenticateAsync(string authenticationType)
    {
        throw new NotImplementedException();
    }

    public void SignIn(AuthenticationProperties properties, params ClaimsIdentity[] identities) { }
    public void SignIn(params ClaimsIdentity[] identities) { }
    public void SignOut(AuthenticationProperties properties, params string[] authenticationTypes) { }
    public void SignOut(params string[] authenticationTypes) { }
}