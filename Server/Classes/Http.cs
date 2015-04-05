using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Server.Classes
{
    public static class Http
    {
        public static HttpResponseMessage GetResponse<T>(this HttpRequestMessage request, T value, string type)
        {
            AddAcceptHeader(request, type);
            if (value == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
            return request.CreateResponse(System.Net.HttpStatusCode.OK, value);
        }

        private static void AddAcceptHeader(HttpRequestMessage request, string type)
        {
            if (type == "json")
            {
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            else if (type == "xml")
            {
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            }
        }
    }
}