﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpPack;

namespace example
{
    public static class TestTLS
    {
        public static bool TLS11()
        {
            var uri = "https://www.howsmyssl.com/a/check";

            var client = new HttpClient<JsonKeyValuePairs>();
            var res = client.Get(uri, "");

            if (res.Code == 200)
            {
                Console.WriteLine("TLS Test -- " + uri);
                Console.WriteLine("  TLS version: " + res.Body["tls_version"]);
                Console.WriteLine("  rating: " + res.Body["rating"]);
                Console.WriteLine();

                return true;
            }

            return false;
        }
    }
}
