<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.11 to version 0.12 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

	<!-- change lift/@version from 0.11 to 0.12 -->
  <xsl:template match="lift/@version">
	<xsl:attribute name="version">0.12</xsl:attribute>
  </xsl:template>

  <!-- copy element "etymology" from "entry/sense" to entry -->
  <xsl:template match="entry">
	<xsl:copy>
	  <xsl:copy-of select="@*"/>
		<xsl:apply-templates/>
		<xsl:copy-of select="sense/etymology"/>
	  </xsl:copy>
	</xsl:template>

  <!-- ignore element "etymology" where it exists (in sense) -->
  <xsl:template match="sense/etymology"/>

	<!-- This is the basic default processing. -->
  <xsl:template match="@*|node()">
	<xsl:copy>
	  <xsl:apply-templates select="@*|node()"/>
	</xsl:copy>
  </xsl:template>

</xsl:stylesheet>
