using FacialDetection.DataObjects.Entities.VmFace;
using FacialDetection.Services.Services.UIHelper;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FacialDetection.Controllers
{
    public class FaceDetectionController : Controller
    {
        private static string ServiceKey = ConfigurationManager.AppSettings["FaceServiceKey"];
        private static string directory;
        private static string ImageName = "../UploadedFiles";
        private ObservableCollection<vmFace> _detectedFaces = new ObservableCollection<vmFace>();
        private ObservableCollection<vmFace> _resultCollection = new ObservableCollection<vmFace>();

        public ObservableCollection<vmFace> DetectedFaces
        {
            get
            {
                return _detectedFaces;
            }
        }

        public ObservableCollection<vmFace> ResultCollection
        {
            get
            {
                return _resultCollection;
            }
        }

        public int MaxImageSize
        {
            get
            {
                return 500;
            }
        }


        //
        // GET: /FaceDetection
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SaveCandidateFiles()
        {
            string message = string.Empty, fileName = string.Empty, actualfileName = string.Empty;
            bool flag = false;

            //Requested File Collection
            HttpFileCollection fileRequested = System.Web.HttpContext.Current.Request.Files;

            if (fileRequested != null)
            {
                //Created New Folder
                CreateDirectory();

                //Clear Existing File in Folder
                ClearDirectory();

                for (int i = 0; i < fileRequested.Count; i++)
                {
                    var file = Request.Files[i];
                    actualfileName = file.FileName;
                    fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    int size = file.ContentLength;

                    try
                    {
                        file.SaveAs(Path.Combine(Server.MapPath(directory), fileName));
                        message = "File uploaded successfully!";
                        ImageName = fileName;
                        flag = true;
                    }
                    catch (Exception)
                    {

                        message = "File upload failed! Try again.";
                    }
                }
            }
            return new JsonResult
            {
                Data = new
                {
                    Message = message,
                    ImageName = fileName,
                    Status = flag
                }
            };
        }

        [HttpGet]
        public async Task<dynamic> GetDetectedFaces()
        {
            ResultCollection.Clear();
            DetectedFaces.Clear();

            var DetectedResultsInText = string.Format("Detecting.......");
            var FullImgPath = Server.MapPath(directory) + '/' + ImageName as string;
            var QueryFaceImageUrl = directory + '/' + ImageName;

            if (ImageName != "")
            {
                CreateDirectory();

                try
                {
                    //Call the detection REST API
                    using (var fStream = System.IO.File.OpenRead(FullImgPath)) 
                    { 
                        //User selected an image
                        var imageInfo = UIHelper.GetImageInfoForRendering(FullImgPath);
 
                        //Create instance of Service Client, passing API Key as parameter
                        var faceServiceClient = new FaceServiceClient(ServiceKey);
                        //Specify what facial attributes to handle
                        Face[] faces = await faceServiceClient.DetectAsync(fStream, true, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Makeup, FaceAttributeType.Smile, FaceAttributeType.FacialHair });

                        //Detection result message
                        DetectedResultsInText = string.Format("{0} face(s) has/have been detected!", faces.Length);
                        Bitmap CroppedFace = null;
                        
                        foreach(var face in faces)
                        {
                            //Create and Save Cropped Images
                            var croppedImage = Convert.ToString(Guid.NewGuid()) + ".jpeg" as string;
                            var croppedImagePath = directory + '/' + croppedImage as string;
                            var croppedImageFullPath = Server.MapPath(directory) + '/' + croppedImage as string;

                            CroppedFace = CropBitmap((Bitmap)Image.FromFile(FullImgPath), face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height);
                            CroppedFace.Save(croppedImageFullPath, ImageFormat.Jpeg);
                            if (CroppedFace != null)
                                ((IDisposable)CroppedFace).Dispose();

                            DetectedFaces.Add(new vmFace()
                            {
                                ImagePath = FullImgPath,
                                FileName = croppedImage,
                                FilePath = croppedImagePath,
                                Left = face.FaceRectangle.Left,
                                Top = face.FaceRectangle.Top,
                                Width = face.FaceRectangle.Width,
                                Height = face.FaceRectangle.Height,
                                FaceId = face.FaceId.ToString(),
                                Gender = face.FaceAttributes.Gender,
                                Age = string.Format("{0:#} years old", face.FaceAttributes.Age),
                                Emotion = face.FaceAttributes.Emotion.ToString(),
                                Glasses = face.FaceAttributes.Glasses.ToString(),
                                Makeup = face.FaceAttributes.Makeup.ToString(),
                                FacialHair = face.FaceAttributes.FacialHair.ToString(),
                                IsSmiling = face.FaceAttributes.Smile > 0.0 ? "Smile" : "Not Smile"
                            });
                        } 

                        //Convert detection results into UI Binding objects
                        var rectFaces = UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo);
                        foreach(var face in rectFaces)
                        {
                            ResultCollection.Add(face);
                        }
                    }
                }
                catch (FaceAPIException ex)
                {
                    ex.ToString();
                }
            }
            return new JsonResult
            {
                Data = new
                {
                    QueryFaceImage = QueryFaceImageUrl,
                    MaxImageSize = MaxImageSize,
                    FaceInfo = DetectedFaces,
                    FaceRectangles = ResultCollection,
                    DetectedResults = DetectedResultsInText
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
            
        public Bitmap CropBitmap(Bitmap bitmap, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = bitmap.Clone(rect, bitmap.PixelFormat);
            return cropped;
        }

        //Method to create 'UploadedFiles folder' if it doesn't exist
        public void CreateDirectory()
        {
            bool exists = System.IO.Directory.Exists(Server.MapPath(directory));
            if (!exists)
            {
                try
                {
                    Directory.CreateDirectory(Server.MapPath(directory));
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        //Method to delete uploaded images from the system after every detection test
        public void ClearDirectory()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Server.MapPath(directory)));
            var files = dir.GetFiles();
            if(files.Length > 0)
            {
                try
                {
                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        fi.Delete();
                    }
                }
                catch (Exception ex)
                {
                    
                    ex.ToString();
                }
            }
        }
    }
}