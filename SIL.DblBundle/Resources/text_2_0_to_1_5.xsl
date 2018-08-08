<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output indent="yes"/>

    <xsl:template match="/DBLMetadata">
        <xsl:choose>
            <xsl:when test="type/medium/text() != 'text' or not(@version) or @version != '2.0'">
                <error code="bad-type-or-version">Not text 2.0 metadata</error>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy>
                    <xsl:attribute name="type">text</xsl:attribute>
                    <xsl:attribute name="typeVersion">1.5</xsl:attribute>
                    <xsl:copy-of select="@id"/>
                    <xsl:copy-of select="@revision"/>
                    <identification>
                        <xsl:call-template name="identification-content"/>
                    </identification>
                    <confidential>
                        <xsl:value-of select="type/isConfidential/text()"/>
                    </confidential>
                    <agencies>
                        <xsl:call-template name="agencies-content"/>
                    </agencies>
                    <language>
                        <xsl:call-template name="language-content"/>
                    </language>
                    <country>
                        <xsl:call-template name="country-content"/>
                    </country>
                    <type>
                        <xsl:call-template name="type-content"/>
                    </type>
                    <bookNames>
                        <xsl:call-template name="bookNames-content"/>
                    </bookNames>
                    <contents>
                        <xsl:call-template name="contents-content"/>
                    </contents>
                    <copyright>
                        <xsl:call-template name="copyright-content"/>
                    </copyright>
                    <promotion>
                        <xsl:call-template name="promotion-content"/>
                    </promotion>
                    <xsl:if test="progress/book">
                        <xsl:copy-of select="progress"/>
                    </xsl:if>
                    <archiveStatus>
                        <xsl:call-template name="archiveStatus-content"/>
                    </archiveStatus>
                    <format>
                        <xsl:text>text/xml</xsl:text>
                    </format>
                </xsl:copy>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

    <xsl:template name="identification-content">
        <name>
            <xsl:value-of select="identification/name/text()"/>
        </name>
        <nameLocal>
            <xsl:choose>
                <xsl:when test="identification/nameLocal/text()">
                    <xsl:value-of select="identification/nameLocal/text()"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="identification/name/text()"/>
                </xsl:otherwise>
            </xsl:choose>
        </nameLocal>
        <abbreviation>
            <xsl:value-of select="identification/abbreviation/text()"/>
        </abbreviation>
        <abbreviationLocal>
            <xsl:choose>
                <xsl:when test="identification/abbreviationLocal/text()">
                    <xsl:value-of select="identification/abbreviationLocal/text()"/>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:value-of select="identification/abbreviation/text()"/>
                </xsl:otherwise>
            </xsl:choose>
        </abbreviationLocal>
        <scope>
            <xsl:value-of select="identification/scope/text()"/>
        </scope>
        <description>
            <xsl:value-of select="identification/description/text()"/>
        </description>
        <dateCompleted>
            <xsl:value-of select="identification/dateCompleted/text()"/>
        </dateCompleted>
        <xsl:for-each select="identification/systemId">
            <xsl:copy>
                <xsl:copy-of select="@type"/>
                <xsl:if test="@type = 'paratext'">
                    <xsl:attribute name="name">
                        <xsl:value-of select="name/text()"/>
                    </xsl:attribute>
                    <xsl:attribute name="fullname">
                        <xsl:choose>
                            <xsl:when test="fullName/text()">
                                <xsl:value-of select="normalize-space(fullName/text())"/>
                            </xsl:when>
                            <xsl:otherwise>
                                <xsl:value-of select="normalize-space(name/text())"/>
                            </xsl:otherwise>
                        </xsl:choose>
                    </xsl:attribute>
                    <xsl:if test="csetId/text()">
                        <xsl:attribute name="csetid">
                            <xsl:value-of select="csetId/text()"/>
                        </xsl:attribute>
                    </xsl:if>
                </xsl:if>
                <xsl:value-of select="id/text()"/>
            </xsl:copy>
        </xsl:for-each>
        <bundleProducer>
            <xsl:value-of select="identification/bundleProducer/text()"/>
        </bundleProducer>
    </xsl:template>

    <xsl:template name="agencies-content">
        <xsl:for-each select="agencies/rightsHolder">
            <xsl:copy>
                <xsl:attribute name="abbr"><xsl:value-of select="abbr/text()"/></xsl:attribute>
                <xsl:if test="local">
                    <xsl:attribute name="local"><xsl:value-of select="local/text()"/></xsl:attribute>
                </xsl:if>
                <xsl:attribute name="url"><xsl:value-of select="url/text()"/></xsl:attribute>
                <xsl:attribute name="uid"><xsl:value-of select="uid/text()"/></xsl:attribute>
                <xsl:value-of select="name/text()"/>
            </xsl:copy>
        </xsl:for-each>
        <xsl:for-each select="agencies/rightsAdmin">
            <xsl:copy>
                <xsl:attribute name="url"><xsl:value-of select="url/text()"/></xsl:attribute>
                <xsl:attribute name="uid"><xsl:value-of select="uid/text()"/></xsl:attribute>
                <xsl:value-of select="name/text()"/>
            </xsl:copy>
        </xsl:for-each>
        <xsl:for-each select="agencies/contributor">
            <xsl:copy>
                <xsl:attribute name="uid"><xsl:value-of select="uid/text()"/></xsl:attribute>
                <xsl:attribute name="content"><xsl:value-of select="content/text()"/></xsl:attribute>
                <xsl:attribute name="management"><xsl:value-of select="management/text()"/></xsl:attribute>
                <xsl:attribute name="publication"><xsl:value-of select="publication/text()"/></xsl:attribute>
                <xsl:attribute name="qa"><xsl:value-of select="qa/text()"/></xsl:attribute>
                <xsl:value-of select="name/text()"/>
            </xsl:copy>
        </xsl:for-each>

    </xsl:template>

    <xsl:template name="language-content">
        <iso>
            <xsl:value-of select="language/iso/text()"/>
        </iso>
        <name>
            <xsl:value-of select="language/name/text()"/>
        </name>
        <xsl:if test="language/nameLocal">
            <nameLocal>
                <xsl:value-of select="language/nameLocal/text()"/>
            </nameLocal>
        </xsl:if>
        <ldml>
            <xsl:value-of select="language/ldml/text()"/>
        </ldml>
        <rod>
            <xsl:value-of select="language/rod/text()"/>
        </rod>
        <script>
            <xsl:value-of select="language/script/text()"/>
        </script>
        <scriptDirection>
            <xsl:value-of select="language/scriptDirection/text()"/>
        </scriptDirection>
        <numerals>
            <xsl:value-of select="language/numerals/text()"/>
        </numerals>
    </xsl:template>

    <xsl:template name="country-content">
        <iso>
            <xsl:value-of select="countries/country[1]/iso/text()"/>
        </iso>
        <name>
            <xsl:value-of select="countries/country[1]/name/text()"/>
        </name>
    </xsl:template>

    <xsl:template name="type-content">
        <translationType>
            <xsl:value-of select="type/translationType/text()"/>
        </translationType>
        <audience>
            <xsl:value-of select="type/audience/text()"/>
        </audience>
    </xsl:template>

    <xsl:template name="bookNames-content">
        <xsl:for-each select="names/name[contains(@id, 'book')]">
            <book>
                <xsl:attribute name="code"><xsl:value-of select="translate(substring(@id, 6), 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')"/></xsl:attribute>
                <abbr>
                    <xsl:value-of select="abbr/text()"/>
                </abbr>
                <short>
                    <xsl:value-of select="short/text()"/>
                </short>
                <long>
                    <xsl:value-of select="long/text()"/>
                </long>
            </book>
        </xsl:for-each>
    </xsl:template>

    <xsl:template name="contents-content">
        <xsl:for-each select="publications/publication">
            <bookList>
                <xsl:attribute name="id">
                    <xsl:choose>
                        <xsl:when test="starts-with(@id, 'p') and translate(@id, '0123456789', '') = 'p'">
                            <xsl:value-of select="substring-after(@id, 'p')"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="position()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:attribute>
                <xsl:if test="@default">
                    <xsl:attribute name="default"><xsl:text>true</xsl:text></xsl:attribute>
                </xsl:if>
                <name>
                    <xsl:choose>
                        <xsl:when test="name/text()">
                            <xsl:value-of select="name/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/name/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </name>
                <nameLocal>
                    <xsl:choose>
                        <xsl:when test="nameLocal/text()">
                            <xsl:value-of select="nameLocal/text()"/>
                        </xsl:when>
                        <xsl:when test="name/text()">
                            <xsl:value-of select="name/text()"/>
                        </xsl:when>
                        <xsl:when test="/DBLMetadata/identification/nameLocal/text()">
                            <xsl:value-of select="/DBLMetadata/identification/nameLocal/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/name/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </nameLocal>
                <abbreviation>
                    <xsl:choose>
                        <xsl:when test="abbreviation/text()">
                            <xsl:value-of select="abbreviation/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/abbreviation/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </abbreviation>
                <abbreviationLocal>
                    <xsl:choose>
                        <xsl:when test="abbreviationLocal/text()">
                            <xsl:value-of select="abbreviationLocal/text()"/>
                        </xsl:when>
                        <xsl:when test="abbreviation/text()">
                            <xsl:value-of select="abbreviation/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/abbreviation/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </abbreviationLocal>
                <description>
                    <xsl:choose>
                        <xsl:when test="description/text()">
                            <xsl:value-of select="description/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/description/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </description>
                <descriptionLocal>
                    <xsl:choose>
                        <xsl:when test="descriptionLocal/text()">
                            <xsl:value-of select="descriptionLocal/text()"/>
                        </xsl:when>
                        <xsl:when test="/description/text()">
                            <xsl:value-of select="description/text()"/>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="/DBLMetadata/identification/description/text()"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </descriptionLocal>
                <books>
                    <xsl:for-each select="structure//content[string-length(@role) = 3]">
                        <book code="{@role}"/>
                    </xsl:for-each>
                </books>
            </bookList>
        </xsl:for-each>
    </xsl:template>

    <xsl:template name="copyright-content">
        <statement contentType="xhtml">
            <xsl:copy-of select="copyright/fullStatement/statementContent/*"/>
        </statement>
    </xsl:template>

    <xsl:template name="promotion-content">
        <promoVersionInfo contentType="xhtml">
            <xsl:copy-of select="promotion/promoVersionInfo/*"/>
        </promoVersionInfo>
    </xsl:template>

    <xsl:template name="archiveStatus-content">
        <archivistName>
            <xsl:value-of select="archiveStatus/archivistName/text()"/>
        </archivistName>
        <dateArchived>
            <xsl:value-of select="archiveStatus/dateArchived/text()"/>
        </dateArchived>
        <dateUpdated>
            <xsl:value-of select="archiveStatus/dateUpdated/text()"/>
        </dateUpdated>
        <comments>
            <xsl:value-of select="archiveStatus/comments/text()"/>
        </comments>
    </xsl:template>

</xsl:stylesheet>