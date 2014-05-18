//----------------------------------------------------------------------------------------------
//    Copyright 2014 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The following using statements were added for this sample.
using System.Globalization;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Windows.UI.Popups;
using System.Net.Http.Headers;
using Windows.Data.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TodoListClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        const string aadInstance = "https://login.windows.net/{0}";
        const string tenant = "[Enter tenant name, e.g. contoso.onmicrosoft.com]";
        const string clientId = "[Enter client ID as obtained from Azure Portal, e.g. 82692da5-a86f-44c9-9d53-2f88d52b478b]";

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        const string todoListResourceId = "[Enter App ID URI of TodoListService, e.g. https://contoso.onmicrosoft.com/TodoListService]";
        const string todoListBaseAddress = "https://localhost:44321";

        private HttpClient httpClient = new HttpClient();
        private AuthenticationContext authContext = null;

        public MainPage()
        {
            this.InitializeComponent();

            //
            // Every Windows Store application has a unique URI.
            // Windows ensures that only this application will receive messages sent to this URI.
            // ADAL uses this URI as the application's redirect URI to receive OAuth responses.
            // 
            // To determine this application's redirect URI, which is necessary when registering the app
            //      in AAD, set a breakpoint on the next line, run the app, and copy the string value of the URI.
            //      This is the only purposes of this line of code, it has no functional purpose in the application.
            //
            Uri redirectURI = Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            authContext = new AuthenticationContext(authority);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // When the app starts, fetch the user's To Do list from the service.
            GetTodoList();
        }

        // Retrieve the user's To Do list.
        private async void GetTodoList()
        {
            //
            // Use ADAL to get an access token to call the To Do list service.
            //
            AuthenticationResult result = await authContext.AcquireTokenAsync(todoListResourceId, clientId);

            if (result.Status != AuthenticationStatus.Succeeded)
            {
                if (result.Error == "authentication_canceled")
                {
                    // The user cancelled the sign-in, no need to display a message.
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\nError Description:\n\n{1}", result.Error, result.ErrorDescription), "Sorry, an error occurred while signing you in.");
                    await dialog.ShowAsync();
                }
                return;
            }

            //
            // Add the access token to the Authorization Header of the call to the To Do list service, and call the service.
            //
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await httpClient.GetAsync(todoListBaseAddress + "/api/todolist");

            if (response.IsSuccessStatusCode)
            {
                // Read the response as a Json Array and databind to the GridView to display todo items
                var todoArray = JsonArray.Parse(await response.Content.ReadAsStringAsync());

                TodoList.ItemsSource = from todo in todoArray
                                       select new
                                       {
                                           Title = todo.GetObject()["Title"].GetString()
                                       };
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service.  Please sign-in again.");
                    await dialog.ShowAsync();
                    authContext.TokenCacheStore.Clear();
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Sorry, an error occurred accessing your To Do list.  Please try again.");
                    await dialog.ShowAsync();
                }
            }
        }

        // Post a new item to the To Do list.
        private async void Button_Click_Add_Todo(object sender, RoutedEventArgs e)
        {
            //
            // Use ADAL to get an access token to call the To Do list service.
            //
            AuthenticationResult result = await authContext.AcquireTokenAsync(todoListResourceId, clientId);

            if (result.Status != AuthenticationStatus.Succeeded)
            {
                if (result.Error == "authentication_canceled")
                {
                    // The user cancelled the sign-in, no need to display a message.
                }
                else
                {
                    MessageDialog dialog = new MessageDialog(string.Format("If the error continues, please contact your administrator.\n\nError: {0}\n\n Error Description:\n\n{1}", result.Error, result.ErrorDescription), "Sorry, an error occurred while signing you in.");
                    await dialog.ShowAsync();
                }
                return;
            }

            //
            // Add the access token to the Authorization Header of the call to the To Do list service, and call the service.
            //
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", TodoText.Text) });

            // Call the todolist web api
            var response = await httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content);

            if (response.IsSuccessStatusCode)
            {
                TodoText.Text = "";
                GetTodoList();
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.
                    MessageDialog dialog = new MessageDialog("Sorry, you don't have access to the To Do Service.  Please sign-in again.");
                    await dialog.ShowAsync();
                    authContext.TokenCacheStore.Clear();
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("Sorry, an error occurred accessing your To Do list.  Please try again.");
                    await dialog.ShowAsync();
                }
            }
        }

        //
        // Clear the token cache.
        //
        private void HyperlinkButton_Click_Remove_Account(object sender, RoutedEventArgs e)
        {
            // Clear session state from the token cache.
            authContext.TokenCacheStore.Clear();

            // Reset UI elements
            TodoList.ItemsSource = null;
            TodoText.Text = "";
        }
    }
}
