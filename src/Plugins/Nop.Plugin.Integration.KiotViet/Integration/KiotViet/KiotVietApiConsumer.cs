using Newtonsoft.Json;
using Nop.Core.Extensions;
using Nop.Plugin.Integration.KiotViet.Integration.KiotViet.Entities;
using RestSharp;
using System.Collections.Generic;
using System.Net;

namespace Nop.Plugin.Integration.KiotViet.Integration.KiotViet
{
    public class KiotVietApiConsumer
    {
        protected RestClient Client;
        private string _token;
        public KiotVietApiConsumer()
        {
            Client = new RestClient(KiotVietConstant.UrlApiRootPublic);
            _token = GetToken();
        }

        private ApiResponse<T> HandleResponse<T>(IRestResponse response) where T : new()
        {
            var apiResponse = new ApiResponse<T>()
            {
                Status = response.StatusCode
            };
            if (response.StatusCode.IsIn(HttpStatusCode.OK, HttpStatusCode.Created))
            {
                apiResponse.Resource = JsonConvert.DeserializeObject<T>(response.Content);
            }
            else
            {
                apiResponse.Error = new ApiError(response.Content);
            }

            return apiResponse;
        }

        public string GetToken()
        {
            var tokenClient = new RestClient(KiotVietConstant.UrlApiRootToken);
            var request = new RestRequest(KiotVietConstant.UrlApiGetToken, Method.POST);
            request.AddParameter("client_id", "8a80ab27-b951-4ad1-879b-c898065f0c1d");
            request.AddParameter("client_secret", "F3462C9643299B121CD4486DCAC620CDDCC03122");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scopes", "PublicApi.Access");

            var response = tokenClient.Execute(request);
            var apiResponse = HandleResponse<GetTokenResponse>(response);

            return apiResponse.Resource?.access_token ?? string.Empty;
        }

        public List<KVCategory> GetCategories()
        {
            if (string.IsNullOrEmpty(_token))
            {
                return null;
            }
            var request = new RestRequest(KiotVietConstant.UrlApiGetCategory, Method.GET);
            request.AddHeader("Retailer", "bisonkiotvietcom");
            request.AddHeader("Authorization", $"Bearer {_token}");
            var response = Client.Execute(request);
            var apiResponse = HandleResponse<GetCategoryResponse>(response);
            if (apiResponse.Status == HttpStatusCode.Unauthorized)
            {
                _token = GetToken();
            }
            return apiResponse.Resource.data;
        }

        public List<KVProduct> GetProducts()
        {
            var itemIndex = 0;
            var pageSize = 100;
            var @continue = true;
            if (string.IsNullOrEmpty(_token)) return null;

            var sourceProduct = new List<KVProduct>();
            while (@continue)
            {
                var request = new RestRequest(string.Format(KiotVietConstant.UrlApiGetProduct, itemIndex, pageSize), Method.GET);
                request.AddHeader("Retailer", "bisonkiotvietcom");
                request.AddHeader("Authorization", $"Bearer {_token}");

                var response = Client.Execute(request);
                var apiResponse = HandleResponse<GetProductResponse>(response);

                if (apiResponse.Status == HttpStatusCode.Unauthorized)
                {
                    _token = GetToken();
                }
                else
                {
                    var products = apiResponse.Resource.data;
                    foreach (var product in products) product.sku = KiotVietHelper.GetSku(product.code);

                    sourceProduct.AddRange(products);
                    if (products.Count < pageSize)
                    {
                        @continue = false;
                    }
                    else
                    {
                        itemIndex = sourceProduct.Count - 1;
                    }
                }
            }
            return sourceProduct;
        }

        public List<KVProduct> GetProducts(int categoryId)
        {
            if (string.IsNullOrEmpty(_token)) return null;

            var request = new RestRequest(string.Format(KiotVietConstant.UrlApiGetProduct, categoryId), Method.GET);
            request.AddHeader("Retailer", "bisonkiotvietcom");
            request.AddHeader("Authorization", $"Bearer {_token}");

            var response = Client.Execute(request);

            var apiResponse = HandleResponse<GetProductResponse>(response);
            if (apiResponse.Status == HttpStatusCode.Unauthorized)
            {
                _token = GetToken();
            }

            return apiResponse.Resource.data;
        }
    }
}
