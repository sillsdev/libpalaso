<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.13 to version 0.11 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<!-- change lift/@version from 0.13 to 0.12 -->

	<xsl:template match="lift/@version">
		<xsl:attribute name="version">0.12</xsl:attribute>
	</xsl:template>


	<!-- convert field types to the old values -->

	<xsl:template match="field/@type">
		<xsl:choose>
	  <xsl:when test=".='literal-meaning'">
		<xsl:attribute name="type">literal_meaning</xsl:attribute>
	  </xsl:when>
			<xsl:when test=".='summary-definition'">
				<xsl:attribute name="type">summary_definition</xsl:attribute>
			</xsl:when>
			<!-- actually flex 5.4 used lower case for these
	  <xsl:when test=".='comment'">
				<xsl:attribute name="type">Comment</xsl:attribute>
			</xsl:when>

	 -->
			<xsl:when test=".='cv-pattern'">
				<xsl:attribute name="type">cvPattern</xsl:attribute>
			</xsl:when>
			<!-- actually flex 5.4 used lower case for these
			<xsl:when test=".='tone'">
				<xsl:attribute name="type">Tone</xsl:attribute>
			</xsl:when>
		-->
	<xsl:when test=".='import-residue'">
				<xsl:attribute name="type">ImportResidue</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='scientific-name'">
				<xsl:attribute name="type">scientific_name</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- fix any header values for fields -->
	<xsl:template match="field/@tag">
		<xsl:choose>
			<xsl:when test=".='literal-meaning'">
				<xsl:attribute name="tag">literal_meaning</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='summary-definition'">
				<xsl:attribute name="tag">summary_definition</xsl:attribute>
			</xsl:when>
	  <!-- actually flex 5.4 used lower case
			<xsl:when test=".='comment'">
				<xsl:attribute name="tag">Comment</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='tone'">
				<xsl:attribute name="tag">Tone</xsl:attribute>
			</xsl:when>
-->
	  <xsl:when test=".='cv-pattern'">
		<xsl:attribute name="tag">cvPattern</xsl:attribute>
	  </xsl:when>

	  <xsl:when test=".='import-residue'">
				<xsl:attribute name="tag">ImportResidue</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='scientific-name'">
				<xsl:attribute name="tag">scientific_name</xsl:attribute>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- convert trait names to the new values -->

	<xsl:template match="trait/@name">
		<xsl:choose>

	  <xsl:when test=".='semantic-domain-ddp4'">
		<xsl:attribute name="name">semantic_domain</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='semantic-domain'">
		<xsl:attribute name="name">semantic_domain</xsl:attribute>
	  </xsl:when>

	  <xsl:when test=".='entry-type'">
				<xsl:attribute name="name">EntryType</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='morph-type'">
				<xsl:attribute name="name">MorphType</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='minor-entry-condition'">
				<xsl:attribute name="name">MinorEntryCondition</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='exclude-as-headword'">
				<xsl:attribute name="name">ExcludeAsHeadword</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='do-not-use-for-parsing'">
				<xsl:attribute name="name">DoNotUseForParsing</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='location'">
				<xsl:attribute name="name">Location</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='environment'">
				<xsl:attribute name="name">Environment</xsl:attribute>
			</xsl:when>

			<xsl:when test=".='domain-type'">
				<xsl:attribute name="name">DomainType</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='sense-type'">
				<xsl:attribute name="name">SenseType</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='usage-type'">
				<xsl:attribute name="name">UsageType</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='status'">
				<xsl:attribute name="name">Status</xsl:attribute>
			</xsl:when>

	  <xsl:when test=".='from-part-of-speech'">
				<xsl:attribute name="name">FromPartOfSpeech</xsl:attribute>
			</xsl:when>
			<xsl:when test=".='type'">
				<xsl:attribute name="name">Type</xsl:attribute>
			</xsl:when>

	  <!-- TODO <xsl:when test="substring(., string-length(.)-5)='-Slots'">
				<xsl:attribute name="name"><xsl:value-of select="substring(., 1, string-length(.)-6)"/>-slot</xsl:attribute>
			</xsl:when>
			<xsl:when test="substring(., string-length(.)-15)='-InflectionClass'">
				<xsl:attribute name="name"><xsl:value-of select="substring(., 1, string-length(.)-16)"/>-infl-class</xsl:attribute>
			</xsl:when>

	  -->

			<xsl:otherwise>
				<xsl:copy/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- fix any header values that match up with traits -->
	<xsl:template match="range/@id">
		<xsl:choose>

	  <xsl:when test=".='semantic-domain-ddp4'">
		<xsl:attribute name="id">semantic_domain</xsl:attribute>
	  </xsl:when>

	  <!-- wasn't fixed in an earlier version of the 0.12 to 0.13 conveter -->
	  <xsl:when test=".='semantic-domain'">
		<xsl:attribute name="id">semantic_domain</xsl:attribute>
	  </xsl:when>

	  <xsl:when test=".='entry-type'">
		<xsl:attribute name="id">EntryType</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='morph-type'">
		<xsl:attribute name="id">MorphType</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='minor-entry-condition'">
		<xsl:attribute name="id">MinorEntryCondition</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='exclude-as-headword'">
		<xsl:attribute name="id">ExcludeAsHeadword</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='do-not-use-for-parsing'">
		<xsl:attribute name="id">DoNotUseForParsing</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='location'">
		<xsl:attribute name="id">Location</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='environment'">
		<xsl:attribute name="id">Environment</xsl:attribute>
	  </xsl:when>

	  <xsl:when test=".='domain-type'">
		<xsl:attribute name="id">DomainType</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='sense-type'">
		<xsl:attribute name="id">SenseType</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='usage-type'">
		<xsl:attribute name="id">UsageType</xsl:attribute>
	  </xsl:when>
	<!-- in the range, it was lower case
	<xsl:when test=".='status'">
		<xsl:attribute name="id">status</xsl:attribute>
	  </xsl:when>
-->
	  <xsl:when test=".='from-part-of-speech'">
		<xsl:attribute name="id">FromPartOfSpeech</xsl:attribute>
	  </xsl:when>
	  <xsl:when test=".='type'">
		<xsl:attribute name="id">Type</xsl:attribute>
	  </xsl:when>

	  <!-- TODO
	  <xsl:when test="substring(., string-length(.)-5)='-Slots'">
				<xsl:attribute name="id"><xsl:value-of select="substring(., 1, string-length(.)-6)"/>-slot</xsl:attribute>
			</xsl:when>
			<xsl:when test="substring(., string-length(.)-15)='-InflectionClass'">
				<xsl:attribute name="id"><xsl:value-of select="substring(., 1, string-length(.)-16)"/>-infl-class</xsl:attribute>
			</xsl:when>
	  -->
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
