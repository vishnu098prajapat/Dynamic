<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="Microsoft.AspNet.FriendlyUrls" %>

<script RunAt="server">


    cls_connection ObjAll = new cls_connection();
    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        // Inject anti-inspect JavaScript into every response
    string script = @"<script>
    document.addEventListener('contextmenu', function(event) {
        event.preventDefault();
    });
    document.addEventListener('keydown', function(event) {
        if (event.key === 'F12' || 
            (event.ctrlKey && event.shiftKey && (event.key === 'I' || event.key === 'J' || event.key === 'C')) || 
            (event.ctrlKey && event.key === 'U')) {
            event.preventDefault();
        }
    });
<\/script>";

        // Apply the response filter to inject the script
        //if (Request.Url.AbsolutePath.EndsWith(".aspx"))
        {
          //  Response.Filter = new ResponseFilter(Response.Filter, script);
        }
    }





    void Application_Start(object sender, EventArgs e)
    {

        RouteTable.Routes.EnableFriendlyUrls();

        // Enable Friendly URLs
        //var settings = new FriendlyUrlSettings();
        //settings.AutoRedirectMode = RedirectMode.Permanent; // Optional: Redirect old URLs to the new format
        //RouteTable.Routes.EnableFriendlyUrls(settings);

    }

    void Application_End(object sender, EventArgs e)
    {
        //  Code that runs on application shutdown

    }

    void Application_Error(object sender, EventArgs e)
    {
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e)
    {
        // Code that runs when a new session is started

    }
    //protected void Application_BeginRequest(Object sender, EventArgs e)
    //{
    //    if (Request.Cookies.Count > 0 && Request.Cookies["ASP.NET_SessionId"] != null)
    //    {
    //        HttpCookie sessionCookie = Request.Cookies["ASP.NET_SessionId"];
    //        sessionCookie.Path = "/; SameSite=None";
    //        sessionCookie.Secure = true;
    //        Response.SetCookie(sessionCookie);
    //    }

    //    //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "https://bype.info");
    //    //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
    //    //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    //}

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {
    }
    void Session_End(object sender, EventArgs e)
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }

    // Helper class to inject the script
    public class ResponseFilter : System.IO.Stream
    {
        private readonly System.IO.Stream _responseStream;
        private readonly string _script;

        public ResponseFilter(System.IO.Stream responseStream, string script)
        {
            _responseStream = responseStream;
            _script = script;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string responseHtml = System.Text.Encoding.UTF8.GetString(buffer, offset, count);
            if (responseHtml.Contains("</body>"))
            {
                // Inject the script before the closing </body> tag
                responseHtml = responseHtml.Replace("</body>", _script.Replace("\\","") + "</body>");
                buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
                offset = 0;
                count = buffer.Length;
            }
            _responseStream.Write(buffer, offset, count);
        }

        // Required abstract members of Stream
        public override bool CanRead => _responseStream.CanRead;
        public override bool CanSeek => _responseStream.CanSeek;
        public override bool CanWrite => _responseStream.CanWrite;
        public override long Length => _responseStream.Length;

        public override long Position
        {
            get { return _responseStream.Position; }
            set { _responseStream.Position = value; }
        }

        public override void Flush() => _responseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) =>
            _responseStream.Read(buffer, offset, count);

        public override long Seek(long offset, System.IO.SeekOrigin origin) =>
            _responseStream.Seek(offset, origin);

        public override void SetLength(long value) => _responseStream.SetLength(value);
    }


</script>


