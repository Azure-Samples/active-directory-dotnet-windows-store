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
        const string tenant = "skwantoso.com";
        const string clientId = "d4fe66e1-a87a-4b3a-92dd-1c1480ee3455";

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        const string todoListResourceId = "https://skwantoso.com/TodoListService";
        const string todoListBaseAddress = "https://localhost:44321";

        private HttpClient httpClient = new HttpClient();
        private AuthenticationContext authContext = null;

        public MainPage()
        {
            this.InitializeComponent();

            authContext = new AuthenticationContext(authority);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
