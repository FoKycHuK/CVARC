﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace ProxyCommon
{
    public static class WebHelper
    {
        private static ILog log = LogManager.GetLogger(nameof(WebHelper));

        public static async Task<T> ReadFromUrlAsync<T>(string url)
        {
            var request = WebRequest.CreateHttp(url);

            try
            {
                var responseStream = (await request.GetResponseAsync()).GetResponseStream();
                if (responseStream == null)
                {
                    log.Error("Response stream is null");
                    return default(T);
                }
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
            catch (Exception e)
            {
                log.Error("Cant read json from web answer", e);
                return default(T);
            }
        }

        public static async Task DownloadFile(string url, string path)
        {
            using (var clinet = new WebClient())
                await clinet.DownloadFileTaskAsync(url, path);
        }
    }
}