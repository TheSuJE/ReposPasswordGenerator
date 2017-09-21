﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Download;

namespace PasswordGenerator
{
    class GoogleDrive
    {
        private string[] Scopes = { DriveService.Scope.Drive };
        private string ApplicationName = "PasswordGenerator";
        List<Google.Apis.Drive.v3.Data.File> files = new List<Google.Apis.Drive.v3.Data.File>();
        List<Google.Apis.Drive.v3.Data.File> folders = new List<Google.Apis.Drive.v3.Data.File>();
        private UserCredential credential;
        private DriveService service;
        string profile = "";
        string folderId = "";

        public GoogleDrive(string profile)
        {
            this.profile = profile;
            Console.Write("Loading credentials of profile: {0}...", profile);
            credential = GetUserCredential();
            Console.WriteLine("Credentials loaded. Loading DriveService...");
            service = GetDriveService();
            Console.WriteLine("DriveService loaded.");
            Update();
        }
        public void Update()
        {
            Console.WriteLine("Loading list of items...");
            IList<Google.Apis.Drive.v3.Data.File> items = service.Files.List().Execute().Files;
            Console.WriteLine("List of items loaded. Sorting...");
            foreach (var file in items)
            {
                if (file.MimeType == "application/vnd.google-apps.folder") folders.Add(file);
                else if (file.MimeType == "application/vnd.google-apps.file") files.Add(file);
            }
            Console.WriteLine("Sorted.");
        }
        public void Sync(string workpath)
        {
            bool contains = false;
            foreach (var file in folders) { if (file.Name == "Passwords") { contains = true; folderId = file.Id; } break; }
            if (!contains)
            {
                Console.WriteLine("Creating folder \"Passwords\"...");
                if (!string.IsNullOrEmpty(CreateFolder("Passwords"))) Console.WriteLine("Folder created.");
                else Console.WriteLine("Error. Folder not created!");
            }
        }
        private string CreateFolder(string folderName)
        {
            var file = new Google.Apis.Drive.v3.Data.File();
            file.Name = "Passwords";
            file.MimeType = "application/vnd.google-apps.folder";

            var request = service.Files.Create(file);
            request.Fields = "id";

            var result = request.Execute();
            folderId = result.Id;
            return result.Id;
        }
        private UserCredential GetUserCredential()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string creadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                creadPath = Path.Combine(creadPath, "driveCredentials", profile, "drive-credentials.json");
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(creadPath, true) ).Result;
            }
        }
        private DriveService GetDriveService()
        {
            return new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });
        }

        private string UploadFileToDrive(string path, string contentType)
        {
            var filedata = new Google.Apis.Drive.v3.Data.File();
            filedata.Name = Path.GetFileName(path);
            filedata.Parents = new List<string> { folderId };

            FilesResource.CreateMediaUpload request;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                request = service.Files.Create(filedata, stream, contentType);
                request.Upload();
            }
            var file = request.ResponseBody;

            return file.Id;
        }
        private void DownloadFileFromDrive(string fileid, string path)
        {
            var request = service.Files.Get(fileid);

            using (var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Completed:
                            Console.WriteLine("Download complete");
                            break;
                        case DownloadStatus.Failed:
                            Console.WriteLine("Download failed");
                            break;
                    }
                };
                request.Download(memoryStream);

                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                }
            }

        }

        public void Download(string workpath)
        {
            Console.WriteLine("Download started!");
            Update();
            for ( int i = 0; i < files.Count; i ++)
            {
                Google.Apis.Drive.v3.Data.File file = files[i];
                if (file.Parents == new List<string> { folderId })
                {
                    DownloadFileFromDrive(file.Id, workpath + "\\" + file.Name);
                }
            }
            Console.WriteLine("Download complete!");
            }
        public void Upload(string workpath)
        {
            Console.WriteLine("Upload started!");
            Update();
            string[] filess = Directory.GetFiles(workpath);
            for (int i = 0; i < filess.Length; i++)
            {
                //Google.Apis.Drive.v3.Data.File file = files[i];
                //if (file.Parents == new List<string> { folderId })
                //{
                //    DownloadFileFromDrive(file.Id, workpath + "\\" + file.Name);
                //}
                foreach (var file in files)
                {
                    if (Path.GetFileName(filess[i]).Equals(file.Name)) { service.Files.Delete(file.Id); }
                }
            }
            foreach (string file in filess)
            {
                UploadFileToDrive(file, );
            }
            Console.WriteLine("Uploading complete!");
        }

    }
}
