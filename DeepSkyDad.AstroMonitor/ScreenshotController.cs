using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Forms;

namespace ScreenshotApi
{
    public class ScreenshotController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Take()
        {
            //Will contain screenshot
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            Graphics screenshotGraphics = Graphics.FromImage(screenshot);
            //Make the screenshot
            screenshotGraphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            MemoryStream ms = new MemoryStream();
            screenshot.Save(ms, ImageFormat.Png);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(ms.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return result;
        }

        [HttpGet]
        public HttpResponseMessage Ping()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }
    }
}
