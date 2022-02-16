using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utf8Json;
namespace ThaumaStudio.Util
{
    public static class Util
    {
        // com.mojang path on desktop
        public static String com_mojang_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Packages\\Microsoft.MinecraftUWP_8wekyb3d8bbwe\\LocalState\\games\\com.mojang\\";
        public static ProjectInfo readProjectInfo(String path)
        {
            /* Reads project name, desc, uuid, and dependencies */
            /// Creates a default pack info
            ProjectInfo projectInfo = new ProjectInfo();

            /// Try parsing the pack manifest.json
            try
            {
                /// Read the file and deserialize as json dynamically
                String content = File.ReadAllText(path + "\\manifest.json");
                var json = JsonSerializer.Deserialize<dynamic>(content);
                /// Read name, desc, uuid
                projectInfo.name = json["header"]["name"];
                projectInfo.description = json["header"]["description"];
                projectInfo.uuid = json["header"]["uuid"];

                /// Read project version
                projectInfo.version = String.Format("v{0}.{1}.{2}", json["header"]["version"][0], json["header"]["version"][1], json["header"]["version"][2]);
                
                /// Add dependencies' uuid to the array
                if(json.ContainsKey("dependencies"))
                foreach (var pack in json["dependencies"])
                {
                    projectInfo.dependencies.Add(pack["uuid"] as String);
                }

                if (json.ContainsKey("modules"))
                    projectInfo.type = json["modules"][0]["type"]; // "data" or "resources"
                
            }
            catch (Exception e) {
                // Do nothing, just return a broken pack data(default)
                //MessageBox.Show("Error reading pack data. " + e.Message + e.StackTrace);
                projectInfo.name = Path.GetFileName(path);
                projectInfo.description = "Error reading pack info";
            }

            return projectInfo;
        }

        public static String ProjectInfoToJSON(ProjectInfo project)
        {
            String result = "{}";

            try
            {
                // Checks if there's dependencies or not, if does, add it
                String dependencies =  project.dependencies.Count > 0 ?
                String.Format(@",
    ""dependencies"": [
        {{
            ""uuid"": ""{0}"",
            ""version"": [1, 0, 0]
        }}
    ]", project.dependencies[0])
                : "";

                result = String.Format(@"{{
    ""format_version"": 1,
    ""made_with.CoreCoder"" : ""2"",
    ""header"": {{
        ""name"": ""{0}"",
        ""description"": ""{1}"",
        ""uuid"": ""{2}"",
        ""version"": [1, 0, 0],
        ""min_engine_version"": [1, 8, 0]
    }},
    ""modules"": [
        {{
            ""description"": ""{1}"",
            ""type"": ""{3}"",
            ""uuid"": ""{4}"",
            ""version"": [1, 0, 0]
        }}
    ]{5}
}}", 
            project.name, project.description, project.uuid, project.type, Guid.NewGuid().ToString(), dependencies);
            }
            catch (Exception e) { MessageBox.Show(e.Message + "\n" + e.StackTrace, e.ToString()); }

            return result;
        }


        public static void SavePackImage(String path)
        {
            /// <summary>
            /// Saves default pack icon to the folder
            /// </summary>
            /// taken from https://www.codeproject.com/Questions/849001/How-to-decode-Base-String-to-image-in-WPF
            byte[] oByteArray = Convert.FromBase64String(@"");
            MemoryStream oMemoryStream = new MemoryStream(oByteArray, 0,oByteArray.Length);
            Image oImage = Image.FromStream(oMemoryStream, true);

            oImage.Save(path + "/pack_icon.png", ImageFormat.Png);
            oMemoryStream.Close();
        }
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }


    public class ProjectInfo {
        public String name = "Unknown Pack";
        public String description = "-";
        public String uuid = "";
        public List<String> dependencies = new List<string>();
        public String version = "";
        public String path = "";
        public String type = "data"; // "data" or "resources"

        public String ToReadableString() {
            return String.Format("{0}\n{1}\n{2}{3}", name, description, version, dependencies.Count > 0? String.Format("\n({0}) Dependencies", dependencies.Count) : "");
        }
    }
}
