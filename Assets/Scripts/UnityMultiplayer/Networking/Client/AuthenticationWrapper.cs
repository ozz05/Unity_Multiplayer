using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static  AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;
    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }
        if (AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Already Authenticating!");
            await Authenticating();
            return AuthState;
        }
        await SingInAnonymouslyAsync(maxTries);
        
        return AuthState;
    }

    private static async Task<AuthState> Authenticating()
    {
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }
    private static async Task SingInAnonymouslyAsync(int maxRetries)
    {
        AuthState = AuthState.Authenticating;
        int retries = 0;
        while(AuthState == AuthState.Authenticating && retries < maxRetries)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if(AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }
            }
            catch(AuthenticationException authenticationException)
            {
                Debug.LogError(authenticationException);
                AuthState = AuthState.Error;
            }
            catch(RequestFailedException requestFailedException)
            {
                Debug.LogError(requestFailedException);
                AuthState = AuthState.Error;
            }

            retries ++;
            //waits 1000 miliseconds 
            await Task.Delay(1000);
        }
        if (AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player not signed in successfuly after {retries} tries");
            AuthState = AuthState.TimeOut;
        }
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}