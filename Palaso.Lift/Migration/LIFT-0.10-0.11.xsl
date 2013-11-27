<?xml version="1.0" encoding="UTF-8"?>
<!-- Convert LIFT file from version 0.10 to version 0.11 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

  <!-- This is the basic default processing. -->

  <xsl:template match="@*|node()">
	<xsl:copy>
	  <xsl:apply-templates select="@*|node()"/>
	</xsl:copy>
  </xsl:template>


	<!-- change lift/@version from 0.10 to 0.11 -->
	<xsl:template match="lift/@version">
		<xsl:attribute name="version">0.11</xsl:attribute>
	</xsl:template>

	<!-- change element "picture" to "illustration" -->
	<xsl:template match="picture">
		<illustration>
		  <xsl:copy-of select="@*"/>
		  <xsl:apply-templates/>
		</illustration>
	</xsl:template>

  <!-- change relation/@name to relation/@type -->
  <xsl:template match="relation">
	<xsl:copy>
	  <xsl:attribute name="type">
		<xsl:value-of select="@name"/>
	  </xsl:attribute>
	  <xsl:copy-of select="@*[not(name(.) = 'name')]" />
	  <xsl:apply-templates/>
	</xsl:copy>
  </xsl:template>


  <!-- change field/@tag to field/@type -->
  <xsl:template match="entry//field">
	<xsl:copy>
	  <xsl:attribute name="type">
		<xsl:value-of select="@tag"/>
	  </xsl:attribute>
	  <xsl:copy-of select="@*[not(name(.) = 'tag')]" />
	  <xsl:apply-templates/>
	</xsl:copy>
  </xsl:template>


  <!-- change form/trait to form/annotation -->
  <xsl:template match="form/trait | gloss/trait">
	<annotation>
	  <xsl:copy-of select="@*"/>
	  <xsl:apply-templates/>
	</annotation>
  </xsl:template>
</xsl:stylesheet>
