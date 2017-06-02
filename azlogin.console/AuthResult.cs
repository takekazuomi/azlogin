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
namespace azlogin.console
{
    public class AuthResult
    {
        public string TokenType { get; set; }
        public string Scope { get; set; }
        public string ExpiresIn { get; set; }
        public string ExtExpiresIn { get; set; }
        public string ExpiresOn { get; set; }
        public string NotBefore { get; set; }
        public string Resource { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }
}