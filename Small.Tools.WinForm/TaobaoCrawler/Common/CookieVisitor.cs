using CefSharp;
using System;

namespace Small.Tools.WinForm.TaobaoCrawler.Common
{
    /// <summary>
    /// CefSharp Cookie
    /// </summary>
    public class CookieVisitor : ICookieVisitor
    {
        public Action<Cookie> SendCookie = null;
        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            SendCookie?.Invoke(cookie);
            return true;
        }

        public void Dispose() { }
    }
}
