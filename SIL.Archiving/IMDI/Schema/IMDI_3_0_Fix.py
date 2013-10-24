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

# comments and using
comment = '''
//
// Generated output modified by IMDI_3_0_Fix.py
//

using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace IMDI.Schema {
'''

# function to do the replacing
def replace_text(line, loc, descr, comment):
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

	return line


# open input and output files
f1 = open('IMDI_3_0.cs', 'r')
f2 = open('IMDI_3_0_Fixed.cs', 'w')
s = f1.read()
f2.write(replace_text(s, loc, descr, comment))
f1.close()
f2.close()


# rename original file
os.rename('IMDI_3_0.cs', 'IMDI_3_0.cs.original.' + time.strftime("%Y%m%d%H%M%S"))

# rename fixed file
os.rename('IMDI_3_0_Fixed.cs', 'IMDI_3_0.cs')