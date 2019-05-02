﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpPack.Json;
using HttpPack.Net;

namespace example
{
    public static class TestTLS
    {
        public static bool TLS11()
        {
            var uri = "https://www.howsmyssl.com/a/check";

            var client = new RestClient();
            var res = client.Fetch(uri);

            if (res.Code == 200)
            {
                var kvp = new JsonKeyValuePairs(res.Body);

                Console.WriteLine("TLS Test -- " + uri);
                Console.WriteLine("  TLS version: " + kvp["tls_version"]);
                Console.WriteLine("  rating: " + kvp["rating"]);

                return true;
            }

            return false;
        }
    }
}