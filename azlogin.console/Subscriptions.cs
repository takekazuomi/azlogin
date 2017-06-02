/* 
 * Copyright 2015-2017 Takekazu Omi
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Newtonsoft.Json;

namespace azlogin.console
{
    public class Subscriptions
    {
        [JsonProperty(PropertyName = "value")]
        public Subscription[] Value { get; set; } = { };
    }
    public class Subscription
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public string DisplayName { get; set; }
        public string State { get; set; }
        public SubscriptionPolicies SubscriptionPolicies { get; set; }
        public string AuthorizationSource { get; set; }
    }

    public class SubscriptionPolicies
    {
        public string LocationPlacementId { get; set; }
        public string QuotaId { get; set; }
        public string SpendingLimit { get; set; }
    }
}