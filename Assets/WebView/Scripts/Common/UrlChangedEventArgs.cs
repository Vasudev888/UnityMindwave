/**
* Copyright (c) 2021 Vuplex Inc. All rights reserved.
*
* Licensed under the Vuplex Commercial Software Library License, you may
* not use this file except in compliance with the License. You may obtain
* a copy of the License at
*
*     https://vuplex.com/commercial-library-license
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using UnityEngine;

namespace Vuplex.WebView {

    /// <summary>
    /// Event args for `UrlChanged`.
    /// </summary>
    public class UrlChangedEventArgs : EventArgs {

        public UrlChangedEventArgs(string url, string type) {
            Url = url;
            Type = type;
        }

        /// <summary>
        /// The new webpage URL.
        /// </summary>
        public string Url;

        [Obsolete("UrlChangedEventArgs.Title has been removed. Please use IWebView.Title or IWebView.TitleChanged instead.", true)]
        public string Title;

        /// <summary>
        /// One of the string constants in <see cref="UrlActionType"/>.
        /// </summary>
        public string Type;
    }
}

