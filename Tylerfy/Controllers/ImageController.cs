﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ImageResizer;
using ImageResizer.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Tylerfy.Controllers
{
    public class ImageController : Controller
    {
        private string imageXmlPath
        {
            get
            {
                return Server.MapPath("~/App_Data/images.xml");
            }
        }

        private XmlDocument _xImages = null;
        private XmlDocument xImages
        {
            get
            {
                if (_xImages == null) 
                {
                    _xImages = new XmlDocument();
                    _xImages.Load(imageXmlPath); 
                }

                return _xImages;
            }
        }

        private XmlNode NextImage
        {
            get
            {
                XmlNodeList imageNodes = xImages.SelectNodes("//image");
                Random rand = new Random();
                int index = rand.Next(imageNodes.Count);

                return imageNodes.Item(index);
            }
        }

        //
        // GET: /Image/
        public FileStreamResult Index(int width, int height)
        {
            return GetImage(width, height, null);
        }

        public FileStreamResult SimpleFilters(int width, int height, int alpha, int grayscale)
        {
            return GetImage(width, height, string.Format("s.alpha={0}&s.grayscale={1}", alpha, grayscale));
        }


        public FileStreamResult GetImage(int width, int height, string extraOpts)
        {
            XmlNode imageNode = NextImage;

            string imgPath = Server.MapPath(string.Format("~/App_Data/{0}", imageNode.Attributes.GetNamedItem("path").Value));
            string imgAnchor = imageNode.Attributes.GetNamedItem("anchor").Value;

            FileStream inStream = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
            MemoryStream memStream = new MemoryStream();

            Instructions ins = new Instructions(string.Format("w={0}&h={1}&mode=crop&anchor={2}&{3}", width, height, imgAnchor, extraOpts));
            ImageJob job = new ImageJob(inStream, memStream, ins);
            ImageBuilder.Current.Build(job);

            memStream.Flush();
            memStream.Position = 0;

            return new FileStreamResult(memStream, "image/jpg");
        }

    }
}
