using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service.FileSystem {
    public class FileSystemServiceDummy : FileSystemServiceImpl {

        readonly string streamingAssetsFingerprint = "StreamingAssets";

        public override bool FileExistsInDomain(FSDomain domain, string relativePath) {
            if(domain == FSDomain.Localizations || domain == FSDomain.SteamingAssets || domain == FSDomain.RuntimeAssets) {
                return base.FileExistsInDomain(domain, relativePath);
            } else {
                return false;
            }
        }

        public override List<string> GetFilesInDomain(FSDomain domain, string innerDomainPath = "", string filter = "*.*", bool recursive = false) {
            if(domain == FSDomain.Localizations || domain == FSDomain.SteamingAssets || domain == FSDomain.RuntimeAssets) {
                return base.GetFilesInDomain(domain, innerDomainPath, filter, recursive);
            } else {
                return new List<string>();
            }
        }

        public override string GetPath(FSDomain domain, string relativePart = "") {
            if(domain == FSDomain.Localizations || domain == FSDomain.SteamingAssets || domain == FSDomain.RuntimeAssets) {
                return base.GetPath(domain, relativePart);
            } else {
                return "";
            }
        }

        public override byte[] LoadFileAsBytesAtDomain(FSDomain domain, string relativePathToFile, bool compressed = false, int estimatedUncompressedSize = 0) {
            if(domain == FSDomain.Localizations || domain == FSDomain.SteamingAssets || domain == FSDomain.RuntimeAssets) {
                return base.LoadFileAsBytesAtDomain(domain, relativePathToFile, compressed, estimatedUncompressedSize);
            } else {
                return new byte[0];
            }
        }

        public override string LoadFileAsStringAtDomain(FSDomain domain, string relativePathToFile) {
            if(domain == FSDomain.Localizations || domain == FSDomain.SteamingAssets || domain == FSDomain.RuntimeAssets) {
                return base.LoadFileAsStringAtDomain(domain, relativePathToFile);
            } else {
                return "";
            }
        }

        public override bool FileExists(string pathToFile) {
            if (pathToFile.Contains(streamingAssetsFingerprint)) {
                return base.FileExists(pathToFile);
            } else {
                return false;
            }
        }

        public override List<string> GetFilesInAbsFolder(string absPath, string pattern = "*.*", bool recursive = false) {
            if (absPath.Contains(streamingAssetsFingerprint)) {
                return base.GetFilesInAbsFolder(absPath, pattern, recursive);
            } else {
                return new List<string>();
            }
        }

        public override byte[] LoadFileAsBytes(string pathToFile, bool compressed = false, int estimatedUncompressedSize = 0) {
            if (pathToFile.Contains(streamingAssetsFingerprint)) {
                return base.LoadFileAsBytes(pathToFile, compressed, estimatedUncompressedSize);
            } else {
                return new byte[0];
            }
        }

        public override string LoadFileAsString(string pathToFile, bool compressed = false) {
            if (pathToFile.Contains(streamingAssetsFingerprint)) {
                return base.LoadFileAsString(pathToFile, compressed);
            } else {
                return "";
            }
        }



        public override bool WriteBytesToFile(string pathToFile, byte[] bytes, bool compress = false) {
            return true;
        }

        public override bool WriteBytesToFileAtDomain(FSDomain domain, string relativePathToFile, byte[] bytes, bool compress = false) {
            return true;
        }

        public override bool WriteStringToFile(string pathToFile, string data, bool append = false) {
            return true;
        }

        public override bool WriteStringToFileAtDomain(FSDomain domain, string relativePathToFile, string data, bool append = false) {
            return true;
        }

        public override void RemoveFile(string filePath) {
        }

        public override void RemoveFileInDomain(FSDomain domain, string relativePath) {
        }
    }
}