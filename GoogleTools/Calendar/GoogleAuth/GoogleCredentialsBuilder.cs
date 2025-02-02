using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace GoogleAuth
{
    public class GoogleCredentialsBuilder
    {
        /// <summary>
        /// Create credentials for goolge access.
        /// </summary>
        /// <param name="credentialsPath"></param>
        /// <param name="tokenPath"></param>
        /// <returns></returns>
        public static UserCredential CreateFromFile(string credentialsPath, string tokenPath, string[] scopes)
        {
            UserCredential credentials = null;
            using (var stream =
            new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)).Result;
            }

            return credentials;
        }
    }
}
