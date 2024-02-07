using Renci.SshNet;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;



class Program
{
    static void Main()
    {
        string sftphost = "sftp-dev.fi.sscgwp.com";
            string sftpusername = "SSC110DEV";
            string sftppassword = "L0v3lyD4y";

        string localfilepath = "/IHubTest/IhubTest.txt";
        string remotefilepath = localfilepath;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        int chunksize = 50 * 1024 * 1024;

        string connectionString = "DefaultEndpointsProtocol=https;AccountName=pmgreportingstgacct;AccountKey=c0ayTaz/QZZltrN18+JqTfzP9AX7mVw0aI9fDIIDx6tqrANDM/a5tLnqRmJvT+8gL4BUHz4YT6db+AStWSL6CQ==;EndpointSuffix=core.windows.net";
        
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, "apipoc");
        BlobClient blobClient = containerClient.GetBlobClient(localfilepath);

        using (var client = new SftpClient(sftphost, sftpusername, sftppassword))
        {
            client.Connect();
            using (var remotestream = client.OpenRead(remotefilepath))
            {
                byte[] buffer = new byte[chunksize];
                int bytesread;

                using (Stream chunkstream = blobClient.OpenWrite(true))
                {
                    while ((bytesread = remotestream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        chunkstream.Write(buffer, 0, bytesread);
                    }
                }
            }
            client.Disconnect();
        }

        stopwatch.Stop();

        long filesizeinbytes = new FileInfo(localfilepath).Length;
        double transferspeedmbps = filesizeinbytes / (stopwatch.Elapsed.TotalSeconds * 1000000);

        Console.WriteLine($"File upload successful");
        Console.WriteLine($"Transfer speed: {transferspeedmbps:F2} Mbps");
    }
}