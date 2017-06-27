using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SlackDotNet.WebApi
{
    /// <summary>
    /// Get information on your direct messages.
    /// </summary>
    public class IM
    {
        private WebApiClient client;

        internal IM(WebApiClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        /// <summary>
        /// Lists direct message channels for the calling user.
        /// </summary>
        /// <returns>This method returns a list of all im channels that the user has.</returns>
        public Task<Response<IMListResponse>> List()
        {
            return client.Call<IMListResponse>("im.list");
        }
    }
}
