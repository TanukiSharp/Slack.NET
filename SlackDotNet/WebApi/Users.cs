using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Get information on members of your Slack team.
    /// </summary>
    public class Users
    {
        private WebApiClient client;

        internal Users(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// This method returns a list of all users in the team. This includes deleted/deactivated users.
        /// </summary>
        /// <param name="cursor">Paginate through collections of data by setting the cursor parameter to a next_cursor attribute returned by a previous request's response_metadata. Default value fetches the first "page" of the collection. See pagination on Slack documentation for more detail.</param>
        /// <param name="limit">The maximum number of items to return. Fewer than the requested number of items may be returned, even if the end of the users list hasn't been reached.</param>
        /// <param name="presence">Whether to include presence data in the output. Setting this to false improves performance, especially with large teams.</param>
        /// <returns>Returns a list of user objects, in no particular order.</returns>
        public Task<Response<UserListResponse>> List(bool presence, string cursor = null, int limit = 0)
        {
            IQueryBuilder query = QueryBuilder.Shared.Clear();

            if (cursor != null)
                query.Append("cursor", cursor);

            if (limit > 0)
                query.Append("limit", limit.ToString());

            query.Append("presence", StringConstants.FromBoolean(presence));

            return client.Call<UserListResponse>("users.list", query);
        }
    }
}
