<?xml version="1.0"?>
<!--
Given a lift document,
Populates the definitions and reversals by harvesting from the glosses, where
there are gaps in these fields for a given writing system (@lang).
Since it is possible to have multiple glosses in a given language but not multiple definitions,
in the case of multiple glosses, they are separated by a semicolon and space ('; ')
-->
<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" >
  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:template name="copy" match="@*|node()">
	  <xsl:copy>
		  <xsl:apply-templates select="@*|node()"/>
	  </xsl:copy>
  </xsl:template>


  <xsl:template match="sense">
	<xsl:message/>
	  <sense>
		<definition>
		  <!-- first, copy in any definition forms we already had -->
			<xsl:for-each select="definition/form">
			  <xsl:call-template name="copy"></xsl:call-template>
			</xsl:for-each>

		  <!-- now add to the definition the glosses for any writing systems for which
			we don't have a definition but we do have have one or
			more glosses -->
		  <xsl:for-each select="gloss[not(parent::*/definition/form/@lang = @lang)]/@lang">
			<!-- for the first gloss with a given lang only -->
			<xsl:if test="not(parent::gloss/preceding-sibling::gloss[@lang = current()])">
			<form>
			  <xsl:copy-of select="current()"/>
			  <text>
				<!-- grab up all glosses with this ws -->
			  <xsl:for-each select="ancestor::sense/gloss[@lang = current()]/text/text()">
				<!-- and separate them with semicolons -->
				<xsl:if test="position() > 1">
				  <xsl:text>; </xsl:text>
				</xsl:if>
				<xsl:call-template name="copy"></xsl:call-template>
			  </xsl:for-each>
			  </text>
			</form>
			</xsl:if>
		  </xsl:for-each>
		</definition>

		  <!-- find any langs for which we lack reversals but we have a gloss, and
		  make reversals
		  <xsl:for-each select="gloss[not(parent::*/reversal/form/@lang = @lang)]">
				<reversal>
					<form>
					  <xsl:copy-of select="current()/@lang"/>
					   <xsl:apply-templates select="text"/>
				  </form>
				</reversal>
			 </xsl:for-each>
-->
		  <!-- now just copy over all the other sense elements.
			This will of course copy in the glosses again, which is what we want.-->
		  <xsl:apply-templates select="node()[not(self::definition)]"/>
	  </sense>
</xsl:template>

</xsl:transform>