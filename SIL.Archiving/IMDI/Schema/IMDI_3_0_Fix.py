###############################################################################
#
# This python script takes the output from xsd.exe and makes changes that
# allow it to be used more easily and also makes it more acceptible to
# ReSharper.
#
###############################################################################

import time
import os

# add SchemaLocation
loc = '''private string xXXFollowUpDependField;

\t\tprivate string schemaLocation = "http://www.mpi.nl/IMDI/Schema/IMDI http://www.mpi.nl/IMDI/Schema/IMDI_3.0.xsd";

\t\t/// <remarks/>
\t\t[XmlAttribute("schemaLocation", Namespace = XmlSchema.InstanceNamespace)]
\t\tpublic string SchemaLocation
\t\t{
\t\t\tget { return this.schemaLocation; }
\t\t\tset { this.schemaLocation = value; }
\t\t}
'''

# replace Description_Type[] with List<Description_Type>
descr = '''if (this.descriptionField == null)
\t\t\t\t\tthis.descriptionField = new List<Description_Type>();
\t\t\t\treturn this.descriptionField;'''

# replace Actor_Type[] with List<Actor_Type>
actor = '''if (this.actorField == null)
\t\t\t\t\tthis.actorField = new List<Actor_Type>();
\t\t\t\treturn this.actorField;'''

# replace Language_Type[] with List<Language_Type>
findLang = '''public List<Language_Type> Language {
\t\t\tget {
\t\t\t\treturn this.languageField;'''

replaceLang = '''public List<Language_Type> Language {
\t\t\tget {
\t\t\t\tif (this.languageField == null)
\t\t\t\t\tthis.languageField = new List<Language_Type>();
\t\t\t\treturn this.languageField;'''

# create MDGroupType on demand
mdGroup = '''if (this.mDGroupField == null)
\t\t\t\t\tthis.mDGroupField = new MDGroupType();
\t\t\t\treturn this.mDGroupField;'''

# replace MediaFile_Type[] with List<MediaFile_Type>
media = '''if (this.mediaFileField == null)
\t\t\t\t\tthis.mediaFileField = new List<MediaFile_Type>();
\t\t\t\treturn this.mediaFileField;'''

# replace WrittenResource_Type[] with List<WrittenResource_Type>
written = '''if (this.writtenResourceField == null)
\t\t\t\t\tthis.writtenResourceField = new List<WrittenResource_Type>();
\t\t\t\treturn this.writtenResourceField;'''

# create Actors_Type on demand
actors = '''if (this.actorsField == null)
\t\t\t\t\tthis.actorsField = new Actors_Type();
\t\t\t\treturn this.actorsField;'''

# create Languages_Type on demand
languages = '''if (this.languagesField == null)
\t\t\t\t\tthis.languagesField = new Languages_Type();
\t\t\t\treturn this.languagesField;'''

# comments and using
comment = '''
//
// Generated output modified by IMDI_3_0_Fix.py
//

using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace SIL.Archiving.IMDI.Schema
{
'''

# function to do the replacing
def replace_text(line):
	line = line.replace('    ', '\t')
	line = line.replace('private System.DateTime dateField;', 'private System.DateTime dateField = DateTime.Today;', 1)
	line = line.replace('private string formatIdField;', 'private string formatIdField = "IMDI 3.03";', 1)
	line = line.replace('private string originatorField;', 'private string originatorField = "Palaso IMDI 0.0.1";', 1)
	line = line.replace('private string versionField;', 'private string versionField = "0";', 1)
	line = line.replace('private string xXXFollowUpDependField;', loc, 1)
	line = line.replace('Description_Type[]', 'List<Description_Type>')
	line = line.replace('return this.descriptionField;', descr)
	line = line.replace('namespace IMDI.Schema {', comment)
	line = line.replace('public Vocabulary_Type() {', '/// <remarks/>\n\t\tpublic Vocabulary_Type() {')
	line = line.replace('public Boolean_Type() {', '/// <remarks/>\n\t\tpublic Boolean_Type() {')
	line = line.replace('public Quality_Type() {', '/// <remarks/>\n\t\tpublic Quality_Type() {')
	line = line.replace('Actor_Type[]', 'List<Actor_Type>')
	line = line.replace('return this.actorField;', actor)
	line = line.replace('Language_Type[]', 'List<Language_Type>')
	line = line.replace(findLang, replaceLang)
	line = line.replace('return this.mDGroupField;', mdGroup)
	line = line.replace('MediaFile_Type[]', 'List<MediaFile_Type>')
	line = line.replace('return this.mediaFileField;', media)
	line = line.replace('WrittenResource_Type[]', 'List<WrittenResource_Type>')
	line = line.replace('return this.writtenResourceField;', written)
	line = line.replace('public partial class WrittenResource_Type {', 'public partial class WrittenResource_Type : IIMDISessionFile {')
	line = line.replace('public partial class MediaFile_Type {', 'public partial class MediaFile_Type : IIMDISessionFile {')
	line = line.replace('public Vocabulary_Type Size {', 'public String_Type Size {')
	line = line.replace('private Vocabulary_Type sizeField;', 'private String_Type sizeField;')
	line = line.replace('return this.actorsField;', actors)
	line = line.replace('return this.languagesField;', languages)

	return line

# open input and output files
f1 = open('IMDI_3_0.cs', 'r')
f2 = open('IMDI_3_0_Fixed.cs', 'w')
s = f1.read()
f2.write(replace_text(s))
f1.close()
f2.close()

# rename original file
os.rename('IMDI_3_0.cs', 'IMDI_3_0.cs.original.' + time.strftime("%Y%m%d%H%M%S"))

# rename fixed file
os.rename('IMDI_3_0_Fixed.cs', 'IMDI_3_0.cs')