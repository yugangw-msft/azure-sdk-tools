﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Commands.Common.Factories
{
    public class AuthenticationFactory : IAuthenticationFactory
    {
        private const string CommonAdTenant = "Common";

        public AuthenticationFactory()
        {
            TokenProvider = new AdalTokenProvider();
        }

        public ITokenProvider TokenProvider { get; set; }

        public IAccessToken Authenticate(AzureEnvironment environment, ref UserCredentials credentials)
        {
            return Authenticate(environment, CommonAdTenant, ref credentials);
        }

        public IAccessToken Authenticate(AzureEnvironment environment, string tenant, ref UserCredentials credentials)
        {
            var token = TokenProvider.GetAccessToken(GetAdalConfiguration(environment, CommonAdTenant), credentials.ShowDialog, credentials.UserName, credentials.Password);
            credentials.UserName = token.UserId;
            return token;
        }

        public SubscriptionCloudCredentials GetSubscriptionCloudCredentials(AzureSubscription subscription, AzureProfile profile)
        {
            if (subscription == null)
            {
                throw new ApplicationException(Resources.InvalidCurrentSubscription);
            }

            var accountId = subscription.GetProperty(AzureSubscription.Property.AzureAccount);

            if (AzureSession.SubscriptionTokenCache.ContainsKey(subscription.Id))
            {
                return new AccessTokenCredential(subscription.Id, AzureSession.SubscriptionTokenCache[subscription.Id]);
            }
            else if (accountId != null)
            {
                Debug.Assert(profile.Accounts.ContainsKey(accountId));

                switch (profile.Accounts[accountId].Type)
                {
                    case AzureAccount.AccountType.User:
                        if (!AzureSession.SubscriptionTokenCache.ContainsKey(subscription.Id))
                        {
                            throw new ArgumentException(Resources.InvalidSubscriptionState);
                        }
                        return new AccessTokenCredential(subscription.Id, AzureSession.SubscriptionTokenCache[subscription.Id]);

                    case AzureAccount.AccountType.Certificate:
                        var certificate = ProfileClient.DataStore.GetCertificate(accountId);
                        return new CertificateCloudCredentials(subscription.Id.ToString(), certificate);

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new ArgumentException(Resources.InvalidSubscriptionState);
            }
        }

        private AdalConfiguration GetAdalConfiguration(AzureEnvironment environment, string tenantId)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            var adEndpoint = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryEndpoint];
            var adResourceId = environment.Endpoints[AzureEnvironment.Endpoint.ActiveDirectoryServiceEndpointResourceId];

            return new AdalConfiguration
            {
                AdEndpoint = adEndpoint,
                ResourceClientUri = adResourceId,
                AdDomain = tenantId
            };
        }
    }
}
