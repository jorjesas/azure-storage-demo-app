using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebRole.Models;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using WebRole.Helpers;
using System.IO;
using System.Net;
using System.Web.Helpers;

namespace WebRole.Controllers
{
    public class BlobController : Controller
    {
        private readonly AzureStorageConfig storageConfig = new AzureStorageConfig() {
            AccountName = "allegrov6backups",
            AccountKey = "9ac06d2uDIPOwYr80dLsOb9e5EmQV4ioTPQXIYZf2zVp096W8Frq9ACrHTub8s0asLnTYGG+WruxPrPltGqEAQ==",
            QueueName = "test-queue",
            ImageContainer = "misc",
            ThumbnailContainer = "misc"
        };

        // GET: Blob
        public ActionResult Index()
        {
            return View();
        }

        // POST /api/images/upload
        [HttpPost()]
        public async Task<ActionResult> Upload(ICollection<HttpPostedFileBase> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.ContentLength > 0)
                        {
                            using (Stream stream = formFile.InputStream)
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)

                        return RedirectToAction("GetThumbNails", "Images", null);
                    else

                        return new HttpStatusCodeResult(HttpStatusCode.OK); 
                }
                else
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Look like the image couldnt upload to the storage");


            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // GET /api/blob/thumbnails
        [HttpGet()]
        public async Task<ActionResult> Thumbnails()
        {
            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Please provide a name for your image container in the azure blob storage");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);

                return Json(thumbnailUrls, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }

        }
    }
}