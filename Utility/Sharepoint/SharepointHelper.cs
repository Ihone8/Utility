using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Sharepoint {
    public class SharepointHelper {

        private static string sharePointSite = ""; // sharepoint web site url
        private static string UserName = "";    //login username
        private static string PassWord = "";    // login password
        private static string Domain = "";      // login domain
        private static string documentLibraryName =""; //sharepoint doucment library name
        public static void UploadFile(string filePath, string fileName) {
            using (ClientContext clientContext = new ClientContext(sharePointSite)) {
                clientContext.Credentials = new System.Net.NetworkCredential(UserName, PassWord, Domain);
                var list  = clientContext.Web.Lists.GetByTitle(documentLibraryName);
                var folder = list.RootFolder;
                clientContext.Load(folder);
                clientContext.ExecuteQuery();

                FileStream fs = new FileStream(filePath, FileMode.Open);
                Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext,
                folder.ServerRelativeUrl + "/" + fileName,
                fs,
                true);
                fs.Close();
            }
        }

        public static void Download(string sharepointfilepath) {
            using (ClientContext clientContext = new ClientContext(sharePointSite)) {
                clientContext.Credentials = new System.Net.NetworkCredential(UserName, PassWord, Domain);
                var list  = clientContext.Web.Lists.GetByTitle(documentLibraryName);
                var folder = list.RootFolder;
                clientContext.Load(folder);
                clientContext.ExecuteQuery();
                //sharepointfilepath 路径是保存在 sharepoint上的相对路径。
                var  file =  Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, sharepointfilepath);
                FileStream fileStream = new FileStream("",FileMode.CreateNew);
                file.Stream.CopyTo(fileStream);
            }

        }

    }
}
