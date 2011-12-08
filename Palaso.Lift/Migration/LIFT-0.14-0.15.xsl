<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.14 to version 0.15 -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<xsl:comment>Converted LIFT 0.14 to 0.15 with <xsl:value-of select="system-property('xsl:vendor')"/> (<xsl:value-of select="system-property('xsl:vendor-url')"/>), XSLT version <xsl:value-of select="system-property('xsl:version')"/></xsl:comment>
		<xsl:apply-templates select="@*|node()"/>
	</xsl:template>


	<!-- change lift/@version from 0.14 to 0.15 -->

	<xsl:template match="lift/@version">
		<xsl:attribute name="version">0.15</xsl:attribute>
	</xsl:template>


	<!-- defining vern-lang-nodes and anal-lang-nodes is SLOW for large files, but I don't know of a better way to get the information. -->
	<!-- defining these globally means we pay the price only once -->

	<xsl:variable name="vern-lang" select="/lift/entry/lexical-unit/form[1]/@lang"/>
	<xsl:variable name="vern-lang-nodes" select="/lift/entry/lexical-unit/form[not(@lang=preceding::lexical-unit/form/@lang)]/@lang"/>
	<xsl:variable name="anal-lang" select="/lift/entry/sense/definition/form[1]/@lang | /lift/entry/sense/gloss[1]/@lang"/>
	<xsl:variable name="anal-lang-nodes" select="/lift/entry/sense/definition/form[not(@lang=preceding::definition/form/@lang or @lang=preceding::gloss/@lang)]/@lang | /lift/entry/sense/gloss[not(@lang=preceding::gloss/@lang or @lang=preceding::definition/form/@lang)]/@lang"/>

	<xsl:variable name="vern-langs">
		<xsl:for-each select="$vern-lang-nodes">
			<xsl:value-of select="string(.)"/>
			<xsl:if test="position()&lt;last()"><xsl:text>&#32;</xsl:text></xsl:if>
		</xsl:for-each>
	</xsl:variable>
	<xsl:variable name="anal-langs">
		<xsl:for-each select="$anal-lang-nodes">
			<xsl:value-of select="string(.)"/>
			<xsl:if test="position()&lt;last()"><xsl:text>&#32;</xsl:text></xsl:if>
		</xsl:for-each>
	</xsl:variable>

	<xsl:variable name="vern-anal-langs">
		<xsl:value-of select="$vern-langs"/>
		<xsl:for-each select="$anal-lang-nodes">
			<xsl:variable name="before" select="substring-before(string($vern-langs),string(current()))"/>
			<xsl:variable name="after" select="substring-after(string($vern-langs),string(current()))"/>
			<xsl:if test="not(contains(string($vern-langs),string(current()))) or not(($before='' or substring($before, string-length($before))=' ') and ($after='' or starts-with($after,' ')))">
				<xsl:text>&#32;</xsl:text>
				<xsl:value-of select="string(.)"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:variable>

	<xsl:variable name="anal-vern-langs">
		<xsl:value-of select="$anal-langs"/>
		<xsl:for-each select="$vern-lang-nodes">
			<xsl:variable name="before" select="substring-before(string($anal-langs),string(current()))"/>
			<xsl:variable name="after" select="substring-after(string($anal-langs),string(current()))"/>
			<xsl:if test="not(contains(string($anal-langs),string(current()))) or not(($before='' or substring($before, string-length($before))=' ') and ($after='' or starts-with($after,' ')))">
				<xsl:text>&#32;</xsl:text>
				<xsl:value-of select="string(.)"/>
			</xsl:if>
		</xsl:for-each>
	</xsl:variable>


	<!-- update field definitions -->

	<xsl:template match="field[@tag]">
		<xsl:element name="field-definition">
			<xsl:variable name="spec"><xsl:value-of select="string(.)"/></xsl:variable>
			<xsl:variable name="tag"><xsl:value-of select="@tag"/></xsl:variable>
			<!-- we can handle only one type of parent in this conversion. -->
			<xsl:variable name="parent-name">
				<xsl:choose>
					<xsl:when test="contains($spec,'Class=LexEntry')"><xsl:text>entry</xsl:text></xsl:when>
					<xsl:when test="contains($spec,'Class=LexSense')"><xsl:text>sense</xsl:text></xsl:when>
					<xsl:when test="contains($spec,'Class=MoForm')"><xsl:text>variant</xsl:text></xsl:when>
					<xsl:when test="contains($spec,'Class=LexExampleSentence')"><xsl:text>example</xsl:text></xsl:when>
					<xsl:otherwise>
						<!-- only the newest files from FieldWorks have Class= in the specification string -->
						<xsl:for-each select="//field[@type=$tag]">
							<xsl:if test="position()=1">
								<xsl:value-of select="name(parent::*)"/>
							</xsl:if>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<xsl:attribute name="name"><xsl:value-of select="@tag"/></xsl:attribute>
			<xsl:for-each select="form[@lang='x-spec' or @lang='qaa-x-spec']/text">
				<!-- FieldWorks Language Explorer was writing out custom field information in a fake language. -->
				<!-- here we have to do some fancy string processing to convert what we can to the new attributes -->
				<xsl:if test="not($parent-name='')">
					<xsl:attribute name="class"><xsl:value-of select="$parent-name"/></xsl:attribute>
				</xsl:if>
				<xsl:choose>
					<!-- these are the only types that can appear (at least for version 0.14 and earlier) -->
					<xsl:when test="contains($spec,'Type=Integer')">
						<xsl:attribute name="type">integer</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=GenDate')">
						<!-- not really datetime, but the closest match -->
						<xsl:attribute name="type">datetime</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=String')">
						<xsl:attribute name="type">string</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=MultiString')">
						<xsl:attribute name="type">multistring</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=MultiUnicode')">
						<xsl:attribute name="type">multistring</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=BigString')">
						<xsl:attribute name="type">string</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=MultiBigString')">
						<xsl:attribute name="type">multistring</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=BigUnicode')">
					</xsl:when>
					<xsl:when test="contains($spec,'Type=MultiBigUnicode')">
						<xsl:attribute name="type">multistring</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=OwningAtom') and contains($spec,'DstCls=StText')">
						<!-- not really multitext, but the closest match -->
						<xsl:attribute name="type">multitext</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=ReferenceAtom')">
						<xsl:attribute name="type">option</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=ReferenceCollection')">
						<xsl:attribute name="type">option-collection</xsl:attribute>
					</xsl:when>
					<xsl:when test="contains($spec,'Type=ReferenceSequence')">
						<xsl:attribute name="type">option-sequence</xsl:attribute>
					</xsl:when>
				</xsl:choose>
				<xsl:if test="contains($spec,'String;') or contains($spec,'Unicode;') or contains($spec,'DstCls=StText')">
					<xsl:choose>
						<xsl:when test="contains($spec,'WsSelector=kwsAnalVerns')">
							<xsl:attribute name="writing-system">
								<xsl:value-of select="$anal-vern-langs"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="contains($spec,'WsSelector=kwsVernAnals')">
							<xsl:attribute name="writing-system">
								<xsl:value-of select="$vern-anal-langs"/>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="contains($spec,'WsSelector=kwsAnals')">
							<xsl:attribute name="writing-system">
								<xsl:for-each select="$anal-lang-nodes">
									<xsl:if test="position()>1"><xsl:text>&#32;</xsl:text></xsl:if>
									<xsl:value-of select="string(.)"/>
								</xsl:for-each>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="contains($spec,'WsSelector=kwsVerns')">
							<xsl:attribute name="writing-system">
								<xsl:for-each select="$vern-lang-nodes">
									<xsl:if test="position()>1"><xsl:text>&#32;</xsl:text></xsl:if>
									<xsl:value-of select="string(.)"/>
								</xsl:for-each>
							</xsl:attribute>
						</xsl:when>
						<xsl:when test="contains($spec,'WsSelector=kwsAnal')">
							<xsl:attribute name="writing-system"><xsl:value-of select="$anal-lang"/></xsl:attribute>
						</xsl:when>
						<xsl:when test="contains($spec,'WsSelector=kwsVern')">
							<xsl:attribute name="writing-system"><xsl:value-of select="$vern-lang"/></xsl:attribute>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="writing-system">
								<xsl:choose>
									<xsl:when test="substring-before(substring-after($spec,'WsSelector='),';')">
										<xsl:value-of select="substring-before(substring-after($spec,'WsSelector='),';')"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="normalize-space(substring-after($spec,'WsSelector='))"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:for-each>
			<xsl:if test="contains($spec,' range=')">
				<xsl:attribute name="option-range">
					<xsl:choose>
						<xsl:when test="substring-before(substring-after($spec,' range='),';')">
							<xsl:value-of select="substring-before(substring-after($spec,' range='),';')"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="normalize-space(substring-after($spec,' range='))"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:attribute>
			</xsl:if>
			<xsl:text>&#xA;</xsl:text>
			<xsl:element name="description"><xsl:text>&#xA;</xsl:text>
			<xsl:for-each select="form">
				<xsl:if test="not(@lang='x-spec' or @lang='qaa-x-spec')">
					<xsl:apply-templates select="."/><xsl:text>&#xA;</xsl:text>
				</xsl:if>
			</xsl:for-each>
			</xsl:element><xsl:text>&#xA;</xsl:text>
		</xsl:element>
	</xsl:template>


	<!-- update field occurrences -->

	<xsl:template match="field[@type]">
		<xsl:copy>
			<xsl:attribute name="name"><xsl:value-of select="@type"/></xsl:attribute>
			<xsl:copy-of select="@dateCreated"/>
			<xsl:copy-of select="@dateModified"/>
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>


	<!-- This is the basic default processing. -->

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
