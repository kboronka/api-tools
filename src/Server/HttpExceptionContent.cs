﻿using System;
using System.Text;

using HttpPack.Utils;

namespace HttpPack
{
    public class HttpExceptionContent : HttpContent
    {
        public HttpExceptionContent(Exception ex) : base()
        {
            var inner = ExceptionHelper.GetInner(ex);
            var json = new JsonKeyValuePairs
            {
                { "message", inner.Message },
                { "stackTrace", ExceptionHelper.GetStackTrace(inner) }
            };

            var body = json.Stringify();

            this.content = Encoding.UTF8.GetBytes(body);
            this.ContentType = "application/json";
        }
    }
}
