<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.12 to version 0.13 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<!-- change lift/@version from 0.12 to 0.13 -->

	<xsl:template match="lift/@version">
		<xsl:attribute name="version">0.13</xsl:attribute>
	</xsl:template>

	<!-- copy element entry/sense/field/[@type='LiteralMeaning'] to entry/field/[@type='literal-meaning'] -->

	<xsl:template match="entry">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
			<xsl:apply-templates/>
			<xsl:if test="sense/field[@type='LiteralMeaning']">
				<xsl:element name="field">
					<xsl:apply-templates select="sense/field/@*"/>
					<xsl:attribute name="type">literal-meaning</xsl:attribute>
					<xsl:copy-of select="sense/field[@type='LiteralMeaning']/*"/>
				</xsl:element>
			</xsl:if>
		</xsl:copy>
	</xsl:template>

	<!-- ignore element "entry/sense/field/@type="LiteralMeaning"" where it exists -->

	<xsl:template match="sense/field[@type='LiteralMeaning']"/>

	<!-- convert field types to the new values -->

	<xsl:template match="field/@type">
		<xsl:choose>
			<xsl:when test=".='literal_meaning'">
				<xsl:attribute name="type">literal-meaning</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='summary_definition'">
				<xsl:attribute name="type">summary-definition</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Comment'">
				<xsl:attribute name="type">comment</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='CVPattern'">
				<xsl:attribute name="type">cv-pattern</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='cvPattern'">
				<xsl:attribute name="type">cv-pattern</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Tone'">
				<xsl:attribute name="type">tone</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='ImportResidue'">
				<xsl:attribute name="type">import-residue</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='scientific_name'">
				<xsl:attribute name="type">scientific-name</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- fix any header values for fields -->
	<xsl:template match="field/@tag">
		<xsl:choose>
			<xsl:when test=".='literal_meaning'">
				<xsl:attribute name="tag">literal-meaning</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='summary_definition'">
				<xsl:attribute name="tag">summary-definition</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Comment'">
				<xsl:attribute name="tag">comment</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='CVPattern'">
				<xsl:attribute name="tag">cv-pattern</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='cvPattern'">
				<xsl:attribute name="tag">cv-pattern</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Tone'">
				<xsl:attribute name="tag">tone</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='ImportResidue'">
				<xsl:attribute name="tag">import-residue</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='scientific_name'">
				<xsl:attribute name="tag">scientific-name</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- convert trait names to the new values -->

	<xsl:template match="trait/@name">
		<xsl:choose>
			<xsl:when test=".='SemanticDomainDdp4'">
				<xsl:attribute name="name">semantic-domain-ddp4</xsl:attribute>
			</xsl:when>
	  <xsl:when test=".='semantic_domain'">
		<xsl:attribute name="name">semantic-domain-ddp4</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='EntryType'">
				<xsl:attribute name="name">entry-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='MorphType'">
				<xsl:attribute name="name">morph-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='MinorEntryCondition'">
				<xsl:attribute name="name">minor-entry-condition</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='ExcludeAsHeadword'">
				<xsl:attribute name="name">exclude-as-headword</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='DoNotUseForParsing'">
				<xsl:attribute name="name">do-not-use-for-parsing</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Location'">
				<xsl:attribute name="name">location</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Environment'">
				<xsl:attribute name="name">environment</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='DomainType'">
				<xsl:attribute name="name">domain-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='SenseType'">
				<xsl:attribute name="name">sense-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='UsageType'">
				<xsl:attribute name="name">usage-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Status'">
				<xsl:attribute name="name">status</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='FromPartOfSpeech'">
				<xsl:attribute name="name">from-part-of-speech</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Type'">
				<xsl:attribute name="name">type</xsl:attribute>
			</xsl:when>
				  <xsl:when test="substring(., 1, 10)='flag_skip_'">
					<xsl:attribute name="name">flag-skip-<xsl:value-of select="substring(., 11,string-length(.))"/></xsl:attribute>
				  </xsl:when>
				  <xsl:when test="substring(., string-length(.)-5)='-Slots'">
				<xsl:attribute name="name"><xsl:value-of select="substring(., 1, string-length(.)-6)"/>-slot</xsl:attribute>
			</xsl:when>
			<xsl:when test="substring(., string-length(.)-15)='-InflectionClass'">
				<xsl:attribute name="name"><xsl:value-of select="substring(., 1, string-length(.)-16)"/>-infl-class</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- fix any header values that match up with traits -->
	<xsl:template match="range/@id">
		<xsl:choose>
	  <xsl:when test=".='semantic_domain'">
				<xsl:attribute name="id">semantic-domain-ddp4</xsl:attribute>
			</xsl:when>
	  <xsl:when test=".='SemanticDomainDdp4'">
		<xsl:attribute name="id">semantic-domain-ddp4</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='EntryType'">
				<xsl:attribute name="id">entry-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='MorphType'">
				<xsl:attribute name="id">morph-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='MinorEntryCondition'">
				<xsl:attribute name="id">minor-entry-condition</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='ExcludeAsHeadword'">
				<xsl:attribute name="id">exclude-as-headword</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='DoNotUseForParsing'">
				<xsl:attribute name="id">do-not-use-for-parsing</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Location'">
				<xsl:attribute name="id">location</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Environment'">
				<xsl:attribute name="id">environment</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='DomainType'">
				<xsl:attribute name="id">domain-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='SenseType'">
				<xsl:attribute name="id">sense-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='UsageType'">
				<xsl:attribute name="id">usage-type</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Status'">
				<xsl:attribute name="id">status</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='FromPartOfSpeech'">
				<xsl:attribute name="id">from-part-of-speech</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='Type'">
				<xsl:attribute name="id">type</xsl:attribute>
			</xsl:when>
			<xsl:when test="substring(., string-length(.)-5)='-Slots'">
				<xsl:attribute name="id"><xsl:value-of select="substring(., 1, string-length(.)-6)"/>-slot</xsl:attribute>
			</xsl:when>
			<xsl:when test="substring(., string-length(.)-15)='-InflectionClass'">
				<xsl:attribute name="id"><xsl:value-of select="substring(., 1, string-length(.)-16)"/>-infl-class</xsl:attribute>
			</xsl:when>
	  <xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- This is the basic default processing. -->

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
