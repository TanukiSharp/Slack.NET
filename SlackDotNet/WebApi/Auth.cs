using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Contains authentication related functionalities.
    /// </summary>
    public class Auth
    {
        private WebApiClient client;

        internal Auth(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// This method checks authentication and tells you who you are.
        /// </summary>
        /// <returns>Returns basic information.</returns>
        public Task<Response<AuthTestResponse>> Test()
        {
            return client.Call<AuthTestResponse>("auth.test");
        }

        /// <summary>
        /// This method revokes an access token.
        /// Use it when you no longer need a token.
        /// For example, with a Sign In With Slack app, call this to log a user out.
        /// </summary>
        /// <param name="test">Setting this parameter to 1 triggers a testing mode where the specified token will not actually be revoked.</param>
        /// <returns>Returns whether the token was actually revoked</returns>
        public Task<Response<AuthRevokeResponse>> Revoke(bool test = false)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (test)
                query.Append("test", StringConstants.True);

            return client.Call<AuthRevokeResponse>("auth.revoke", query);
        }
    }
}
