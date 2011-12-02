<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.13 to version 0.14 -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/">
		<xsl:comment>Converted LIFT 0.13 to 0.14 with <xsl:value-of select="system-property('xsl:vendor')"/> (<xsl:value-of select="system-property('xsl:vendor-url')"/>), XSLT version <xsl:value-of select="system-property('xsl:version')"/></xsl:comment>
		<xsl:apply-templates select="@*|node()"/>
	</xsl:template>


	<!-- change lift/@version from 0.13 to 0.14 -->

	<xsl:template match="lift/@version">
		<xsl:attribute name="version">0.14</xsl:attribute>
	</xsl:template>


	<!--
		Version 0.14 never really saw use.  The only conversion needed is promoting
		some common FieldWorks produced field elements to note elements.
	-->
	<xsl:template match="field[@type='literal-meaning' or @type='cv-pattern' or @type='tone' or @type='comment' or @type='summary-definition' or @type='scientific-name']">
		<xsl:element name="note">
			<xsl:apply-templates select="@*|node()"/>
		</xsl:element>
	</xsl:template>


	<!-- Erase the corresponding field definitions. -->

	<xsl:template match="field[@tag='literal-meaning' or @tag='cv-pattern' or @tag='tone' or @tag='comment' or @tag='summary-definition' or @tag='scientific-name']"/>


	<!-- This is the basic default processing. -->

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
