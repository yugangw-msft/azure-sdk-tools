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

using Microsoft.WindowsAzure.Commands.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public partial class AzureAccount
    {
        public AzureAccount()
        {
            Properties = new Dictionary<Property,string>();
        }

        public string GetProperty(Property property)
        {
            return Properties.GetProperty(property);
        }

        public string[] GetPropertyAsArray(Property property)
        {
            return Properties.GetPropertyAsArray(property);
        }

        public void SetProperty(Property property, params string[] values)
        {
            Properties.SetProperty(property, values);
        }

        public bool IsPropertySet(Property property)
        {
            return Properties.IsPropertySet(property);
        }

        public List<AzureSubscription> GetSubscriptions(AzureProfile profile)
        {
            string subscriptions = string.Empty;
            List<AzureSubscription> subscriptionsList = new List<AzureSubscription>();
            Properties.TryGetValue(Property.Subscriptions, out subscriptions);

            foreach (var subscription in subscriptions.Split(','))
            {
                Guid subscriptionId = new Guid(subscription);
                Debug.Assert(profile.Subscriptions.ContainsKey(subscriptionId));
                subscriptionsList.Add(profile.Subscriptions[subscriptionId]);
            }

            return subscriptionsList;
        }

        public bool HasSubscription(Guid subscriptionId)
        {
            bool exists = false;
            string subscriptions = GetProperty(Property.Subscriptions);

            if (!string.IsNullOrEmpty(subscriptions))
            {
                exists = subscriptions.Contains(subscriptionId.ToString());
            }

            return exists;
        }

        public void SetSubscriptions(List<AzureSubscription> subscriptions)
        {
            string value = string.Join(",", subscriptions.Select(s => s.Id.ToString()));
            Properties[Property.Subscriptions] = value;
        }

        public void RemoveSubscription(Guid id)
        {
            string subscriptions = GetProperty(Property.Subscriptions);

            if (string.IsNullOrEmpty(subscriptions))
            {
                SetProperty(Property.Subscriptions, 
                    string.Join(",", subscriptions.Split(',').Where(s => s != id.ToString())));
            }
        }

        public override bool Equals(object obj)
        {
            var anotherAccount = obj as AzureAccount;
            if (anotherAccount == null)
            {
                return false;
            }
            else
            {
                return anotherAccount.Id == Id;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
