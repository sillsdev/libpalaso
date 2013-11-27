//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using SIL.Archiving.Generic;
//using SIL.Archiving.IMDI.Schema;

//namespace SIL.Archiving.IMDI
//{
//    /// <summary>Impliments the Session functionality for IMDI session</summary>
//    public class IMDISession : IArchivingSession
//    {
//        // **** Corpus Layout ****
//        //
//        // Test_Corpus (directory)
//        // Test_Corpus.imdi (corpus meta data file)
//        // Test_Corpus\Test_Corpus_Catalog.imdi (catalogue of information)
//        // Test_Corpus\Test_Session (directory)
//        // Test_Corpus\Test_Session.imdi (session meta data file)
//        // Test_Corpus\Test_Session\Contributors (directory - contains files pertaining to contributers/actors)
//        // Test_Corpus\Test_Session\Files*.* (session files)
//        // Test_Corpus\Contributors\Files*.* (contributor/actor files)

//        /// <summary>Add a file for this session</summary>
//        /// <param name="file"></param>
//        public void AddFile(IMDIFile file)
//        {
//            Files.Add(file);
//        }

//        /// <summary>Creates the session directory structure, meta data files, and copies content files</summary>
//        /// <param name="corpusDirectoryName"></param>
//        /// <returns></returns>
//        public bool CreateIMDISession(string corpusDirectoryName)
//        {
//            // normalize session name
//            var sessionDirectoryName = IMDIArchivingDlgViewModel.NormalizeDirectoryName(GetName());

//            // create the session directory
//            var sessionDirInfo = Directory.CreateDirectory(Path.Combine(corpusDirectoryName, sessionDirectoryName));

//            // create the contributor directory
//            if (Actors.Count > 0)
//            {
//                if (Actors.FileCount > 0)
//                {
//                    var contributorDirInfo = Directory.CreateDirectory(Path.Combine(sessionDirInfo.FullName, "Contributors"));

//                    // copy contributor files
//                    foreach (var contributor in Actors)
//                    {
//                        foreach (IMDIFile file in contributor.Files)
//                        {
//                            if (file.AccessProtocol == null)
//                                file.AccessProtocol = AccessProtocol;

//                            file.Copy(contributorDirInfo.FullName);
//                        }
//                    }
//                }
//            }

//            // copy session files
//            foreach (IMDIFile file in Files)
//            {
//                if (file.AccessProtocol == null)
//                    file.AccessProtocol = AccessProtocol;

//                file.Copy(sessionDirInfo.FullName);
//            }

//            // create session meta data file
//            CreateMetaDataFile(corpusDirectoryName, sessionDirectoryName);

//            return true;
//        }

//        private void CreateMetaDataFile(string corpusDirectoryFullName, string sessionDirectoryName)
//        {
//            var session = new Session_Type
//            {
//                Name = IMDISchemaHelper.SetString(GetName()),
//                Title = IMDISchemaHelper.SetString(GetTitle()),
//            };

//            // descriptions
//            foreach (var d in Descriptions)
//                session.Description.Add(d.ToIMDIDescriptionType());

//            // actors / contributors
//            foreach (var actor in Actors)
//                session.MDGroup.Actors.Actor.Add(actor.ToIMDIActorType());

//            // languages

//			// location
//			session.MDGroup.Location = Location.ToIMDILocationType();

//            // date
//            var sessionDate = GetDateCreated();
//            if (!string.IsNullOrEmpty(sessionDate))
//                session.Date = new DateRange_Type { Value = sessionDate };

//            // media files
//            foreach (var file in MediaFiles)
//                session.Resources.MediaFile.Add(file.ToMediaFileType(sessionDirectoryName, "/"));

//            // written resources
//            foreach (var file in WrittenResources)
//                session.Resources.WrittenResource.Add((file.ToWrittenResourceType(sessionDirectoryName, "/")));

//            // contributor files
//            var contributorsDirectoryName = sessionDirectoryName + "/Contributors";
//            foreach (IMDIContributor contributor in Actors)
//            {
//                foreach (var file in contributor.MediaFiles)
//                    session.Resources.MediaFile.Add(file.ToMediaFileType(contributorsDirectoryName, "/"));

//                foreach (var file in contributor.WrittenResources)
//                    session.Resources.WrittenResource.Add((file.ToWrittenResourceType(contributorsDirectoryName, "/")));
//            }

//            // write imdi file
//            var xmlFile = Path.Combine(corpusDirectoryFullName, sessionDirectoryName + ".imdi");
//            IMDISchemaHelper.WriteImdiFile(Metatranscript_Value_Type.SESSION, session, xmlFile);
//        }

//        private IEnumerable<IMDIFile> MediaFiles
//        {
//            get { return Files.Cast<IMDIFile>().Where(file => file.IsMediaFile).ToList(); }
//        }

//        private IEnumerable<IMDIFile> WrittenResources
//        {
//            get { return Files.Cast<IMDIFile>().Where(file => file.IsWrittenResource).ToList(); }
//        }
//    }
//}
