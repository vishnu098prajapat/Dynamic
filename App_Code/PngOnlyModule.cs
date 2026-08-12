using System;
using System.IO;
using System.Web;

public class PngOnlyModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        context.BeginRequest += new EventHandler(OnBeginRequest);
    }

    private void OnBeginRequest(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;
        HttpRequest request = app.Context.Request;

        if (request.HttpMethod == "POST" && request.Files.Count > 0)
        {
            foreach (string key in request.Files)
            {
                HttpPostedFile file = request.Files[key];
                if (file != null && file.ContentLength > 0)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    string contentType = file.ContentType.ToLower();

                    if (!IsValidImage(file))
                    {
                        app.Context.Response.Write("Only Allow files are allowed.");
                        app.Context.Response.End();
                    }
                }
            }
        }
    }

    private bool IsValidImage(HttpPostedFile file)
    {
        try
        {
            // Check file size and extensions first
            string[] validExtensions = { ".jpg", ".jpeg", ".png" };
            string fileExt = Path.GetExtension(file.FileName).ToLower();


            if (Array.IndexOf(validExtensions, fileExt) < 0)
            {
                return false;
            }

            // Read magic bytes (first few bytes of the stream)
            byte[] buffer = new byte[8];
            file.InputStream.Read(buffer, 0, buffer.Length);
            file.InputStream.Position = 0; // reset stream position for saving later

            // Check magic headers
            if (buffer[0] == 0xFF && buffer[1] == 0xD8) // JPEG
                return true;
            if (buffer[0] == 0x89 && buffer[1] == 0x50) // PNG
                return true;
            //if (buffer[0] == 0x47 && buffer[1] == 0x49) // GIF
            //    return true;
            //if (buffer[0] == 0x42 && buffer[1] == 0x4D) // BMP
            //    return true;
            //if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46) // WebP starts with RIFF
            //    return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose() { }
}
